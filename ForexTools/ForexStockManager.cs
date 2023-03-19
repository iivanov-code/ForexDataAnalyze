using System;
using System.Collections.Generic;
using ForexCommon.Enums;
using ForexCommon.EventArgModels;
using ForexCommon.Interfaces;
using ForexCommon.Models;
using ForexTools.ConcreteTypes;
using ForexTools.Enums;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;
using ForexTools.Structs;
using ForexTools.Utils;

namespace ForexTools
{
    public class ForexStockManager : IForexStockManager
    {
        private readonly IPeriodsCollectionManager collectionManager;
        private readonly List<ITracker> trackers;
        private Price currentPrice;

        private event EventHandler<TrackersMotionEventArgs> signal = delegate { };

        public event EventHandler<TrackersMotionEventArgs> Signal
        {
            add
            {
                signal += value;
            }
            remove
            {
                signal -= value;
            }
        }

        private event EventHandler<PriceEventArgs> newValue = delegate { };

        public event EventHandler<PriceEventArgs> NewValue
        {
            add
            {
                newValue += value;
            }
            remove
            {
                newValue -= value;
            }
        }

        public event EventHandler<PeriodPriceEventArgs> CandleOfInterest
        {
            add
            {
                this.collectionManager.CandleOfInterest += value;
            }
            remove
            {
                this.collectionManager.CandleOfInterest -= value;
            }
        }

        private readonly Dictionary<PeriodTypeEnum, Dictionary<TrackerTypeEnum, Percentage>> PercentageDict;
        private readonly Dictionary<PeriodTypeEnum, MotionPercentageModel> indexes;

        public StockTypeEnum StockType
        {
            get
            {
                return collectionManager.StockType;
            }
        }

        public ForexStockManager(IPeriodsCollectionManager collectionManager, TrackerTypeEnum[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }
            this.PercentageDict = new Dictionary<PeriodTypeEnum, Dictionary<TrackerTypeEnum, Percentage>>();
            this.indexes = new Dictionary<PeriodTypeEnum, MotionPercentageModel>();
            this.collectionManager = collectionManager;
            this.trackers = new List<ITracker>();

            foreach (var period in this.collectionManager.Periods)
            {
                indexes.Add(period.Period, new MotionPercentageModel(MotionTypeEnum.Down, (Percentage.MaxValue / types.Length)));
                PercentageDict.Add(period.Period, new Dictionary<TrackerTypeEnum, Percentage>());
                var dict = PercentageDict[period.Period];
                foreach (var trackerType in types)
                {
                    ITracker tracker = TrackerUtils.GetTracker(period, trackerType);
                    tracker.MotionChange += Tracker_MotionChange;
                    this.trackers.Add(tracker);
                    dict.Add(trackerType, (Percentage.MaxValue / types.Length));
                }
            }
        }

        public void AddPrice(Price currentPrice)
        {
            this.currentPrice = currentPrice;
            this.collectionManager.AddNewValue(currentPrice.AvgPrice);
            this.newValue(this, new PriceEventArgs { Price = currentPrice.AvgPrice, StockType = StockType });
        }

        private void Tracker_MotionChange(object sender, MotionEventArgs e)
        {
            ITracker tracker = sender as ITracker;
            Percentage percent = PercentageDict[tracker.PeriodType][TrackerUtils.GetTrackerType(tracker)];
            var index = indexes[tracker.PeriodType];

            var args = new TrackersMotionEventArgs(currentPrice, TrackerUtils.GetTrackerType(tracker), e.MotionDirection, collectionManager.StockType, tracker.PeriodType, index.Percentage);
            signal(this, args);
        }

        public override int GetHashCode()
        {
            return this.StockType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.StockType == (obj as ForexStockManager).StockType;
        }

        public bool Equals(IForexStockManager other)
        {
            return this.StockType == other.StockType;
        }
    }
}
