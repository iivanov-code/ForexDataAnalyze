using System;
using System.Runtime.CompilerServices;
using System.Timers;
using ForexCommon.Enums;
using ForexTools.Enums;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;

namespace ForexTools.Collections
{
    public partial class PeriodsCollectionManager : IDisposable, IPeriodsCollectionManager, IInitCollectionManager
    {
        public StockTypeEnum StockType { get; }
        public PeriodsDictionary Periods { get; }

        private double smallestPeriodMillis, largestPeriodMillis;
        private readonly Timer timer;

        internal PeriodsCollectionManager(StockTypeEnum type)
        {
            StockType = type;
            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            this.Periods = new PeriodsDictionary();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            Periods[0].EndPeriod();
            timer.Enabled = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddNewValue(float value)
        {
            Periods[0].Add(value);
            if (!timer.Enabled)
                timer.Enabled = true;
        }

#if DEBUG

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ForceEndPeriod()
        {
            if (timer.Enabled)
            {
                timer.Enabled = false;
            }

            Periods[0].EndPeriod();
        }

#endif

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IPeriodsCollectionManager AddPeriod(PeriodTypeEnum periodType)
        {
            if (Periods.Add(periodType))
            {
                Periods[periodType].CandleOfInterest += PeriodsCollectionManager_CandleOfInterest;

                for (int i = 1; i < Periods.Count; i++)
                {
                    Periods[i - 1].DependentPeriod = Periods[i];
                }
                timer.Stop();
                smallestPeriodMillis = TimeSpan.FromSeconds((int)Periods.SmallestPeriod.Period).TotalMilliseconds;
                largestPeriodMillis = TimeSpan.FromSeconds((int)Periods.LargestPeriod.Period).TotalMilliseconds;

                timer.Interval = smallestPeriodMillis;

                return this;
            }
            else
            {
                return this;
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

        private void PeriodsCollectionManager_CandleOfInterest(object sender, PeriodPriceEventArgs e)
        {
            candleOfInterest(sender, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
