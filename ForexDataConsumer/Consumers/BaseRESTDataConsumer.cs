using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Timers;
using ForexCommon.Enums;
using ForexCommon.Models;
using Timer = System.Timers.Timer;

namespace ForexDataConsumer.Consumers
{
    public abstract class BaseRESTDataConsumer<T> : BaseDataConsumer, IDisposable
    {
        private readonly Timer timer;
        private readonly Uri baseURI;
        private readonly string localStockTypes;
        private readonly object timerPadlock = new object();
        private bool disposedValue;

        protected BaseRESTDataConsumer(double queryPeriodMs, string baseURI, params StockTypeEnum[] stockTypes)
            : base(stockTypes)
        {
            localStockTypes = string.Join(",", stockTypes.Select(x => ConvertToLocalStockType(x)));

            this.baseURI = new Uri(baseURI);
            timer = new Timer(queryPeriodMs);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(timerPadlock))
            {
                timer.Enabled = false;

                try
                {
                    using HttpClient client = new HttpClient();
                    client.BaseAddress = baseURI;
                    string queryString = FormatQueryString(localStockTypes);
                    string json = client.GetStringAsync(queryString).Result;
                    List<T> data = JsonSerializer.Deserialize<List<T>>(json);
                    foreach (var price in ConvertToEventArgs(data))
                    {
                        AddPrice(price);
                    }
                }
                catch (Exception)
                {
                    //TODO Loging logic
                }
                finally
                {
                    Monitor.Exit(timerPadlock);
                    timer.Enabled = true;
                }
            }
        }

        protected abstract string FormatQueryString(string localStockTypes);

        protected abstract List<Price> ConvertToEventArgs(List<T> data);

        public override void Start()
        {
            lock (timerPadlock)
            {
                timer.Start();
            }
        }

        public override void Stop()
        {
            lock (timerPadlock)
            {
                timer.Stop();
            }
        }

        protected override string ConvertToLocalStockType(StockTypeEnum stockType)
        {
            return stockType.ToString().Replace("_", "");
        }

        protected override StockTypeEnum ConvertBack(string stockType)
        {
            return (StockTypeEnum)Enum.Parse(typeof(StockTypeEnum), stockType.Substring(0, 3) + "_" + stockType.Substring(3, 3));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
