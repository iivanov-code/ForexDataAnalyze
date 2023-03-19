using System;
using System.Collections.Generic;
using System.Linq;
using ForexTools.Collections;
using ForexTools.Enums;

namespace ForexTools.Utils
{
    public static partial class ForexUtils
    {
        public static float CalculateSMA(PeriodList periodData, MAIPeriod index)
        {
            if (periodData.Count >= (int)index)
            {
                if (!periodData[0].ClosingPrice.HasValue && periodData.Count == (int)index)
                {
                    throw new ArgumentException("Price list smaller than period", nameof(periodData));
                }
                else
                {
                    if (!periodData[0].ClosingPrice.HasValue)
                    {
                        return CalculateSMA(periodData.Skip(1).Select(x => x.ClosingPrice.Value), index);
                    }
                    else
                    {
                        return CalculateSMA(periodData.Select(x => x.ClosingPrice.Value), index);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Price list smaller than period", nameof(periodData));
            }
        }

        private static float CalculateSMA(IEnumerable<float> periodData, MAIPeriod index)
        {
            if (periodData.Take((int)index).Count() == (int)index)
            {
                return periodData.Select(x => x).Take((int)index).Average();
            }
            else
            {
                throw new ArgumentException("Price list smaller than period", nameof(periodData));
            }
        }

        public static float CalculateEMA(PeriodList periodData, MAIPeriod index, float prevEMA = default)
        {
            if (periodData.Count >= (int)index)
            {
                if (periodData[0].ClosingPrice.HasValue)
                {
                    return CalculateEMA(periodData.Select(x => x.ClosingPrice.Value), index, prevEMA);
                }
                else
                {
                    return CalculateEMA(periodData.Skip(1).Select(x => x.ClosingPrice.Value), index, prevEMA);
                }
            }
            else
            {
                throw new ArgumentException("Price list smaller than period", nameof(periodData));
            }
        }

        private static float CalculateEMA(IEnumerable<float> periodData, MAIPeriod index, float prevEMA = default)
        {
            if (periodData.Take((int)index).Count() == (int)index)
            {
                if (prevEMA == 0)
                {
                    return CalculateSMA(periodData, index);
                }
                else
                {
                    float lastPrice = periodData.First();
                    return lastPrice * (2 / ((float)index + 1f)) + prevEMA * (1 - (2 / ((float)index + 1f)));
                }
            }
            else
            {
                throw new ArgumentException("Price list smaller than period", nameof(periodData));
            }
        }

        public static float CalculateMACD(PeriodList periodData, StockStack<float> macdLine, out float histogram, ref float signalLine, ref float smallerEMA, ref float largerEMA, MAIPeriod smallerPeriod = MAIPeriod.MAI12, MAIPeriod largerPeriod = MAIPeriod.MAI26, MAIPeriod signalLinePeriod = MAIPeriod.MAI9)
        {
            if (periodData[0].ClosingPrice.HasValue)
            {
                var data = periodData.Select(x => x.ClosingPrice.Value);
                return CalculateMACD(data, macdLine, out histogram, ref signalLine, ref smallerEMA, ref largerEMA, smallerPeriod, largerPeriod, signalLinePeriod);
            }
            else
            {
                var data = periodData.Skip(1).Select(x => x.ClosingPrice.Value);
                return CalculateMACD(data, macdLine, out histogram, ref signalLine, ref smallerEMA, ref largerEMA, smallerPeriod, largerPeriod, signalLinePeriod);
            }
        }

        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_average_convergence_divergence_macd
        private static float CalculateMACD(IEnumerable<float> periodData, StockStack<float> macdLine, out float histogram, ref float signalLine, ref float smallerEMA, ref float largerEMA, MAIPeriod smallerPeriod = MAIPeriod.MAI12, MAIPeriod largerPeriod = MAIPeriod.MAI26, MAIPeriod signalLinePeriod = MAIPeriod.MAI9)
        {
            smallerEMA = CalculateEMA(periodData, smallerPeriod, smallerEMA);
            if (periodData.Take((int)largerPeriod).Count() == (int)largerPeriod)
            {
                largerEMA = CalculateEMA(periodData, largerPeriod, largerEMA);
                float MACDLine = smallerEMA - largerEMA;

                if (macdLine.Count >= (int)signalLinePeriod)
                {
                    signalLine = CalculateEMA(macdLine, signalLinePeriod, signalLine);
                    histogram = MACDLine - signalLine;
                }
                else
                {
                    histogram = default;
                }

                return MACDLine;
            }
            else
            {
                histogram = default;
                return default;
            }
        }

        public static float CalculateRSI(PeriodList periodData, MAIPeriod periodType, out float avgGain, out float avgLoss, float? prevAvgGain = null, float? prevAvgLoss = null)
        {
            if (prevAvgGain.HasValue && prevAvgLoss.HasValue)
            {
                avgGain = CalculateAverageGainLoss(periodData, periodType);
                avgLoss = CalculateAverageGainLoss(periodData, periodType, isForLoss: true);
            }
            else
            {
                avgGain = CalculateAverageGainLoss(periodData, periodType, prevAvgGain);
                avgLoss = CalculateAverageGainLoss(periodData, periodType, prevAvgLoss, isForLoss: true);
            }

            float RSI = 100 - (100 / (1 + (avgGain / avgLoss)));
            return RSI;
        }

        private static float CalculateAverageGainLoss(PeriodList periodData, MAIPeriod period, float? prevGainLoss = null, bool isForLoss = false)
        {
            Func<float, bool> comparer;

            if (isForLoss)
            {
                comparer = (value) => value <= 0;
            }
            else
            {
                comparer = (value) => value >= 0;
            }

            if (!prevGainLoss.HasValue)
            {
                float average = 0;
                int i = periodData[0].ClosingPrice.HasValue ? 0 : 1;

                for (i = 0; i < (int)period; i++)
                {
                    float value = periodData[i].ClosingPrice.Value - periodData[i + 1].ClosingPrice.Value;
                    if (comparer(value))
                    {
                        average += Math.Abs(value);
                    }
                }
                return average / (float)period;
            }
            else
            {
                float diff = CalculateMomentum(periodData);

                if (diff < 0 && isForLoss)
                {
                    diff = Math.Abs(diff);
                }
                else if ((diff < 0 && !isForLoss) || (diff > 0 && isForLoss))
                {
                    diff = 0;
                }
                else if (diff > 0 && !isForLoss)
                {
                    diff = Math.Abs(diff);
                }
                return ((prevGainLoss.Value * 13) + diff) / 14;
            }
        }

        public static float CalculateMomentum(PeriodList period)
        {
            if (period[0].ClosingPrice.HasValue)
            {
                return period[0].ClosingPrice.Value - period[1].ClosingPrice.Value;
            }
            else
            {
                return period[1].ClosingPrice.Value - period[2].ClosingPrice.Value;
            }
        }
    }
}
