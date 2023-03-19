using ForexCommon.Enums;
using ForexCommon.Models;
using ForexTools.Enums;
using ForexTools.Structs;

namespace ForexTools.EventArgTypes
{
    public class TrackersMotionEventArgs : MotionEventArgs
    {
        public readonly StockTypeEnum StockType;
        public readonly PeriodTypeEnum Period;
        public readonly Percentage Percentage;
        public readonly Price CurrentFullPrice;
        public readonly TrackerTypeEnum TrackerType;

        public TrackersMotionEventArgs(Price currentPrice, TrackerTypeEnum trackerType, MotionTypeEnum motionDirection, StockTypeEnum stockType, PeriodTypeEnum period, Percentage percentage)
            : base(motionDirection, currentPrice.AvgPrice)
        {
            this.CurrentFullPrice = currentPrice;
            this.StockType = stockType;
            this.Period = period;
            this.Percentage = percentage;
            this.TrackerType = trackerType;
        }
    }
}
