using System;
using ForexCommon.Enums;
using ForexCommon.Models;

namespace ForexDataConsumer.Interfaces
{
    public interface IConsumer
    {
        StockTypeEnum[] StockTypes { get; }

        void Start();

        void Stop();

        Action<Price> AddPrice { set; }
    }
}
