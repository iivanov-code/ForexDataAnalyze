using System;
using ForexCommon.Enums;
using ForexCommon.Models;

namespace ForexCommon.Interfaces
{
    public interface IForexStockManager : IEquatable<IForexStockManager>
    {
        void AddPrice(Price currentPrice);

        StockTypeEnum StockType { get; }
    }
}
