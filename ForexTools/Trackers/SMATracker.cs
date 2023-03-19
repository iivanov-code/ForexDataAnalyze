using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Utils;

namespace ForexTools.Trackers
{
    public class SMATracker : BaseTrendLineTracker
    {
        public SMATracker(PeriodList periodData)
            : this(periodData, MAIPeriod.MAI8)
        { }

        public SMATracker(PeriodList periodData, MAIPeriod periodType)
            : base(periodData, periodType)
        { }

        protected override float CalculateValue(PeriodList periodData)
        {
            return ForexUtils.CalculateSMA(periodData, periodType);
        }
    }
}
