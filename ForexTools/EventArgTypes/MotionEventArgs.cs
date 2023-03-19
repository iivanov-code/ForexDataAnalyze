using System;
using ForexTools.Enums;

namespace ForexTools.EventArgTypes
{
    public class MotionEventArgs : EventArgs
    {
        public MotionTypeEnum MotionDirection { get; }
        public float CurrentPrice { get; }

        public MotionEventArgs(MotionTypeEnum motionDirection, float currentPrice)
        {
            this.MotionDirection = motionDirection;
            this.CurrentPrice = currentPrice;
        }
    }
}
