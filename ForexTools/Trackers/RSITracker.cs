using System;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Structs;
using ForexTools.Utils;

namespace ForexTools.Trackers
{
    public class RSITracker : BaseTrendLineTracker
    {
        private float? prevAvgGain, prevAvgLoss;

        protected virtual Percentage MIN_PERCENT { get; } = 30;
        protected virtual Percentage MAX_PERCENT { get; } = 70;

        private bool overSold = false, overBought = false;

        public RSITracker(PeriodList periodData)
            : base(periodData, MAIPeriod.MAI14)
        { }

        protected override Func<float, bool> comparerUp
        {
            get
            {
                return (value) =>
                {
                    if (value < MIN_PERCENT)
                    {
                        overSold = true;
                        overBought = false;
                        return false;
                    }
                    else if (value > MIN_PERCENT && overSold)
                    {
                        overSold = false;
                        overBought = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }
        }

        protected override Func<float, bool> comparerDown
        {
            get
            {
                return (value) =>
                {
                    if (value > MAX_PERCENT)
                    {
                        overBought = true;
                        overSold = false;
                        return false;
                    }
                    else if (value < MAX_PERCENT && overBought)
                    {
                        overSold = false;
                        overBought = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }
        }

        protected override float CalculateValue(PeriodList periodData)
        {
            float rsi = ForexUtils.CalculateRSI(periodData, periodType, out float avgGain, out float avgLoss, prevAvgGain, prevAvgLoss);

            prevAvgGain = avgGain;
            prevAvgLoss = avgLoss;

            return rsi;
        }
    }
}
