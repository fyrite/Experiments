using System;
using NUnit.Framework;

namespace Experiments.Tests
{
    [TestFixture]
    public class PermutationTests
    {
        [Test]
        public void InitializedToZeros()
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act

            // Assert
            var str = string.Join(',', permutation.Current);
            Assert.That(str, Is.EqualTo("0,0"));
        }
        
        [Test]
        public void MoveNext_ComputesNextPermutation()
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act
            permutation.MoveNext();

            // Assert
            var str = string.Join(',', permutation.Current);
            Assert.That(str, Is.EqualTo("1,0"));
        }
        
        [TestCase(new []{0})]
        [TestCase(new []{2, 1, 0})]
        public void Set_InvalidArrayLength_ThrowsException(int[] state)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => permutation.Set(state));
        }
        
        [TestCase(new []{0, 3})]
        [TestCase(new []{4, 3})]
        [TestCase(new []{4, 0})]
        public void Set_ExceedsMaxValue_ThrowsException(int[] state)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act / Assert
            Assert.Throws<ArgumentException>(() => permutation.Set(state));
        }
        
        [TestCase(new []{0, 0}, "0,0")]
        [TestCase(new []{1, 2}, "1,2")]
        [TestCase(new []{2, 1}, "2,1")]
        [TestCase(new []{2, 2}, "2,2")]
        public void Set_SetsCurrentState(int[] state, string expectedState)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act
            permutation.Set(state);

            // Assert
            var str = string.Join(',', permutation.Current);
            Assert.That(str, Is.EqualTo(expectedState));
        }
        
        [TestCase(new []{0, 0}, "1,0")]
        [TestCase(new []{1, 0}, "2,0")]
        [TestCase(new []{2, 0}, "0,1")]
        [TestCase(new []{0, 1}, "1,1")]
        [TestCase(new []{1, 1}, "2,1")]
        [TestCase(new []{2, 1}, "0,2")]
        [TestCase(new []{0, 2}, "1,2")]
        [TestCase(new []{1, 2}, "2,2")]
        [TestCase(new []{2, 2}, "0,0")]
        public void MoveNext_UpdatesTheCurrentState(int[] state, string nextState)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            permutation.Set(state);
            
            // Act
            permutation.MoveNext();

            // Assert
            var str = string.Join(',', permutation.Current);
            Assert.That(str, Is.EqualTo(nextState));
        }
        
        [TestCase(new []{0, 0}, false)]
        [TestCase(new []{1, 0}, false)]
        [TestCase(new []{2, 0}, false)]
        [TestCase(new []{0, 1}, false)]
        [TestCase(new []{1, 1}, false)]
        [TestCase(new []{2, 1}, false)]
        [TestCase(new []{0, 2}, false)]
        [TestCase(new []{1, 2}, false)]
        [TestCase(new []{2, 2}, true)]
        public void Set_UpdatesHasCompletedCycle(int[] state, bool hasCompletedCycle)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            
            // Act
            permutation.Set(state);
            
            // Assert
            Assert.That(permutation.HasCompletedCycle, Is.EqualTo(hasCompletedCycle));
        }
        
        [TestCase(new []{0, 0}, false)]
        [TestCase(new []{1, 0}, false)]
        [TestCase(new []{2, 0}, false)]
        [TestCase(new []{0, 1}, false)]
        [TestCase(new []{1, 1}, false)]
        [TestCase(new []{2, 1}, false)]
        [TestCase(new []{0, 2}, false)]
        [TestCase(new []{1, 2}, true)]
        [TestCase(new []{2, 2}, false)]
        public void MoveNext_UpdatesHasCompletedCycle(int[] state, bool hasCompletedCycle)
        {
            // Arrange
            var permutation = new Permutation(2, 2);
            permutation.Set(state);
            
            // Act
            permutation.MoveNext();
            
            // Assert
            Assert.That(permutation.HasCompletedCycle, Is.EqualTo(hasCompletedCycle));
        }
        
        [Test]
        public void MoveNext_PermutationLiteSuccess()
        {
            // Arrange
            var permutationLite = new PermutationLite(2, 2);
            var permutation = new Permutation(2, 2);
            
            // Act
            while (true)
            {
                // Assert
                Assert.That(permutation.ToString(), Is.EqualTo(permutationLite.ToString()));
                
                if (permutation.HasCompletedCycle) 
                    break;
                
                permutation.MoveNext();
                permutationLite.MoveNext();
            }
        }
    }
}