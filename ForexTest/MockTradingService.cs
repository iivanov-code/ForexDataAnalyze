using System;
using ForexCommander.Interfaces;
using ForexCommon.Enums;

namespace ForexTest
{
    public class MockTradingService : ITraderService
    {
        public float TotalWalletValue { get; set; }

        private Tuple<float, float> LastOrder;

        public float BuyToClose(float price, StockTypeEnum stockType)
        {
            Console.WriteLine("BUY TO CLOSE " + price);
            if (LastOrder == null)
            {
                throw new ArgumentException("No position made to Close it!");
            }
            else
            {
                float diff = (LastOrder.Item1 - price);

                TotalWalletValue += LastOrder.Item2 * diff;
                LastOrder = null;
            }
            return price;
        }

        public float BuyGoods(float price, uint quantity, StockTypeEnum stockType)
        {
            Console.WriteLine("BUY SINGLE PRICE: " + price);

            if (LastOrder == null)
            {
                LastOrder = new Tuple<float, float>(price, quantity);
                TotalWalletValue -= price * quantity;
            }
            else
            {
                float totalQuantity = LastOrder.Item2 + quantity;
                TotalWalletValue -= price * quantity;
                float newPrice = (LastOrder.Item1 * LastOrder.Item2 + price * (float)quantity) / totalQuantity;
                LastOrder = new Tuple<float, float>(newPrice, totalQuantity);
            }

            return price;
        }

        public float SellToClose(float price, StockTypeEnum stockType)
        {
            Console.WriteLine("SELL AND CLOSE " + price);

            if (LastOrder == null)
            {
                throw new ArgumentException("No position made to Close it!");
            }
            else
            {
                float diff = (price - LastOrder.Item1);

                TotalWalletValue += LastOrder.Item2 * diff;
                LastOrder = null;
            }

            return price;
        }

        public float SellGoods(float price, uint quantity, StockTypeEnum stockType)
        {
            Console.WriteLine("SELL SINGLE PRICE: " + price);
            if (LastOrder == null)
            {
                LastOrder = new Tuple<float, float>(price, quantity);
                TotalWalletValue -= price * quantity;
                return price;
            }
            else
            {
                float totalQuantity = LastOrder.Item2 + quantity;
                TotalWalletValue -= price * quantity;
                float newPrice = (LastOrder.Item1 * LastOrder.Item2 + price * quantity) / totalQuantity;
                LastOrder = new Tuple<float, float>(newPrice, totalQuantity);
                return newPrice;
            }
        }

        public float WalletBalance()
        {
            return TotalWalletValue;
        }
    }
}
