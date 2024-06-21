using System;

namespace Experiments
{
    public static class Program
    {
        public static void Main()
        {
            var permutation = new Permutation(3, 2);
            while (true)
            {
                Console.WriteLine(permutation.ToString());

                if (permutation.HasCompletedCycle) 
                    break;
                
                permutation.MoveNext();
            }
        }
    }
}