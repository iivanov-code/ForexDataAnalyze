using System;
using System.Collections.Concurrent;
using ForexCommander.Enums;
using ForexCommander.Interfaces;
using ForexCommander.Models;
using ForexCommon.Enums;
using ForexCommon.EventArgModels;
using ForexTools.Enums;
using ForexTools.EventArgTypes;

namespace ForexCommander
{
    public class WalletManager
    {
        private readonly ForexStrategy strategy;
        private readonly ITraderService tradingService;
        private readonly ConcurrentDictionary<StockTypeEnum, BidModel> Positions;
        private readonly object padlock = new object();

        public float WalletAmount
        {
            get
            {
                if (!Positions.IsEmpty)
                {
                    Console.WriteLine(Positions[0].Price * Positions[0].Quantity);
                }

                return strategy.WalletAmount;
            }
        }

        public WalletManager(float initialMoney, ITraderService tradingService)
        {
            this.tradingService = tradingService;
            this.strategy = new ForexStrategy(initialMoney);
            this.Positions = new ConcurrentDictionary<StockTypeEnum, BidModel>();
        }

        internal void HandleSignals(object sender, TrackersMotionEventArgs e)
        {
            if (!Positions.ContainsKey(e.StockType))
            {
                PositionType type = Convert(e.MotionDirection);

                uint quantity = strategy.CalculateBidQuantity(e.CurrentFullPrice, type, out float price);
                if (quantity == 0) return;

                var decision = new BidModel(price, quantity, type);
                if (Positions.TryAdd(e.StockType, decision))
                {
                    decision.Price = BuySell(decision, decision.Price, e.StockType);
                }
            }
            else
            {
                BidModel position = Positions[e.StockType];

                lock (position)
                {
                    PositionType currentPostion = Convert(e.MotionDirection);
                    if (position.Type == currentPostion)
                    {
                        uint quantity = strategy.CalculateBidQuantity(e.CurrentFullPrice, currentPostion, out float price);
                        if (quantity == 0) return;

                        float newPrice = ForexStrategy.CalculateMidPrice(position, price, quantity);

                        position.Price = newPrice;

                        newPrice = BuySell(newPrice, position.Price, quantity, e.StockType, position.Type, false);

                        position.Quantity += quantity;
                        position.Price = newPrice;
                    }
                    else
                    {
                        float newPrice;
                        if (position.Type == PositionType.Long && position.Price < e.CurrentFullPrice.SellPrice)
                        {
                            newPrice = BuySell(e.CurrentFullPrice.SellPrice, position.Price, position.Quantity, e.StockType, position.Type.Negate(), true);
                        }
                        else if (position.Type == PositionType.Short && position.Price > e.CurrentFullPrice.BuyPrice)
                        {
                            newPrice = BuySell(e.CurrentFullPrice.BuyPrice, position.Price, position.Quantity, e.StockType, position.Type.Negate(), true);
                        }
                    }
                }
            }
        }

        internal void CandleOfInterestHandler(object sender, PeriodPriceEventArgs e)
        {
            //   Console.WriteLine(this.CurrentWalletMoney);
            //  Console.WriteLine(tradingService.WalletBalance());
        }

        internal void NewValueHandler(object sender, PriceEventArgs e)
        {
            if (Positions.ContainsKey(e.StockType))
            {
                BidModel position = Positions[e.StockType];
                lock (position)
                {
                    if (ForexStrategy.ShouldClosePosition(position, e.Price))
                    {
                        BuySell(e.Price, position.Price, position.Quantity, e.StockType, position.Type, true);
                    }
                }
            }
        }

        private float BuySell(BidModel price, float positionPrice, StockTypeEnum stockType, bool shouldClose = false)
        {
            return BuySell(price.Price, positionPrice, price.Quantity, stockType, price.Type, shouldClose);
        }

        private float BuySell(float price, float positionPrice, uint quantity, StockTypeEnum stockType, PositionType position, bool shouldClose = false)
        {
            switch (position)
            {
                case PositionType.Short:
                    {
                        if (shouldClose)
                        {
                            price = tradingService.SellToClose(price, stockType);
                            Positions.TryRemove(stockType, out BidModel bid);
                            strategy.AddToWallet(price, bid);

                            return price;
                        }
                        else
                        {
                            price = tradingService.SellGoods(price, quantity, stockType);
                            BidModel bid = new BidModel(price, quantity, position);
                            strategy.SubstractFromWallet(price, bid);
                            return price;
                        }
                    }
                case PositionType.Long:
                    {
                        if (shouldClose)
                        {
                            price = tradingService.BuyToClose(price, stockType);
                            Positions.TryRemove(stockType, out BidModel bid);
                            strategy.AddToWallet(price, bid);

                            return price;
                        }
                        else
                        {
                            price = tradingService.BuyGoods(price, quantity, stockType);
                            BidModel bid = new BidModel(price, quantity, position);
                            strategy.SubstractFromWallet(price, bid);
                            return price;
                        }
                    }
                default:
                    throw new ArgumentException(nameof(position));
            }
        }

        private static PositionType Convert(MotionTypeEnum motion)
        {
            switch (motion)
            {
                case MotionTypeEnum.Up:
                    return PositionType.Long;

                case MotionTypeEnum.Down:
                    return PositionType.Short;

                default:
                    throw new ArgumentException(nameof(motion));
            }
        }
    }
}
