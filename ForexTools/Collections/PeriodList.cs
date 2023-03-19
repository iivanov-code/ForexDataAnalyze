using System;
using System.Collections.Generic;
using System.Linq;
using ForexTools.ConcreteTypes;
using ForexTools.Enums;

namespace ForexTools.Collections
{
    public sealed class PeriodList : BasePeriodList, IEquatable<PeriodList>
    {
        public PeriodList(PeriodTypeEnum period) : base(period)
        { }

        public PeriodList(PeriodTypeEnum period, BasePeriodList dependentPeriod) : base(period, dependentPeriod)
        { }

        public int Capacity => periods.Capacity;

        public bool NewPeriodOpened { get; private set; }

        private event EventHandler periodEnded = delegate { };

        public event EventHandler PeriodEnded
        {
            add
            {
                periodEnded += value;
            }
            remove
            {
                periodEnded -= value;
            }
        }

        public override void Add(float price)
        {
            base.Add(price);
            NewPeriodOpened = true;
        }

        public override void EndPeriod()
        {
            base.EndPeriod();
            NewPeriodOpened = false;
            periodEnded?.Invoke(this, EventArgs.Empty);
        }

        public static explicit operator List<PeriodPrice>(PeriodList v)
        {
            return v.ToList();
        }

        public bool Equals(PeriodList other)
        {
            return this.Period == other.Period;
        }

        public override int GetHashCode()
        {
            return this.Period.GetHashCode();
        }
    }
}
