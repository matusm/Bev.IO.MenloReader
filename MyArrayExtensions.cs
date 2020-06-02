using System;
using System.Linq;

namespace Bev.IO.MenloReader
{
    /// <summary>
    /// Quick and dirty extension to calculate the standard deviation and span of a double array.
    /// </summary>
    public static class MyArrayExtensions
    {
        /// <summary>
        /// Quick and dirty extension to calculate the standard deviation of a double array
        /// </summary>
        /// <param name="values">An array of doubles</param>
        /// <returns>The standard deviation.</returns>
        public static double StandardDeviation(this double[] values)
        {
            int n = values.Length;
            if (n < 2) return double.NaN;
            double var = 0;
            double mean = values.Average();
            foreach (double x in values)
                var += (x - mean) * (x - mean);
            return Math.Sqrt(var / (n - 1));
        }

        public static decimal? StandardDeviation(this decimal[] values)
        {
            int n = values.Length;
            if (n < 2) return null;
            decimal var = 0;
            decimal mean = values.Average();
            foreach (decimal x in values)
                var += (x - mean) * (x - mean);
            return (decimal)Math.Sqrt((double)(var / (n - 1)));
        }


        /// <summary>
        /// Quick and dirty extension to calculate the span of a double array.
        /// </summary>
        /// <param name="values">An array of doubles</param>
        /// <returns>The span.</returns>
        public static double Span(this double[] values)
        {
            return values.Max() - values.Min();
        }

        public static decimal Span(this decimal[] values)
        {
            return values.Max() - values.Min();
        }

    }
}
