using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Utils;

namespace ForexTools.Trackers
{
    public class EMATracker : BaseTrendLineTracker
    {
        public EMATracker(PeriodList periodData)
            : this(periodData, MAIPeriod.MAI8)
        { }

        public EMATracker(PeriodList periodData, MAIPeriod periodType)
            : base(periodData, periodType)
        { }

        protected override float CalculateValue(PeriodList periodData)
        {
            float prevEma = 0;
            if (Curve.Count > 0)
            {
                prevEma = Curve[0];
            }

            return ForexUtils.CalculateEMA(periodData, periodType, prevEma);
        }
    }
}
