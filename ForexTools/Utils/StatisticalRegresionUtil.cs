using System;
using System.Collections.Generic;
using System.Linq;
using ForexTools.ConcreteTypes;
using ForexTools.Interfaces;

namespace ForexTools.Utils
{
    public static class StatisticalRegresionUtil
    {
        public static LinearRegresion CalculateLinearRegresion(IEnumerable<ITimeSeries> values)
        {
            int nCount = values.Count();
            double sumXY = values.Sum(x => x.X * x.Y);
            double sumX = values.Sum(x => x.X);
            double sumY = values.Sum(x => x.Y);
            double sumXSquare = values.Sum(x => Math.Pow(x.X, 2));
            double sumYSquare = values.Sum(x => Math.Pow(x.Y, 2));
            var ab = FindLeastSquares(nCount, sumXY, sumX, sumY, sumXSquare, sumYSquare);
            double rSquare = CalculateRSquare(nCount, sumXY, sumX, sumY, sumXSquare, sumYSquare);

            return new LinearRegresion { A = ab.Item1, B = ab.Item2, RSquare = rSquare };
        }

        /// <summary>
        /// Calculates Linear Regresion
        /// </summary>
        /// <param name="nCount">Data Count</param>
        /// <param name="sumXY">Sum of X pultiplyed by Y</param>
        /// <param name="sumX">Sum of all X values</param>
        /// <param name="sumY">Sum of all Y values</param>
        /// <param name="sumXSquare">Sum of all X values squared</param>
        /// <param name="sumYSquare">Sum of all Y values squared</param>
        /// <returns>Tuple of item1 = a and item2 = b</returns>
        private static Tuple<double, double> FindLeastSquares(int nCount, double sumXY, double sumX, double sumY, double sumXSquare, double sumYSquare)
        {
            double a = ((sumY * sumXSquare) - (sumX * sumXY)) / ((nCount * sumXSquare) - Math.Pow(sumX, 2));
            double b = ((nCount * sumXY) - (sumX * sumY)) / ((nCount * sumXSquare) - Math.Pow(sumX, 2));

            return new Tuple<double, double>(a, b);
        }

        /// <summary>
        /// Coefficient of Determination
        /// </summary>
        /// <param name="nCount">Data Count</param>
        /// <param name="sumXY">Sum of X pultiplyed by Y</param>
        /// <param name="sumX">Sum of all X values</param>
        /// <param name="sumY">Sum of all Y values</param>
        /// <param name="sumXSquare">Sum of all X values squared</param>
        /// <param name="sumYSquare">Sum of all Y values squared</param>
        /// <returns>R squared</returns>
        private static double CalculateRSquare(int nCount, double sumXY, double sumX, double sumY, double sumXSquare, double sumYSquare)
        {
            double r = ((nCount * sumXY) - (sumX * sumY)) / (Math.Sqrt(((nCount * sumXSquare) - Math.Pow(sumX, 2)) * ((nCount * sumYSquare) - Math.Pow(sumY, 2))));
            return Math.Pow(r, 2);
        }
    }
}
