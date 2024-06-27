using System;
using System.IO;
using System.Text;

namespace Experiments
{
    /// <summary>
    /// Represents a "non-algebraic invertible wave function". 
    /// </summary>
    /// <remarks>
    /// I was experimenting with creating an invertible wave function that acts somewhat like a sine or cosine wave.
    /// 
    /// This wave function is not technically invertible from an algebraic perspective but it does produce data in
    /// such a way so as to technically be considered invertible.
    /// 
    /// e.g. setting x = 0.5 yields y = 0,8660254037844386
    /// e.g. setting x = 0,8660254037844386 yields y = 0,4999999999999999
    /// </remarks>
    public static class InvertibleWaveFunction
    {
        public static void GenerateExcelFile(int fromX, int toX, double increment)
        {
            var data = new StringBuilder();
            
            for (double x = fromX; x <= toX; x += increment)
            {
                data.AppendLine($"{x};{WaveFn(x)};{WaveFnInverse(x)}");
            }
            
            File.WriteAllText("waveFn.csv", data.ToString());
        }
        
        public static double WaveFn(double x)
        {
            var origX = x;

            var isNegative = (x < 0);
            if (isNegative)
            {
                x = MapNegativeValue(x);
            }

            var intX = Math.Truncate(x);

            var xShift = CalcOddShift(intX);
            var yShift = CalcEventShift(intX);
            var toggle = Math.Pow(-1, intX);
            var xDiffSquared = Math.Pow(xShift - x, 2);
            var ans = toggle * Math.Sqrt(1 - xDiffSquared) + yShift;

            if (!isNegative)
            {
                return ans;
            }

            ans += CalcNegativeValueShift(origX);
            return ans;
        }

        public static double WaveFnInverse(double x)
        {
            var origX = x;

            var isNegative = (x < 0);
            if (isNegative)
            {
                x = MapNegativeValue(x);
            }

            var intX = Math.Truncate(x);

            var xShift = CalcEventShift(intX);
            var yShift = CalcOddShift(intX);
            var toggle = Math.Pow(-1, intX + 1);
            var xDiffSquared = Math.Pow(x - xShift, 2);
            var ans = toggle * Math.Sqrt(1 - xDiffSquared) + yShift;

            if (!isNegative)
            {
                return ans;
            }

            ans += CalcNegativeValueShift(origX);
            return ans;
        }

        private static double CalcOddShift(double x)
        {
            return x + 0.5 + (0.5 * Math.Pow(-1, x));
        }

        private static double CalcEventShift(double x)
        {
            return x + 0.5 + (0.5 * Math.Pow(-1, x + 1));
        }

        private static double CalcNegativeValueShift(double x)
        {
            var isEven = (int) x % 2 == 0;
            if (isEven)
            {
                return CalcEventShift(Math.Truncate(x)) * 2 - 2;
            }
            return CalcOddShift(Math.Truncate(x)) * 2 - 2;
        }

        private static double MapNegativeValue(double x)
        {
            var xShift = Math.Abs(x) + 1; // use the curve of abs(x) + 1 for the given negative x-value
            var integerPart = Math.Truncate(xShift);
            var fractionalPart = xShift - integerPart;
            var adjustedFractionalPart = 1 - fractionalPart; // mapping-rule: 0.9 -> 0.1, 0.8 -> 0.2, 0.7 -> 0.3 ...
            var adjustedValue = integerPart + adjustedFractionalPart;
            return adjustedValue;
        }
    }
}