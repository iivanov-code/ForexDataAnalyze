using System;

namespace ForexTools.EventArgTypes
{
    public abstract class BasePeriodPriceEventArgs : EventArgs
    {
        protected BasePeriodPriceEventArgs(float openingPrice)
        {
            _openingPrice = openingPrice;
        }

        public float MaxPrice { get; protected set; }
        public float MinPrice { get; protected set; }
        protected readonly float _openingPrice;

        public float OpeningPrice
        {
            get
            {
                return _openingPrice;
            }
        }

        public float ClosingPrice { get; protected set; }

        public float AvgPrice
        {
            get
            {
                return (MaxPrice + MinPrice) / 2;
            }
        }
    }
}
