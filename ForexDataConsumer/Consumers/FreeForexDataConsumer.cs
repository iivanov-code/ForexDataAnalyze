using System;
using System.Collections.Generic;
using ForexCommon.Models;

namespace ForexDataConsumer.Consumers
{
    public class FreeForexDataConsumer : BaseRESTDataConsumer<int>
    {
        private const string BASE_URL = "https://www.freeforexapi.com/api";
        private const string QUERY_STRING = "live?pairs={0}";

        public FreeForexDataConsumer(double queryPeriodMs, params ForexCommon.Enums.StockTypeEnum[] stockTypes)
            : base(queryPeriodMs, BASE_URL, stockTypes)
        { }

        protected override List<Price> ConvertToEventArgs(List<int> data)
        {
            throw new NotImplementedException();
        }

        protected override string FormatQueryString(string localStockTypes)
        {
            return string.Format(QUERY_STRING, localStockTypes);
        }
    }
}
