using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForexCommon.Enums;
using ForexCommon.Interfaces;
using ForexCommon.Models;

namespace ForexDataConsumer
{
    public class ConsumersManager : BaseDataConsumer
    {
        private readonly List<BaseDataConsumer> DataConsumers;
        private readonly Dictionary<StockTypeEnum, IForexStockManager> stockManagers;

        public ConsumersManager(IEnumerable<BaseDataConsumer> dataConsumers, HashSet<IForexStockManager> stockManagers)
            : base(dataConsumers.SelectMany(x => x.StockTypes).Distinct().ToArray())
        {
            if (stockManagers == null)
            {
                throw new ArgumentNullException(nameof(stockManagers));
            }

            this.stockManagers = stockManagers.ToDictionary(x => x.StockType, x => x);

            foreach (var consumer in dataConsumers)
            {
                consumer.AddPrice = AddPrice;
            }

            this.DataConsumers = new List<BaseDataConsumer>(dataConsumers);
        }

        private new void AddPrice(Price price)
        {
            stockManagers[price.StockType].AddPrice(price);
        }

        public override void Start()
        {
            Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(DataConsumers, (item) =>
                {
                    item.Start();
                });
            }, TaskCreationOptions.LongRunning);
        }

        public override void Stop()
        {
            Parallel.ForEach(DataConsumers, (item) =>
            {
                item.Stop();
            });
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
