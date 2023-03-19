using System;
using ForexTools.EventArgTypes;

namespace ForexTools.Interfaces
{
    internal interface IPeriodPrice : IBasePeriodPrice
    {
        float LastPrice { get; }

        void AssignClosingPrice(float closingPrice);

        void AssignMinPrice(float minPrice);

        void AssignMaxPrice(float maxPrice);

        void EndPeriod();

        void UpdateValue(float price);

        event EventHandler<PeriodPriceEventArgs> CandleOfInterest;
    }
}
