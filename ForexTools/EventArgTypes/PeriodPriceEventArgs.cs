using ForexTools.Enums;
using ForexTools.Interfaces;

namespace ForexTools.EventArgTypes
{
    public class PeriodPriceEventArgs : BasePeriodPriceEventArgs
    {
        public PeriodPriceEventArgs(IBasePeriodPrice baseEventArgs, CandleStickTypeEnum type)
            : base(baseEventArgs.OpeningPrice)
        {
            MaxPrice = baseEventArgs.MaxPrice;
            MinPrice = baseEventArgs.MinPrice;
            ClosingPrice = baseEventArgs.ClosingPrice.Value;
            Type = type;
            if (ClosingPrice > OpeningPrice)
            {
                CandleColor = CandleColorEnum.Green;
            }
            else
            {
                CandleColor = CandleColorEnum.Red;
            }
        }

        public CandleStickTypeEnum Type { get; }
        public CandleColorEnum CandleColor { get; }
    }
}
