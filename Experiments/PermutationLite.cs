namespace Experiments
{
    /// <summary>
    /// Represents a lightweight implementation of a <see cref="Permutation"/>.
    /// </summary>
    /// <param name="digits">The number of digits in the permutation.</param>
    /// <param name="maxValue">The maximum value of any digit in the permutation.</param>
    public class PermutationLite(int digits, int maxValue)
    {
        public readonly int[] Current = new int[digits];

        public void MoveNext()
        {
            for (var i = 0; i < digits; i++)
            {
                if (Current[i] == maxValue)
                {
                    Current[i] = 0;
                    continue;
                }
                
                Current[i]++;
                return;
            }
        }

        public override string ToString()
        {
            return string.Join(',', Current);
        }
    }
}