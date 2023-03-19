using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ForexTools.Enums;

namespace ForexTools.Collections
{
    public class PeriodsDictionary : IReadOnlyCollection<PeriodList>
    {
        private List<PeriodList> periods;

        private readonly Dictionary<PeriodTypeEnum, int> indexes;

        public PeriodsDictionary()
        {
            periods = new List<PeriodList>();
            indexes = new Dictionary<PeriodTypeEnum, int>();
        }

        public PeriodList SmallestPeriod { get; set; }
        public PeriodList LargestPeriod { get; set; }

        public int Count
        {
            get
            {
                return periods.Count;
            }
        }

        internal PeriodList this[int i]
        {
            get
            {
                return periods[i];
            }
        }

        public PeriodList this[PeriodTypeEnum key]
        {
            get
            {
                return periods[indexes[key]];
            }
        }

        public bool Add(PeriodTypeEnum type)
        {
            if (!indexes.ContainsKey(type))
            {
                indexes.Add(type, periods.Count);
                var period = new PeriodList(type);
                periods.Add(period);

                periods = periods.OrderBy(x => x.Period).ToList();
                SetSmallestAndLargest(period);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetSmallestAndLargest(PeriodList period)
        {
            if (SmallestPeriod == null)
            {
                SmallestPeriod = period;
            }
            else if (SmallestPeriod.Period > period.Period)
            {
                SmallestPeriod = period;
            }

            if (LargestPeriod == null)
            {
                LargestPeriod = period;
            }
            else if (LargestPeriod.Period < period.Period)
            {
                LargestPeriod = period;
            }
        }

        public IEnumerator<PeriodList> GetEnumerator()
        {
            return periods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
