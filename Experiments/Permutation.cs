using System;
using System.Linq;

namespace Experiments
{
    /// <summary>
    /// Represents a permutation.
    /// </summary>
    /// <param name="digits">The number of digits in the permutation.</param>
    /// <param name="maxValue">The maximum value of any digit in the permutation.</param>
    public class Permutation(int digits, int maxValue)
    {
        // Counts the number of digits in the permutation that equal the maxValue.
        private int maxValueHits;

        /// <summary>
        /// The current state of the permutation.
        /// </summary>
        public readonly int[] Current = new int[digits];

        /// <summary>
        /// True if the permutation has gone through a full cycle; Otherwise, False. 
        /// </summary>
        public bool HasCompletedCycle => maxValueHits == digits;

        /// <summary>
        /// Computes the next permutation in the sequence from right to left.
        /// </summary>
        public void MoveNext()
        {
            maxValueHits = 0;
            var hasIncrementedDigit = false;
            
            for (var i = 0; i < digits; i++)
            {
                if (hasIncrementedDigit) // Only evaluate if we've reach maxValue for each digit.
                {
                    maxValueHits += Current[i] == maxValue ? 1 : 0;
                    continue;
                }
                
                if (Current[i] == maxValue)
                {
                    // Reset the digit as it equals maxValue.
                    Current[i] = 0;
                    continue;
                }
                
                // Increment the digit as we have not reached maxValue.
                Current[i]++;
                maxValueHits += Current[i] == maxValue ? 1 : 0;
                    
                hasIncrementedDigit = true;
            }
        }

        /// <summary>
        /// Sets the state of the permutation.
        /// </summary>
        /// <param name="state">The state to set.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Set(int[] state)
        {
            if (state.Length != digits)
            {
                throw new ArgumentOutOfRangeException(nameof(state), $"An array of length {digits} is required.");
            }
            
            if (state.Any(x => x > maxValue))
            {
                throw new ArgumentException($"Permutation must contain values less than {maxValue}.");
            }

            maxValueHits = 0;
            
            for (var i = 0; i < digits; i++)
            {
                Current[i] = state[i];
                maxValueHits += state[i] == maxValue ? 1 : 0;
            }
        }

        public override string ToString()
        {
            return string.Join(',', Current);
        }

        public override bool Equals(object obj)
        {
            if (obj is Permutation permutation)
            {
                return permutation.ToString() == ToString();
            }
            return false;
        }
    }
}