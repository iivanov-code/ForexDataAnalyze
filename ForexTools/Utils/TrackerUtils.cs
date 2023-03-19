using System;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Interfaces;
using ForexTools.Trackers;

namespace ForexTools.Utils
{
    internal static class TrackerUtils
    {
        public static ITracker GetTracker(PeriodList periodData, TrackerTypeEnum type)
        {
            switch (type)
            {
                case TrackerTypeEnum.SMA:
                    return new SMATracker(periodData);

                case TrackerTypeEnum.EMA:
                    return new EMATracker(periodData);

                case TrackerTypeEnum.MACD:
                    return new MACDTracker(periodData);

                case TrackerTypeEnum.RSI:
                    return new RSITracker(periodData);

                case TrackerTypeEnum.StochRSI:
                    return new StochRSITracker(periodData);
            }

            return null;
        }

        public static TrackerTypeEnum GetTrackerType(ITracker tracker)
        {
            TrackerTypeEnum type = TrackerTypeEnum.SMA;
            switch (tracker)
            {
                case EMATracker ema:
                    type = TrackerTypeEnum.EMA;
                    break;

                case SMATracker sma:
                    type = TrackerTypeEnum.SMA;
                    break;

                case MACDTracker macd:
                    type = TrackerTypeEnum.MACD;
                    break;

                case StochRSITracker stochRSI:
                    type = TrackerTypeEnum.StochRSI;
                    break;

                case RSITracker rsi:
                    type = TrackerTypeEnum.RSI;
                    break;

                default:
                    throw new ArgumentException("Value not handled!");
            }
            return type;
        }
    }
}
