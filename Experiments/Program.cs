﻿using System;

namespace Experiments
{
    public static class Program
    {
        public static void Main()
        {
            // Permutation();
            
            // PrimeRotation(31);
        }

        private static void Permutation()
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

        private static void PrimeRotation(int prime)
        {
            for (var cycle = 1; cycle < prime; cycle++)
            {
                for (var angle = 1; angle < prime; angle++)
                {
                    var rotated = PrimeInverseRotation.Rotate(prime, angle, cycle);
                    Console.WriteLine($"{cycle} : {angle} : {rotated}");
                }   
            }
        }
    }
}