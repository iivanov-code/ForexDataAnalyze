using System;
using ForexTools.Enums;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;

namespace ForexTools.ConcreteTypes
{
    public class PeriodPrice : BasePeriodPrice, IPeriodPrice
    {
        private float lastPrice;
        private const float Threashold = 1.3f;

        public float LastPrice
        {
            get
            {
                return lastPrice;
            }
        }

        public PeriodPrice(float openingPrice)
            : base(openingPrice)
        { }

        public void AssignMaxPrice(float maxPrice)
        {
            this.maxPrice = maxPrice;
        }

        public void AssignMinPrice(float minPrice)
        {
            this.minPrice = minPrice;
        }

        public void AssignClosingPrice(float closingPrice)
        {
            this.closingPrice = closingPrice;
        }

        public void UpdateValue(float price)
        {
            lastPrice = price;
            if (price > MaxPrice)
            {
                AssignMaxPrice(price);
            }

            if (price < MinPrice)
            {
                AssignMinPrice(price);
            }
        }

        public void EndPeriod()
        {
            AssignClosingPrice(lastPrice);
            CheckCandleType();
        }

        private void CheckCandleType()
        {
            double max = Math.Max(ClosingPrice.Value, OpeningPrice);
            double min = Math.Min(ClosingPrice.Value, OpeningPrice);

            if (Math.Abs(MaxPrice - max) > Math.Abs(max - min) * Threashold)
            {
                OnCandleOfInterest(new PeriodPriceEventArgs(this, CandleStickTypeEnum.ShootingStar));
            }
            else if (Math.Abs(MinPrice - min) > Math.Abs(max - min) * Threashold)
            {
                OnCandleOfInterest(new PeriodPriceEventArgs(this, CandleStickTypeEnum.HammerHead));
            }
            else
            {
                double lowerDiff = Math.Abs(MinPrice - min);
                double upperDiff = Math.Abs(MaxPrice - max);
                double crossHeight = Math.Abs(max - min);

                if (Math.Abs(lowerDiff - upperDiff) < Threashold && upperDiff / crossHeight > Threashold)
                {
                    OnCandleOfInterest(new PeriodPriceEventArgs(this, CandleStickTypeEnum.MorningDojiStar));
                }
            }
        }
    }
}
