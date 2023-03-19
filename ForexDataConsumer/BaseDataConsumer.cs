using System;
using System.Collections.Generic;
using ForexCommon.Enums;
using ForexCommon.Models;
using ForexDataConsumer.Interfaces;

namespace ForexDataConsumer
{
    public abstract class BaseDataConsumer : IConsumer
    {
        protected readonly List<string> stockTypes;
        public StockTypeEnum[] StockTypes { get; }
        public Action<Price> AddPrice { get; set; }

        protected BaseDataConsumer(params StockTypeEnum[] stockTypes)
        {
            this.StockTypes = stockTypes;

            this.stockTypes = new List<string>();

            foreach (var stockType in stockTypes)
            {
                string localType = ConvertToLocalStockType(stockType);
                this.stockTypes.Add(localType);
            }
        }

        public abstract void Start();

        public abstract void Stop();

        protected abstract string ConvertToLocalStockType(StockTypeEnum stockType);

        protected abstract StockTypeEnum ConvertBack(string stockType);
    }
}
