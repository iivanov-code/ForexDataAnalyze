using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Interfaces;
using ForexTools.Utils;

namespace ForexTools.Trackers
{
    public class MACDTracker : BaseTrendLineTracker, ITracker
    {
        public StockStack<float> SignalLine { get; }
        public StockStack<float> Histogram { get; }

        private float signalLine;
        private float smallerEma;
        private float largerEma;

        public MACDTracker(PeriodList periodData)
            : base(periodData, MAIPeriod.MAI26)
        {
            this.SignalLine = new StockStack<float>(periodData.Capacity);
            this.Histogram = new StockStack<float>(periodData.Capacity);
        }

        protected override float ComparisonValue => SignalLine[0];

        protected override float CalculateValue(PeriodList periodData)
        {
            float prevEma = 0;
            if (Curve.Count > 0)
            {
                prevEma = Curve[0];
            }

            float macd = ForexUtils.CalculateMACD(periodData, Curve, out float histogram, ref signalLine, ref smallerEma, ref largerEma);

            this.SignalLine.Push(signalLine);
            this.Histogram.Push(histogram);
            return macd;
        }
    }
}
