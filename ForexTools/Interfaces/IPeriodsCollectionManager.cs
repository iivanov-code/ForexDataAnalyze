using System;
using ForexCommon.Enums;
using ForexTools.Collections;
using ForexTools.EventArgTypes;

namespace ForexTools.Interfaces
{
    public interface IPeriodsCollectionManager
    {
        void AddNewValue(float value);

#if DEBUG

        void ForceEndPeriod();

#endif
        PeriodsDictionary Periods { get; }
        StockTypeEnum StockType { get; }

        event EventHandler<PeriodPriceEventArgs> CandleOfInterest;
    }
}
