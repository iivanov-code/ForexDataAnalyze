# Trading Platform Architecture — Design Spec

- **Date:** 2026-10-12
- **Status:** Approved in brainstorming, pending written-spec review
- **Scope:** Overall architecture for the rewrite of ForexDataAnalyze into a stocks + forex
  data, analysis and (simulated) trading platform. Each subsystem listed under
  [Build order](#13-build-order) gets its own detailed spec and implementation plan.

## 1. Goals and non-goals

### Goals
1. Collect price data for **US stocks/ETFs and forex** from multiple pluggable sources and store it.
2. Collect **company fundamentals** (revenue, net income, EPS, cash flow, balance sheet, …) and
   derive valuation metrics (market cap, P/E, margins, growth, …).
3. Run **algorithmic strategies** that decide what to buy, sell or **short**, and for **how long**
   (each signal carries a maximum holding period).
4. Split capital between **short-term and long-term investing** using sleeves with independent
   risk policies.
5. **Backtest** strategies on stored history with the same code that runs live.
6. Expose everything through a **Web API**.
7. Persist through **EF Core without committing to a specific RDBMS**; the target database is
   chosen later.

### Non-goals (v1)
- Real-money or real-broker execution (the `IBroker` seam exists; only `PaperBroker` is built).
- Tick/quote storage or streaming; the smallest unit is the 1-minute bar.
- Non-US stock markets (the model supports them; no providers are implemented).
- Parameter optimisation and walk-forward analysis.
- Pyramiding (adding to an existing position in the same direction).
- Candlestick-pattern recognition.
- Multi-user accounts, JWT/OIDC auth, rate limiting for external consumers.
- Any UI beyond the generated OpenAPI documentation page.

## 2. Decisions log

| # | Decision | Choice |
|---|---|---|
| D1 | Existing code | Deleted. The three CSV files are kept as test fixtures. |
| D2 | API consumers | Only the owner. Single API key; JWT can replace it later. |
| D3 | Trading depth | Built-in paper broker + backtester; `IBroker` seam for real brokers later. |
| D4 | Price granularity | 1-minute and daily bars stored; other timeframes aggregated on read. |
| D5 | Markets | US stocks first; the model is multi-exchange and multi-currency. |
| D6 | Data budget | Free tiers first; providers are pluggable and rate-limited. |
| D7 | Capital split | One portfolio, multiple sleeves, each with its own cash and risk policy. |
| D8 | Architecture style | Modular monolith: one ASP.NET Core host, one project per module. |
| D9 | Persistence | EF Core, provider-agnostic. No migrations until the RDBMS is chosen. InMemory provider for local runs; SQLite in-memory for tests. |

## 3. Architecture overview

```
Providers (prices, fundamentals) ──► Ingestion workers ──► TradingDbContext (EF Core)
                                          │ BarsIngested                │
                                          ▼                             ▼
                                   StrategyRunner ──► Signals     Web API (/api/v1)
                                          │
                                          ▼
                     RiskManager ──► Orders ──► IBroker (PaperBroker) ──► Fills
                                          ▲                                 │
                                          └──── PositionMonitor ◄── Positions / Ledger
```

### 3.1 Solution layout

```
Trading.slnx
Directory.Build.props        net10.0, Nullable=enable, ImplicitUsings, TreatWarningsAsErrors
Directory.Packages.props     central package versions
src/
  Trading.Core               shared kernel: Instrument, Exchange, Bar, Timeframe, Money, IClock,
                             IPriceLookup, FundamentalsSnapshot
  Trading.MarketData         price providers, ingestion workers, BarAggregator, bar queries
  Trading.Fundamentals       company model, fundamentals providers, metrics snapshots
  Trading.Analysis           indicators, strategy contract, reference strategies (pure, no I/O)
  Trading.Execution          strategy instances + StrategyRunner, portfolio, sleeves, ledger,
                             risk, orders, positions, IBroker, PaperBroker
  Trading.Backtesting        replay feed, backtest engine, metrics
  Trading.Persistence        TradingDbContext, entity configurations, store implementations
  Trading.Api                ASP.NET Core host: endpoints, workers, DI wiring, auth
tests/
  Trading.<Module>.Tests     one xUnit project per module
  fixtures/                  CSV and recorded-HTTP fixtures
```

### 3.2 Dependency rules

- `Trading.Core` depends on nothing. It holds the few contracts several modules share:
  `IPriceLookup` (implemented by `MarketData`) and the `FundamentalsSnapshot` read model
  (produced by `Fundamentals`, consumed by strategies).
- `Trading.Analysis` depends only on `Core`. It has no EF, no HTTP and no clock access beyond
  the `StrategyContext` it is given.
- `MarketData` and `Fundamentals` depend only on `Core`. `Fundamentals` gets prices through
  `IPriceLookup`, not by referencing `MarketData`.
- `Execution` depends on `Core` and `Analysis`. It owns `StrategyInstance` and the
  `StrategyRunner`, because running strategies means persisting signals and creating orders.
- Each module declares the **store interfaces** it needs (e.g. `IBarStore`).
- `Trading.Persistence` depends on the modules and implements their store interfaces. Modules
  never reference EF Core types.
- `Trading.Backtesting` depends on `Core`, `MarketData` (read side), `Fundamentals` (read side),
  `Analysis` and `Execution`.
- `Trading.Api` is the composition root and the only project that references everything.

## 4. Core model (`Trading.Core`)

- **Money and prices** use `decimal`. **All timestamps are UTC** (`DateTime` with `Kind=Utc`,
  enforced by EF value converters).
- **`Instrument`**: `Id`, `Symbol`, `AssetClass` (`Stock | Etf | Forex`), `ExchangeId`,
  `QuoteCurrency`, `BaseCurrency?` (forex only), `TickSize`, `LotSize`, `IsShortable`,
  `ListedFrom?`, `DelistedAt?`, `IsTracked`.
- **`Exchange`**: `Id`, `Mic` (e.g. `XNAS`; `FXSPOT` for forex), `TimeZoneId`, trading sessions.
  Forex is modelled as a 24×5 session.
- **`InstrumentAlias`**: `(Provider, ProviderSymbol) → InstrumentId`, unique per provider.
- **`Bar`**: key `(InstrumentId, Timeframe, OpenTimeUtc)`; `Open`, `High`, `Low`, `Close`,
  `Volume?` (nullable: forex often has none), `Source`.
- **`Timeframe`**: `M1, M5, M15, M30, H1, H4, D1, W1, MN1`. Only `M1` and `D1` are persisted.
- **`Money`**: `(decimal Amount, Currency Currency)`. Arithmetic across currencies throws.
- **`IClock`**: `SystemClock` (live) and `SimulatedClock` (backtests). No code reads
  `DateTime.Now`/`UtcNow` directly.
- **`IPriceLookup`**: `GetCloseAtOrBeforeAsync(instrumentId, timeUtc)` and
  `GetFxRateAsync(fromCurrency, toCurrency, timeUtc)`. Implemented in `MarketData` from stored
  bars; used by `Fundamentals` (market cap, P/E) and `Execution` (currency conversion).
- **`FundamentalsSnapshot`**: an immutable record of point-in-time metrics (see §7.2), defined
  here so strategies can read fundamentals without referencing `Fundamentals`.

## 5. Persistence (`Trading.Persistence`)

- One `TradingDbContext` applies one `IEntityTypeConfiguration<T>` per entity.
- Registered by `services.AddTradingPersistence(Action<DbContextOptionsBuilder> configure)`.
  The host picks the provider. The library references only `Microsoft.EntityFrameworkCore`
  (plus `Relational` for shared configuration), never a concrete provider.
- **Portability rules:**
  - Explicit `HasPrecision` on every decimal (prices `(18,8)`, money `(19,4)`).
  - Explicit max lengths on strings.
  - No raw SQL, no provider-specific column types, no `rowversion`.
  - Optimistic concurrency uses a `Guid Version` property marked `IsConcurrencyToken()`,
    regenerated in `SaveChanges`.
  - UTC value converters on all `DateTime` properties.
- **Migrations are not created** until the target RDBMS is chosen, because EF migrations are
  provider-specific.
- **Local runs:** EF Core InMemory provider (nothing written to disk).
  **Tests:** SQLite in-memory (`DataSource=:memory:`, connection kept open per test), which is
  relational and enforces keys and constraints.
- Bars are written through an **upsert** keyed by `(InstrumentId, Timeframe, OpenTimeUtc)`,
  implemented with portable EF operations (load existing keys for the batch range, then
  insert or update).

## 6. Market data (`Trading.MarketData`)

### 6.1 Provider contract
```csharp
public interface IMarketDataProvider
{
    string Name { get; }
    ProviderCapabilities Capabilities { get; } // asset classes, timeframes, max history, rate limit
    IAsyncEnumerable<Bar> GetBarsAsync(InstrumentRef instrument, Timeframe timeframe,
                                       DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}
```

### 6.2 Implementations (v1)
1. **`CsvBarProvider`**: configurable column mapping (date/time columns, OHLC(V) order), date
   format, source time zone, header/no-header. Parses numbers with `CultureInfo.InvariantCulture`
   and sorts output oldest-first. It must handle all three files in `tests/fixtures/`.
2. **One free online provider** covering US stocks and forex at 1-minute granularity. The
   concrete provider is chosen in sub-project 2's spec after checking current free-tier terms
   (candidates: Twelve Data, Finnhub, Alpha Vantage).

### 6.3 Ingestion
- **Backfill job** (triggered via API) fetches a historical range in provider-sized chunks.
- **`IncrementalSyncWorker`** (`BackgroundService`) fetches from the last stored bar to now for
  each tracked instrument, during that instrument's sessions.
- **`IngestionCursor`** row per `(InstrumentId, Timeframe, Provider)`: `LastBarTimeUtc`,
  `LastRunUtc`, `LastStatus`, `LastError`. Restarts resume from the cursor.
- **Provider priority** is configured per asset class. The first provider that returns data for
  a range wins; bars from different sources are never merged.
- **Resilience:** `IHttpClientFactory` with the standard resilience handler (retry with
  exponential backoff on 429/5xx, timeouts). A per-provider `RateLimiter` keeps calls within quota.
- **Validation** before storing: `High ≥ max(Open, Close)`, `Low ≤ min(Open, Close)`, all prices
  > 0, `OpenTimeUtc` not in the future. Invalid bars are logged and skipped. Gaps are recorded
  in the log, never filled synthetically.
- After a batch is saved, a `BarsIngested(InstrumentId, Timeframe, UpToUtc)` event is written
  to an in-process `Channel<BarsIngested>`.

### 6.4 Aggregation and queries
- **`BarAggregator`** (pure function) builds `M5…H4` from `M1` and `W1`/`MN1` from `D1`. Bucket
  boundaries come from bar timestamps aligned in the exchange time zone, never from timers.
- **`IBarQueryService.GetBarsAsync(instrument, timeframe, fromUtc, toUtc, maxBars)`** returns
  stored or aggregated bars. A bucket that is still forming is only returned when the caller
  asks for it explicitly.

## 7. Fundamentals (`Trading.Fundamentals`)

### 7.1 Model
- **`Company`**: `Id`, `Name`, `Cik?`, `Sector`, `Industry`, `Country`, `ReportingCurrency`,
  plus one or more listing `Instrument`s.
- **`FinancialReport`**: `Id`, `CompanyId`, `FiscalYear`, `FiscalPeriod` (`Q1..Q4 | FY`),
  `PeriodEndDate`, `Form` (`10-K | 10-Q | …`), **`FiledAtUtc`**, `Source`. Restatements are
  separate rows with later `FiledAtUtc`.
- **`FinancialLineItem`**: `ReportId`, `Concept`, `Value`, `Unit`, `SourceTag`.
- **`Concept`** (normalised): `Revenue`, `CostOfRevenue`, `GrossProfit`, `OperatingIncome`,
  `NetIncome`, `EpsBasic`, `EpsDiluted`, `TotalAssets`, `TotalLiabilities`, `ShareholdersEquity`,
  `CashAndEquivalents`, `TotalDebt`, `OperatingCashFlow`, `CapitalExpenditure`,
  `SharesOutstanding`, `DividendsPerShare`.
- **`ConceptMapping`**: `(Provider, SourceTag) → Concept`, e.g. both `us-gaap:Revenues` and
  `us-gaap:RevenueFromContractWithCustomerExcludingAssessedTax` map to `Revenue`.

### 7.2 Point-in-time metrics
`IFundamentalsService.GetSnapshotAsync(companyId, asOfUtc)`:
- Uses only reports with `FiledAtUtc ≤ asOfUtc`, taking the latest filing per period.
- Combines them with the closing price on or before `asOfUtc`.
- Computes, on read (never stored): market cap, P/E (TTM), P/B, EV/EBITDA, gross/operating/net
  margin, YoY revenue and EPS growth, ROE, debt/equity, free cash flow.

### 7.3 Provider
- `IFundamentalsProvider`, implemented in v1 by **`SecEdgarProvider`**. It uses SEC's company
  facts JSON and the ticker→CIK mapping, sends a descriptive `User-Agent` with contact details
  (configured), and stays under SEC's request-rate limit.
- `FundamentalsSyncWorker` checks tracked companies for new filings once a day.

## 8. Analysis and strategies (`Trading.Analysis`)

### 8.1 Indicators
```csharp
public interface IIndicator<out T>
{
    void Update(Bar bar);
    bool IsReady { get; }
    T Value { get; }
}
```
v1 set: `Sma`, `Ema`, `Macd` (line/signal/histogram), `Rsi` (Wilder smoothing), `StochRsi`
(over raw RSI history), `Atr`. Each is verified against published reference values in tests.

### 8.2 Strategy contract
```csharp
public interface IStrategy
{
    StrategyHorizon Horizon { get; }               // ShortTerm | LongTerm
    IReadOnlyList<Timeframe> Timeframes { get; }
    int WarmupBars { get; }
    IEnumerable<Signal> Evaluate(StrategyContext context);
}
```
- **`StrategyContext`**: `NowUtc`, the instrument, **completed bars only** (close time ≤ `NowUtc`)
  per requested timeframe, a `FundamentalsSnapshot?` as of `NowUtc` (pre-loaded by the runner;
  `null` for forex or when no filings exist), and the strategy's current position in its own
  sleeve. The context is fully built before `Evaluate` is called, so strategies stay synchronous
  and free of I/O.
- **`Signal`**: `InstrumentId`, `Direction` (`Long | Short | Exit`), `Confidence` (0–1),
  `SuggestedStop?`, `SuggestedTarget?`, `MaxHoldingPeriod`, `Reason`, `GeneratedAtUtc`.
- Strategies never size positions or place orders. Combining indicators happens inside a
  strategy.

### 8.3 Reference strategies
- **`TrendPullback`** (short-term): M15/H1, stocks and forex, long and short, ATR-based stop,
  max holding period of hours to days.
- **`FundamentalMomentum`** (long-term): monthly rebalance. Ranks US stocks by fundamentals
  (earnings growth, sensible P/E, positive operating cash flow), filtered by close above the
  200-day SMA; holding period of weeks to months. For forex it uses a D1/W1 trend-following
  variant.

### 8.4 Strategy instances and runner (implemented in `Trading.Execution`)
- **`StrategyInstance`** (persisted): `Id`, `StrategyType`, `ParametersJson`, `SleeveId`,
  instrument list, `Enabled`, `ConsecutiveFailures`, `DisabledReason?`.
- **`StrategyRunner`** reads `BarsIngested` (live) or is called by the replay loop (backtest). When a timeframe used by an enabled instance has
  closed, it builds the context, calls `Evaluate`, saves every `Signal`, and passes each signal
  to Execution.
- After **N** consecutive exceptions (configurable, default 3) an instance is disabled and the
  reason is stored.

## 9. Execution (`Trading.Execution`)

### 9.1 Portfolio, sleeves and ledger
- **`Portfolio`**: `BaseCurrency` (default USD) and `Sleeve`s.
- **`Sleeve`**: `Name`, `Horizon`, `RiskPolicy`, `Halted`, `HaltReason?`.
- **`RiskPolicy`**: `RiskPerTradePct`, `MaxPositionPct`, `MaxOpenPositions`,
  `MaxGrossExposurePct`, `AllowShort`, `MaxLeverage`, `MaxDrawdownPct`,
  `AllowedAssetClasses`.
- **Ledger:** append-only `CashEntry` rows (`Deposit`, `Withdrawal`, `SleeveTransfer`,
  `FillDebit`, `FillCredit`, `Commission`, `BorrowFee`, `Financing`, `FxConversion`). Sleeve
  cash is the sum of its entries; there is no mutable balance field.

### 9.2 Orders, fills, positions
- **`Order`**: `Id`, `ClientOrderId` (unique, for idempotency), `SleeveId`, `InstrumentId`,
  `Side`, `Quantity`, `Type` (`Market | Limit | Stop`), `LimitPrice?`, `StopPrice?`,
  `Status` (`Pending | Submitted | PartiallyFilled | Filled | Cancelled | Rejected`),
  `SignalId?`, `ParentOrderId?` (brackets), `OcoGroupId?`, `BrokerOrderId?`, `Version`.
- **`Fill`**: `OrderId`, `Quantity`, `Price`, `Commission`, `TimeUtc`.
- **`Position`**: unique `(SleeveId, InstrumentId)`. **Signed `Quantity`** (> 0 long, < 0 short),
  quantity-weighted `AverageEntryPrice`, `RealizedPnl`, `StopPrice?`, `TargetPrice?`,
  `OpenedAtUtc`, `MaxHoldUntilUtc?`, `Version`. It can always be rebuilt from its fills.

### 9.3 Signal → order flow
1. `RiskManager.Evaluate(signal, sleeveState)` returns an `OrderRequest` or a `RiskRejection`
   with a reason. Rejections are stored.
2. **Sizing:** `qty = sleeveEquity × RiskPerTradePct / |entry − stop|`, capped by
   `MaxPositionPct`, leverage and cash/margin, rounded down to `LotSize`. A zero result means
   rejection.
3. **Opposite-direction** signal: close the position, then open the new one (two orders).
   **Same-direction** signal while already in a position: ignored. **`Exit`**: close.
4. Entries are submitted as a **bracket**: entry plus protective stop and target, linked by
   `OcoGroupId` (one fills → the other is cancelled).
5. **`PositionMonitor`** closes positions past `MaxHoldUntilUtc` and halts a sleeve whose
   drawdown exceeds `MaxDrawdownPct`. A halted sleeve rejects new entries but still manages exits.
   It is resumed through the API.
6. Each portfolio's execution events go through a **single-reader channel**, so there are no
   locks in execution code.

### 9.4 Shorting
- **Stocks:** allowed only if `Instrument.IsShortable` and `RiskPolicy.AllowShort`. Short
  proceeds are not counted as available cash. A configurable margin requirement applies
  (default 150% of short market value), and a configurable annual borrow rate accrues daily.
- **Forex:** long and short are symmetric. Margin = notional ÷ `MaxLeverage`, and a configurable
  daily financing rate applies per direction.

### 9.5 Brokers
```csharp
public interface IBroker
{
    Task<BrokerOrderAck> SubmitAsync(OrderRequest request, CancellationToken ct);
    Task CancelAsync(Guid orderId, CancellationToken ct);
    Task<IReadOnlyList<OrderSnapshot>> GetOpenOrdersAsync(CancellationToken ct);
    IAsyncEnumerable<FillEvent> Fills(CancellationToken ct);
}
```
**`PaperBroker`** is driven by bars through `OnBarAsync(Bar)`. Live, it receives each newly
ingested M1 bar; in backtests, the replay feed supplies them. It never looks at wall-clock time.

Fill rules (all costs configurable per asset class):
- **Market orders** fill at the **next bar's open** ± half spread ± slippage, never on the bar
  that produced the signal.
- **Limit orders** fill when the bar trades through the limit, at the limit or better (the open
  if it gaps through).
- **Stop orders** trigger when the bar touches the stop and fill at the stop, or at the open if
  the bar gaps past it.
- If stop and target are both touched in the same bar, the **stop is assumed to fill first**
  (conservative).
- **Commission:** per share (stocks); spread only (forex).

### 9.6 Currency
P&L for instruments not quoted in the portfolio currency is converted using the latest stored
forex close at or before the event time. A missing rate is an error, not a silent 1.0.

## 10. Backtesting (`Trading.Backtesting`)

- **No separate trading logic:** the engine combines the real `StrategyRunner`, `RiskManager`,
  `PositionMonitor` and `PaperBroker` with a `SimulatedClock`, a `BarReplayFeed` and in-memory
  execution state (live portfolio tables are never touched).
- **`BarReplayFeed`** reads stored bars through `IBarStore` and merges all instruments into one
  time-ordered stream (priority queue on bar close time).
- **Per time step:** advance clock → broker fills pending orders against the new bar → accrue
  financing and borrow fees → update indicators → evaluate strategies whose timeframe closed →
  risk checks → new orders queued for the next bar.
- **`BacktestDefinition`**: strategy instances and parameters, universe, sleeves with policies
  and initial capital, date range, cost model.
- **`BacktestRun`** (persisted): definition JSON, `Status` (`Queued | Running | Completed | Failed`),
  timestamps, error, metrics; plus `BacktestEquityPoint` (daily) and `BacktestTrade` rows.
- **Metrics** per portfolio, sleeve and strategy: total return, CAGR, max drawdown, Sharpe,
  Sortino, win rate, profit factor, average holding period, exposure (time in market), trade count.
- Runs are queued through a `Channel` and executed by `BacktestWorker`.
- **Deterministic:** no randomness; the same definition and data give identical results.
- **Known limitation:** free data sources rarely include delisted stocks, so stock backtests
  have survivorship bias. `ListedFrom`/`DelistedAt` let the universe be filtered correctly
  once such data is available.

## 11. Web API (`Trading.Api`)

- Minimal APIs, one endpoint group per module, prefix `/api/v1`.
- Built-in OpenAPI document plus an interactive docs UI.
- Errors returned as RFC 7807 `ProblemDetails`.
- **Auth:** `X-Api-Key` header checked by middleware against configuration; disabled in the
  `Development` environment.
- **Response limits:** bar endpoints enforce `maxBars` (default 5,000).

| Area | Endpoints |
|---|---|
| Instruments | `GET /instruments?assetClass=&search=` · `GET /instruments/{symbol}` · `POST /instruments` |
| Prices | `GET /instruments/{symbol}/bars?timeframe=&from=&to=` · `GET /instruments/{symbol}/indicators/{name}?timeframe=&period=` |
| Ingestion | `POST /ingestion/backfill` · `GET /ingestion/status` |
| Companies | `GET /companies/{ticker}` · `GET /companies/{ticker}/financials?period=annual\|quarterly` · `GET /companies/{ticker}/metrics?asOf=` |
| Strategies | `GET /strategies` · `POST /strategies` · `PUT /strategies/{id}` · `POST /strategies/{id}/enable` · `POST /strategies/{id}/disable` · `GET /signals?strategyId=&from=` |
| Portfolio | `GET /portfolio` · `GET /sleeves` · `POST /sleeves/{id}/transfer` · `POST /sleeves/{id}/resume` · `GET /positions` · `GET /orders` · `POST /orders` · `DELETE /orders/{id}` · `GET /trades` |
| Backtests | `POST /backtests` · `GET /backtests/{id}` · `GET /backtests/{id}/equity` · `GET /backtests/{id}/trades` |
| Ops | `GET /health` (database and per-provider status) |

Endpoints contain no business logic; they call module services (`IBarQueryService`,
`IFundamentalsService`, `IPortfolioService`, `IBacktestService`, …).

## 12. Cross-cutting

### 12.1 Configuration
Strongly typed options per module (`MarketData`, `Fundamentals`, `Execution:PaperBroker`,
`Execution:Risk`, `Api`) with `ValidateDataAnnotations().ValidateOnStart()`. Secrets (provider
API keys, the API key, the SEC contact e-mail) come from user-secrets in Development and from
environment variables elsewhere. No secrets in source control.

### 12.2 Error handling
| Situation | Behaviour |
|---|---|
| Provider call fails | Only that instrument/range is affected; the cursor records the error; the worker backs off and continues. |
| Background loop throws | Caught at loop level, logged, retried after a delay; the host never crashes. |
| Strategy throws | Logged; instance disabled after N consecutive failures with the reason stored. |
| Domain rule violated (e.g. short not allowed, insufficient cash) | Domain exception → `400`/`409` `ProblemDetails`. |
| Concurrency conflict on `Order`/`Position` | `DbUpdateConcurrencyException` → reload and retry once, then `409`. |
| Unexpected exception | Logged with context → `500` `ProblemDetails`, without internal details. |

### 12.3 Logging
Structured logging through `ILogger<T>`, with scopes that include instrument, strategy
instance, sleeve and backtest run id where relevant.

### 12.4 Testing
- **Unit:** indicators (reference values), `BarAggregator`, CSV parsing for all three fixtures,
  bar validation, fundamentals concept mapping and point-in-time snapshot selection, position
  maths (open/add/reduce/flip, long and short), risk sizing and rejections, `PaperBroker` fill
  rules (including gaps and same-bar stop/target), ledger balances, backtest metrics.
- **Integration:** persistence and stores on SQLite in-memory; API endpoints via
  `WebApplicationFactory` with SQLite in-memory; providers against recorded HTTP responses
  (no network access in tests).
- **Golden backtest:** a fixed fixture dataset and strategy configuration, with results
  compared to a stored snapshot to guarantee determinism.

## 13. Build order

Each item is a separate sub-project with its own spec → plan → implementation cycle:

1. **Foundation:** delete old projects; solution and build props; `Core`; `Persistence`;
   `CsvBarProvider`; `BarAggregator`; instruments and bars endpoints; test projects.
2. **Online market data:** choose and implement the free provider; backfill; `IncrementalSyncWorker`;
   ingestion endpoints.
3. **Fundamentals:** company model; `SecEdgarProvider`; snapshots; companies endpoints.
4. **Analysis:** indicators; strategy contract; reference strategies; indicator endpoint.
5. **Execution:** portfolio, sleeves, ledger, risk, orders, positions, `PaperBroker`; portfolio endpoints.
6. **Backtesting:** replay feed, engine, metrics, backtest endpoints, golden test.
7. **Live paper trading:** `StrategyRunner` wired to ingestion events and execution end-to-end;
   strategy and signal endpoints.
