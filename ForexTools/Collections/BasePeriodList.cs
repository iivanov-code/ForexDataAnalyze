using System;
using System.Collections;
using System.Collections.Generic;
using ForexTools.ConcreteTypes;
using ForexTools.Enums;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;

namespace ForexTools.Collections
{
    public abstract class BasePeriodList : IEnumerable<PeriodPrice>
    {
        internal readonly PeriodTypeEnum Period;
        protected StockStack<PeriodPrice> periods;
        private readonly object padlock = new object();
        private IPeriodPrice activePeriod;

        internal IBasePeriodPrice ActivePeriod
        {
            get
            {
                return activePeriod;
            }
        }

        public int Count => periods.Count;

        public PeriodPrice this[int index] => periods[index];

        private BasePeriodList dependentPeriod;

        internal BasePeriodList DependentPeriod
        {
            get
            {
                return dependentPeriod;
            }
            set
            {
                lock (padlock)
                {
                    dependentPeriod = value;
                }
            }
        }

        internal BasePeriodList(PeriodTypeEnum period)
        {
            this.Period = period;
            int capacity = GetCapacityByPeriod(period);
            periods = new StockStack<PeriodPrice>(capacity);
        }

        internal BasePeriodList(PeriodTypeEnum period, BasePeriodList dependentPeriod)
            : this(period)
        {
            this.DependentPeriod = dependentPeriod;
        }

        private void AddToDependentPeriod(float price)
        {
            DependentPeriod?.Add(price);
        }

        public virtual void Add(float price)
        {
            lock (padlock)
            {
                if (activePeriod == null)
                {
                    var period = new PeriodPrice(price);
                    period.CandleOfInterest += Period_CandleOfInterest;
                    activePeriod = period;
                    this.periods.Push(period);
                }
                else
                {
                    activePeriod.UpdateValue(price);
                }

                AddToDependentPeriod(price);
            }
        }

        private event EventHandler<PeriodPriceEventArgs> candleOfInterest = delegate { };

        public event EventHandler<PeriodPriceEventArgs> CandleOfInterest
        {
            add
            {
                candleOfInterest += value;
            }
            remove
            {
                candleOfInterest -= value;
            }
        }

        private void Period_CandleOfInterest(object sender, PeriodPriceEventArgs e)
        {
            candleOfInterest(sender, e);
        }

        public virtual void EndPeriod()
        {
            lock (padlock)
            {
                if (activePeriod != null)
                {
                    activePeriod.EndPeriod();
                    activePeriod.CandleOfInterest -= Period_CandleOfInterest;
                    activePeriod = null;
                }

                dependentPeriod?.EndPeriod();
            }
        }

        public IEnumerator<PeriodPrice> GetEnumerator()
        {
            return periods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static int GetCapacityByPeriod(PeriodTypeEnum periodType)
        {
            int capacity = 1000; //default capacity
            switch (periodType)
            {
                case PeriodTypeEnum.Second:
                    capacity = 14400;
                    break;

                case PeriodTypeEnum.Minute:
                    capacity = 1440;
                    break;

                case PeriodTypeEnum.FiveMinutes:
                    capacity = 288;
                    break;

                case PeriodTypeEnum.TenMinutes:
                    capacity = 144;
                    break;

                case PeriodTypeEnum.FifteenMinutes:
                case PeriodTypeEnum.HalfHour:
                    capacity = 96;
                    break;

                case PeriodTypeEnum.Hour:
                    capacity = 48;
                    break;

                case PeriodTypeEnum.FourHours:
                    capacity = 168;
                    break;

                case PeriodTypeEnum.Day:
                    capacity = 366;
                    break;

                case PeriodTypeEnum.Week:
                    capacity = 54;
                    break;

                case PeriodTypeEnum.Month:
                    capacity = 24;
                    break;

                case PeriodTypeEnum.NONE:
                    break;

                default:
                    capacity = 100;
                    break;
            }

            return capacity;
        }
    }
}
