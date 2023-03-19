using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForexCommon;
using ForexCommon.Enums;
using ForexCommon.Models;

namespace ForexDataConsumer.Consumers
{
    public class CsvFileConsumer : BaseDataConsumer
    {
        private string fullFilePath;
        private int skipLines;
        private int skipColumns;
        private bool shouldStop;

        public CsvFileConsumer(string fullFilePath, int skipLines = 1, int skipColumns = 2)
        {
            this.fullFilePath = fullFilePath;
            this.skipLines = skipLines;
            this.skipColumns = skipColumns;
        }

        public override void Start()
        {
            shouldStop = false;

            Task.Factory.StartNew(() =>
            {
                IEnumerable<string> lines = File.ReadLines(fullFilePath);
                foreach (var line in lines.Skip(skipLines))
                {
                    string[] values = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values.Skip(skipColumns).Take(4))
                    {
                        string val = value.Replace("\"", "").Replace("\\", "");
                        float currPrice = float.Parse(val);
                        float spread = 0.02f;
                        spread = 0;
                        var price = new Price { AvgPrice = currPrice, BuyPrice = currPrice - spread, SellPrice = currPrice + spread, StockType = StockTypeEnum.EUR_USD };
                        AddPrice(price);
                        if (shouldStop) break;
                    }
                    TestMethodConnection.EndPeriod();
                    if (shouldStop) break;
                }
            });
        }

        public override void Stop()
        {
            shouldStop = true;
        }

        protected override StockTypeEnum ConvertBack(string stockType)
        {
            return (StockTypeEnum)Enum.Parse(typeof(StockTypeEnum), stockType);
        }

        protected override string ConvertToLocalStockType(StockTypeEnum stockType)
        {
            return stockType.ToString();
        }
    }
}
