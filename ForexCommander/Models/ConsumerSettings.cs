using ForexCommander.Interfaces;
using ForexCommon.Enums;
using ForexDataConsumer.Interfaces;
using ForexTools.Enums;

namespace ForexCommander.Models
{
    public class ConsumerSettings
    {
        public float WalletMoney { get; set; }
        public StockTypeEnum[] StockTypes { get; set; }
        public ITraderService TradingService { get; set; }
        public IConsumer Consumer { get; set; }
        public PeriodTypeEnum[] Periods { get; set; }
        public TrackerTypeEnum[] TrackerTypes { get; set; }
    }
}
