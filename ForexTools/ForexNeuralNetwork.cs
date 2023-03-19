//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace ForexTools
//{
//    public class ForexNeuralNetwork
//    {
//        private INetwork network;

//        public ForexNeuralNetwork()
//        {
//        }

//        public void Train(string fileName, int skipLines = 1, int skipColumns = 2)
//        {
//            int take = 4;
//            var prices = GetPricesFromFile(fileName, skipLines, skipColumns);
//            int count = prices.Count();
//            HashSet<float> uniqueValues = new HashSet<float>();
//            float prevValue = 0;

//            for (int skip = 0; skip < count; skip += take)
//            {
//                Quad[] pricesArr = prices.Skip(skip).Take(take).Cast<Quad>().ToArray();
//                network.Propagate(pricesArr);
//                float totalError = 0;
//                if (pricesArr[3] < prevValue)
//                {
//                    totalError = network.Backpropagate(new Quad[] { 1, 0 });
//                }
//                else
//                {
//                    totalError = network.Backpropagate(new Quad[] { 0, 1 });
//                }

//                prevValue = pricesArr[3];
//                Console.WriteLine(totalError);
//                //Console.WriteLine(string.Join(" ", network.Values));
//                //float[] targets = pricesArr.Skip(5).ToArray();
//                //if (targets.Length == 3)
//                {
//                    // float totalError = network.Backpropagate(targets);
//                    // Console.WriteLine(totalError);
//                }
//            }
//            Console.WriteLine(string.Join(Environment.NewLine, uniqueValues));
//            Console.WriteLine(uniqueValues.Count);
//        }

//        private static IEnumerable<float> GetPricesFromFile(string fileName, int skipLines = 1, int skipColumns = 1)
//        {
//            string fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
//            IEnumerable<string> lines = File.ReadLines(fullFilePath);
//            foreach (var line in lines.Skip(skipLines))
//            {
//                string[] values = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

//                foreach (var value in values.Skip(skipColumns).Take(4))
//                {
//                    string val = value.Replace("\"", "").Replace("\\", "");
//                    float currPrice = float.Parse(val);
//                    yield return currPrice;
//                }
//            }
//        }
//    }
//}
