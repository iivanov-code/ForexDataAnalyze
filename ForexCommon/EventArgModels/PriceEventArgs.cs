using ForexCommon.Enums;

namespace ForexCommon.EventArgModels
{
    public class PriceEventArgs
    {
        public float Price { get; set; }
        public StockTypeEnum StockType { get; set; }
    }
}
