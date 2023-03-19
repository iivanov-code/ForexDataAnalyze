using System;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.EventArgTypes;

namespace ForexTools.Interfaces
{
    public interface ITracker
    {
        StockStack<float> Curve { get; }
        PeriodTypeEnum PeriodType { get; }

        event EventHandler<MotionEventArgs> MotionChange;
    }
}
