using System;
using System.Collections.Generic;
using System.Linq;
using ForexCommander.Interfaces;
using ForexCommander.Models;
using ForexCommon;
using ForexCommon.Enums;
using ForexDataConsumer.Interfaces;
using ForexTools;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Interfaces;

namespace ForexCommander
{
    public partial class Commander
    {
        public static WalletManager walletManager;
        private static ITraderService tradingService;
        private static HashSet<StockTypeEnum> Stockes = new HashSet<StockTypeEnum>();

        public static Commander GetWallet(float money, StockTypeEnum type, ITraderService tradingService, IConsumer consumer)
        {
            TrackerTypeEnum[] trackers = new TrackerTypeEnum[]
            {
                TrackerTypeEnum.MACD,
                TrackerTypeEnum.EMA,
                TrackerTypeEnum.SMA
            };

            PeriodTypeEnum[] periodTypes = new PeriodTypeEnum[]
            {
                PeriodTypeEnum.Second
            };

            return GetWallet(money, type, tradingService, consumer, periodTypes, trackers);
        }

        public static Commander GetWallet(float money, StockTypeEnum stockType, ITraderService tradingService, IConsumer consumer, PeriodTypeEnum[] periods, TrackerTypeEnum[] trackerTypes)
        {
            return GetCommander(new ConsumerSettings
            {
                WalletMoney = money,
                Consumer = consumer,
                Periods = periods,
                StockTypes = new StockTypeEnum[] { stockType },
                TrackerTypes = trackerTypes,
                TradingService = tradingService
            });
        }

        public static Commander GetCommander(ConsumerSettings settings)
        {
            if (tradingService == null)
            {
                tradingService = settings.TradingService;
            }

            if (walletManager == null)
            {
                walletManager = new WalletManager(settings.WalletMoney, tradingService);
            }

            foreach (var stockType in settings.StockTypes)
            {
                if (Stockes.Contains(stockType))
                {
                    throw new ArgumentException("Already has this stock type: " + stockType, nameof(stockType));
                }
            }

            List<ForexStockManager> stockManagers = new List<ForexStockManager>();
            foreach (var stockType in settings.StockTypes)
            {
                IPeriodsCollectionManager periodsManager = null;
                var tempPeriodsManager = PeriodsCollectionManager
                  .GetManager(stockType);

                foreach (var period in settings.Periods.Take(settings.Periods.Length - 1))
                {
                    tempPeriodsManager.AddPeriod(period);
                }

                periodsManager = tempPeriodsManager.AddPeriod(settings.Periods.Last());

                ForexStockManager forexManager = new ForexStockManager(periodsManager, settings.TrackerTypes);
                TestMethodConnection.EndPeriod = periodsManager.ForceEndPeriod;
                stockManagers.Add(forexManager);
                settings.Consumer.AddPrice = forexManager.AddPrice;
                forexManager.Signal += walletManager.HandleSignals;
                forexManager.NewValue += walletManager.NewValueHandler;
                forexManager.CandleOfInterest += walletManager.CandleOfInterestHandler;
            }

            return new Commander(stockManagers, settings.Consumer);
        }
    }
}
