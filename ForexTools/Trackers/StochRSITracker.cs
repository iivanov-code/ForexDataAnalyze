using System.Linq;
using ForexTools.Collections;
using ForexTools.Structs;

namespace ForexTools.Trackers
{
    public class StochRSITracker : RSITracker
    {
        public StochRSITracker(PeriodList periodData)
            : base(periodData)
        { }

        protected override Percentage MIN_PERCENT => 20;
        protected override Percentage MAX_PERCENT => 80;

        private float lowestRSI = float.MaxValue;
        private float highestRSI = float.MinValue;
        private byte periodCount = 0;

        protected override float CalculateValue(PeriodList periodData)
        {
            float RSI = base.CalculateValue(periodData);

            if (periodCount < (byte)periodType)
            {
                periodCount++;
                return RSI;
            }
            else
            {
                lowestRSI = Curve.Take((int)periodType).Min();
                highestRSI = Curve.Take((int)periodType).Max();

                RSI = ((RSI - lowestRSI) / (highestRSI - lowestRSI)) * 100;
                if (RSI > 100) RSI = 100;
                if (RSI < 0) RSI = 0;

                return RSI;
            }
        }
    }
}
