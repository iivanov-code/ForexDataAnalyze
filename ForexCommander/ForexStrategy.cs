using System;
using System.Collections.Concurrent;
using System.Threading;
using ForexCommander.Enums;
using ForexCommander.Models;
using ForexCommon.Models;

namespace ForexCommander
{
    public class ForexStrategy
    {
        private const float POSITION_PERCENTAGE = 0.01f;
        private const float ACCEPTED_DIFFERENCE_PRERCENT = 0.2f;

        public float WalletAmount { get; private set; }

        private float initialMoney;
        private readonly object padlock;
        private volatile bool loop = true;
        private ConcurrentQueue<QueueItem> pricesQueue;

        public ForexStrategy(float money)
        {
            this.initialMoney = money;
            this.WalletAmount = money;
            this.padlock = new object();
            this.pricesQueue = new ConcurrentQueue<QueueItem>();
            InitializeMoneyWatcher();
        }

        public static bool ShouldClosePosition(BidModel position, float currentPrice)
        {
            float acceptedDiff = Math.Max(currentPrice, position.Price) * ACCEPTED_DIFFERENCE_PRERCENT;
            float diff = 0;

            switch (position.Type)
            {
                case PositionType.Short:
                    diff = currentPrice - position.Price;
                    break;

                case PositionType.Long:
                    diff = position.Price - currentPrice;
                    break;
            }

            return diff > acceptedDiff;
        }

        public static float CalculateMidPrice(BidModel position, float newPrice, uint newQuantity)
        {
            return (position.Price + newPrice) / 2;
        }

        private void InitializeMoneyWatcher()
        {
            Thread backgroundWorker = new Thread(new ThreadStart(() =>
            {
            Begin:
                while (loop)
                {
                    Thread.Sleep(10);
                }

                while (!pricesQueue.IsEmpty)
                {
                    if (pricesQueue.TryDequeue(out QueueItem item))
                    {
                        item.Action(item.CurrentPrice, item.PlacedBid);
                    }
                }

                loop = true;
                goto Begin;
            }));

            backgroundWorker.IsBackground = true;
            backgroundWorker.Start();
        }

        public void SubstractFromWallet(float currentPrice, BidModel placedBid)
        {
            pricesQueue.Enqueue(new QueueItem
            {
                Action = Substract,
                CurrentPrice = currentPrice,
                PlacedBid = placedBid
            });
            loop = false;
        }

        private void Substract(float currentPrice, BidModel placedBid)
        {
            lock (padlock)
            {
                WalletAmount -= placedBid.Quantity * currentPrice;
            }
        }

        public void AddToWallet(float currentPrice, BidModel placedBid)
        {
            pricesQueue.Enqueue(new QueueItem
            {
                Action = Add,
                CurrentPrice = currentPrice,
                PlacedBid = placedBid
            });
            loop = false;
        }

        private void Add(float currentPrice, BidModel placedBid)
        {
            lock (padlock)
            {
                switch (placedBid.Type)
                {
                    case PositionType.Short:
                        {
                            WalletAmount += placedBid.Quantity * placedBid.Price;
                            WalletAmount += placedBid.Quantity * (placedBid.Price - currentPrice);
                        }
                        break;

                    case PositionType.Long:
                        {
                            WalletAmount += placedBid.Quantity * currentPrice;
                        }
                        break;
                }
            }
        }

        public uint CalculateBidQuantity(Price currentPrice, PositionType type, out float price)
        {
            lock (padlock)
            {
                if (type == PositionType.Short)
                {
                    price = currentPrice.SellPrice;
                }
                else
                {
                    price = currentPrice.BuyPrice;
                }

                float wallet = Math.Min(initialMoney, WalletAmount);

                if (WalletAmount > initialMoney * 0.5)
                {
                    float percentPrice = wallet * POSITION_PERCENTAGE;
                    float quantity = (float)Math.Floor(percentPrice / price);

                    return (uint)quantity;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class QueueItem
        {
            public float CurrentPrice { get; set; }
            public BidModel PlacedBid { get; set; }
            public Action<float, BidModel> Action { get; set; }
        }
    }
}
