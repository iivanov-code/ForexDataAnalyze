using System.Collections.Generic;
using ForexCommander.Interfaces;
using ForexDataConsumer.Interfaces;
using ForexTools;

namespace ForexCommander
{
    public partial class Commander : ICommander
    {
        private List<ForexStockManager> stockManagers;
        private IConsumer consumer;

        public Commander(ForexStockManager stockManager, IConsumer consumer)
            : this(new List<ForexStockManager> { stockManager }, consumer)
        { }

        public Commander(List<ForexStockManager> stockManagers, IConsumer consumer)
        {
            this.stockManagers = stockManagers;
            this.consumer = consumer;
        }

        public void Start()
        {
            consumer.Start();
        }

        public void Stop()
        {
            consumer.Stop();
        }
    }
}
