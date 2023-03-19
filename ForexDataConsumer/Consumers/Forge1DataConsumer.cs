using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ForexCommon.Enums;
using ForexCommon.Models;

namespace ForexDataConsumer.Consumers
{
    public class Forge1DataConsumer : BaseRESTDataConsumer<JsonQuery>
    {
        private const string API_KEY = "2IxrWZwDGweGX46FkHw0Mg9gFp0owDs3";
        private const string QUOTES = "quotes?pairs={0}&api_key={1}";
        private const string BASE_URL = "https://forex.1forge.com/1.0.3";

        public Forge1DataConsumer(double queryPeriodMs, params StockTypeEnum[] stockTypes)
            : base(queryPeriodMs, BASE_URL, stockTypes)
        { }

        protected override List<Price> ConvertToEventArgs(List<JsonQuery> data)
        {
            List<Price> prices = new List<Price>();
            DateTime baseDate = new DateTime(1970, 1, 1);
            foreach (var item in data)
            {
                prices.Add(new Price
                {
                    BuyPrice = item.Ask,
                    AvgPrice = item.Price,
                    SellPrice = item.Bid,
                    SignalTime = baseDate.AddMilliseconds(item.Timestamp),
                    StockType = ConvertBack(item.Symbol)
                });
            }
            return prices;
        }

        protected override string FormatQueryString(string localStockTypes)
        {
            return string.Format(QUOTES, localStockTypes, API_KEY);
        }
    }

    public class JsonQuery
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("bid")]
        public float Bid { get; set; }

        [JsonPropertyName("ask")]
        public float Ask { get; set; }

        [JsonPropertyName("price")]
        public float Price { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
