using ForexCommander;
using ForexCommon.Enums;
using ForexDataConsumer.Consumers;
using ForexTools.Collections;
using ForexTools.Enums;
using ForexTools.Utils;
using System;
using System.IO;

namespace ForexTest
{
    public static class Program
    {
        public static void Main()
        {
            Console.ReadLine();
        }

        private static Commander cmd;
        private static readonly Random rand = new Random();

        //private static void TrainNetwork()
        //{
        //    ForexNeuralNetwork network = new ForexNeuralNetwork();
        //    string fileName = "EUR_USD Historical Data.csv";
        //    // do
        //    {
        //        network.Train(fileName);
        //    }  //while (++count < 5);

        //    while (Console.ReadLine() != "exit")
        //    {
        //        network.Train(fileName);
        //    }
        //}

        private static void Train(string fileName)
        {
            string fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            cmd = Commander.GetWallet(100000, StockTypeEnum.USD_EUR, new MockTradingService(), new CsvFileConsumer(fullFilePath));
            cmd.Start();

            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine(Commander.walletManager.WalletAmount);
            }
        }

        private static void CollectionManagerTest()
        {
            var setUpManager = PeriodsCollectionManager.GetManager(StockTypeEnum.USD_EUR);
            setUpManager.AddPeriod(PeriodTypeEnum.Minute);
            setUpManager.AddPeriod(PeriodTypeEnum.FiveMinutes);
            setUpManager.AddPeriod(PeriodTypeEnum.FifteenMinutes);
            setUpManager.AddPeriod(PeriodTypeEnum.FourHours);
            var manager = setUpManager.AddPeriod(PeriodTypeEnum.Day);

            int count = 0;

            while (count++ < 5000)
            {
                manager.AddNewValue((float)rand.NextDouble() + 1);
                //  Thread.Sleep(1000);
            }

            var fae = manager.Periods[PeriodTypeEnum.Minute];
            ForexUtils.CalculateEMA(fae, MAIPeriod.MAI21);
        }
    }
}
