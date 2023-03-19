using System.Collections.Generic;
using System.Linq;
using ForexTools.Collections;
using ForexTools.ConcreteTypes;
using ForexTools.Enums;

namespace ForexTools.Utils
{
#if DEBUG

    public static partial class ForexUtils
    {
        public static float CalculateSMA(IEnumerable<PeriodPrice> periodData, MAIPeriod index)
        {
            if (periodData.First().ClosingPrice.HasValue)
            {
                return CalculateSMA(periodData.Select(x => x.ClosingPrice.Value), index);
            }
            else
            {
                return CalculateSMA(periodData.Skip(1).Select(x => x.ClosingPrice.Value), index);
            }
        }

        public static float CalculateEMA(IList<PeriodPrice> periodData, MAIPeriod index, float prevEMA = 0)
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

        /// <summary>
        /// Calculation of Moving Average Covergence/Divergence
        /// </summary>
        /// <param name="periodData">Period data</param>
        /// <param name="histogram">Histogram</param>
        /// <param name="prevEma9">Signal line 9 period EMA</param>
        /// <param name="prevEma12">Previous 12 periods EMA</param>
        /// <param name="prevEma26">Previous 26 periods EMA</param>
        /// <returns></returns>
        public static float CalculateMACD(IList<PeriodPrice> periodData, StockStack<float> macdLine, out float histogram, ref float prevEma9, ref float prevEma12, ref float prevEma26)
        {
            return CalculateMACD(periodData.Select(x => x.ClosingPrice.Value), macdLine, out histogram, ref prevEma9, ref prevEma12, ref prevEma26);
        }
    }

#endif
}
