using System;
using System.Collections;
using System.Collections.Generic;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;

namespace ForexTools.Trackers
{
    public abstract class BaseTrendLineTracker : IEnumerable<float>, ITracker
    {
        protected internal readonly PeriodList periodData;
        protected internal readonly MAIPeriod periodType;
        private MotionTypeEnum? prevMotion;

        public StockStack<float> Curve { get; }
        public PeriodTypeEnum PeriodType => periodData.Period;

        protected BaseTrendLineTracker(PeriodList periodData, MAIPeriod periodType)
        {
            this.periodData = periodData;
            this.periodType = periodType;
            this.Curve = new StockStack<float>(periodData.Capacity);
            this.periodData.PeriodEnded += Periods_PeriodEnded;
        }

        private void Periods_PeriodEnded(object sender, EventArgs e)
        {
            if (periodData.Count > (int)periodType)
            {
                float value = CalculateValue(periodData);

                FindTrend(value);

                Curve.Push(value);
            }
        }

        protected virtual float ComparisonValue
        {
            get
            {
                return periodData[0].LastPrice;
            }
        }

        protected virtual Func<float, bool> comparerDown
        {
            get
            {
                return (value) => value < ComparisonValue;
            }
        }

        protected virtual Func<float, bool> comparerUp
        {
            get
            {
                return (value) => value > ComparisonValue;
            }
        }

        private void FindTrend(float value)
        {
            if (comparerUp(value) && prevMotion.HasValue && prevMotion != MotionTypeEnum.Up)
            {
                prevMotion = MotionTypeEnum.Up;
                OnMotionChange(prevMotion.Value, periodData[0].LastPrice);
            }
            else if (comparerDown(value) && prevMotion.HasValue && prevMotion != MotionTypeEnum.Down)
            {
                prevMotion = MotionTypeEnum.Down;
                OnMotionChange(prevMotion.Value, periodData[0].LastPrice);
            }
            else if (!prevMotion.HasValue)
            {
                prevMotion = ComparisonValue > value ? MotionTypeEnum.Up : MotionTypeEnum.Down;
            }
        }

        private event EventHandler<MotionEventArgs> motionChange = delegate { };

        public event EventHandler<MotionEventArgs> MotionChange
        {
            add
            {
                motionChange += value;
            }
            remove
            {
                motionChange -= value;
            }
        }

        protected virtual void OnMotionChange(MotionTypeEnum motionType, float currentPrice)
        {
            motionChange(this, new MotionEventArgs(motionType, currentPrice));
        }

        protected abstract float CalculateValue(PeriodList periodData);

        public IEnumerator<float> GetEnumerator()
        {
            return Curve.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
