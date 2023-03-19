using System;
using ForexTools.EventArgTypes;
using ForexTools.Interfaces;

namespace ForexTools.ConcreteTypes
{
    public abstract class BasePeriodPrice : IBasePeriodPrice
    {
        protected float openingPrice;
        protected float? closingPrice;
        protected float maxPrice;
        protected float minPrice;

        internal BasePeriodPrice(float openingPrice)
        {
            this.openingPrice = openingPrice;
            float value = float.MinValue;
            this.maxPrice = value;
            value = float.MaxValue;
            this.minPrice = value;
        }

        private event EventHandler<PeriodPriceEventArgs> candleOfInterest;

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

        protected void OnCandleOfInterest(PeriodPriceEventArgs e)
        {
            candleOfInterest(this, e);
        }

        public float OpeningPrice
        {
            get
            {
                return openingPrice;
            }
        }

        public float MaxPrice
        {
            get
            {
                return maxPrice;
            }
        }

        public float MinPrice
        {
            get
            {
                return minPrice;
            }
        }

        public float? ClosingPrice
        {
            get
            {
                return closingPrice;
            }
        }

        public float AvgPrice
        {
            get
            {
                return (MaxPrice + MinPrice) / 2;
            }
        }
    }
}
