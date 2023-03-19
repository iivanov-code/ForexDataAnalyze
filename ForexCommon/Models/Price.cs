using System;
using ForexCommon.Enums;

namespace ForexCommon.Models
{
    public class Price
    {
        public DateTime SignalTime { get; set; }
        public float AvgPrice { get; set; }
        public float SellPrice { get; set; }
        public float BuyPrice { get; set; }
        public StockTypeEnum StockType { get; set; }
    }
}
