using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiments
{
    /// <summary>
    /// Used to rotate the decimal representation of the inverse of a prime number.
    /// </summary>
    public static class PrimeInverseRotation
    {
        // this value enables rotation of the cycle by one decimal at a time
        private const int PowerLimit = 10;
        
        private static readonly Dictionary<string, double[]> Cache = new();
        
        /// <summary>
        /// Rotates the inverse of the divisor by the specified angle for the specified cycle.
        /// </summary>
        /// <param name="divisor">The divisor to rotate.</param>
        /// <param name="angle">The number of digits to rotate by.</param>
        /// <param name="cycle">The cycle to use.</param>
        /// <returns>The rotated decimal number of the divisor.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Some divisors have multiple rotations dependent on which cycle is used.
        /// The maximum number of cycles is equal to the divisor minus 1.
        /// </remarks>
        public static double Rotate(double divisor, int angle, int cycle = 1)
        {
            if (cycle <= 0)
                throw new ArgumentException("The cycle must be greater than 0.");
            
            if (divisor == 0)
                throw new ArgumentException("The divisor must be greater than 0.");

            var key = $"{divisor}:{cycle}";
            if (!Cache.TryGetValue(key, out var dividendCycle))
            {
                dividendCycle = CalculateDividendCycle(divisor, cycle);
                Cache.TryAdd(key, dividendCycle);
            }

            // fetch the pre-calculated dividend
            var cycleIndex = (angle - 1) % dividendCycle.Length; // subtract 1 since the cycle is zero indexed
            var dividend = dividendCycle[cycleIndex];

            // return the rotated value
            return dividend / divisor;
        }

        /// <summary>
        /// Calculates a cycle of dividends that can be used in sequence to rotate the inverse of a number.
        /// </summary>
        /// <returns>
        /// An array of precalculated dividends to use when rotating the inverse of the divisor. 
        /// </returns>
        private static double[] CalculateDividendCycle(double divisor, int cycle)
        {
            // setup the dividend to start on the correct cycle
            double dividend = cycle;
            var maxCycleLength = divisor - 1; // the max cycle length for divisor n is n-1 

            var cycles = new HashSet<double>();
            for (var i = 0; i < maxCycleLength; i++)
            {
                // calculate the next dividend
                var pow = i == 0 ? 1 : PowerLimit;
                dividend = (pow * dividend) % divisor;

                // break if we have all the values in the dividend cycle
                if (!cycles.Add(dividend))
                    break;
            }
            return cycles.ToArray();
        }
        
        /// <summary>
        /// Calculates the digital root of a number.
        /// </summary>
        /// <param name="n">The number to calculate the root of.</param>
        /// <returns>The digital root.</returns>
        public static int DigitalRoot(int n)
        {
            return 1 + (n - 1) % 9;
        }
    }
}