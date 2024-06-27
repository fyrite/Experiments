using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Experiments.Tests
{
    public class TransactionQueueTest
    {
        private TransactionQueue<int> queue;

        [SetUp]
        public void Setup()
        {
            queue = new TransactionQueue<int>();
        }

        [TearDown]
        public void TearDown()
        {
            queue = null;
        }

        [Test]
        public void QueueCountStartsAtZero()
        {
            // Assert
            Assert.That(queue.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanEnqueueItem()
        {
            // Arrange / Act
            queue.Enqueue(1);

            // Assert
            Assert.That(queue.Count, Is.EqualTo(1));
        }

        [Test]
        public void CloseTransactionOnLastItemYieldsZeroQueueSize()
        {
            // Arrange
            queue.Enqueue(1);
            queue.CreateTransaction(out var key);

            // Act
            queue.CloseTransaction(key);

            // Assert
            Assert.That(queue.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateTransactionReturnsCorrectKey()
        {
            // Arrange
            var key1 = queue.Enqueue(1);

            // Act
            queue.CreateTransaction(out var key2);

            // Assert
            Assert.That(key1, Is.EqualTo(key2));
        }

        [Test]
        public void CanUpdateValueInTransactionQueue()
        {
            // Arrange
            queue.Enqueue(1);
            queue.CreateTransaction(out var key);

            // Act
            queue.UpdateTransaction(key, 2);
            queue.DiscardTransaction(key);
            var item = queue.CreateTransaction(out _);

            // Assert
            Assert.That(item, Is.EqualTo(2));
        }

        [Test]
        public void CanEnqueueMultipleItems()
        {
            // Arrange / Act
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);

            Assert.That(queue.Count, Is.EqualTo(4));
        }

        [Test]
        public void KeyReturnedFromEnqueueEqualsQueueCount()
        {
            // Arrange
            queue.Enqueue(10);
            queue.Enqueue(11);

            // Act
            var key = queue.Enqueue(12);

            Assert.That(key, Is.EqualTo(queue.Count));
        }

        [Test]
        public void CanReserveItem()
        {
            // Arrange
            queue.Enqueue(1);

            // Act
            var item = queue.CreateTransaction(out _);

            // Assert
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.TransactionCount, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(1));
        }

        [Test]
        public void CanReserveMultipleItemsIndividually()
        {
            // Arrange
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);

            // Act / Assert
            var item = queue.CreateTransaction(out _);
            Assert.That(item, Is.EqualTo(1));

            item = queue.CreateTransaction(out _);
            Assert.That(item, Is.EqualTo(2));

            item = queue.CreateTransaction(out _);
            Assert.That(item, Is.EqualTo(3));

            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.TransactionCount, Is.EqualTo(3));
        }

        [Test]
        public void ClearResetsCounts()
        {
            // Arrange
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);

            queue.CreateTransaction(out _);
            queue.CreateTransaction(out _);

            // Act
            queue.Clear();

            // Assert
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.TransactionCount, Is.EqualTo(0));
        }

        [Test]
        public void CanUnreserveItem()
        {
            // Arrange
            queue.Enqueue(1);
            queue.CreateTransaction(out var key);

            // Act
            queue.DiscardTransaction(key);

            // Assert
            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.TransactionCount, Is.EqualTo(0));
        }

        [Test]
        public void CanRemoveItem()
        {
            // Arrange
            queue.Enqueue(1);
            queue.CreateTransaction(out var key);

            // Act
            queue.CloseTransaction(key);

            // Assert
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.TransactionCount, Is.EqualTo(0));
        }

        [Test]
        public void CreateTransactionPullFromFrontOfQueue()
        {
            // Arrange
            queue.Enqueue(1);
            queue.Enqueue(2);

            // Act
            var item = queue.CreateTransaction(out var key);

            // Assert
            Assert.That(item, Is.EqualTo(1));
        }

        [Test]
        public void DiscardTransactionAddsToBackOfQueue()
        {
            // Arrange
            queue.Enqueue(1);
            queue.Enqueue(2);

            // Act
            var item = queue.CreateTransaction(out var key);
            queue.DiscardTransaction(key); // adds first item to the back of the queue

            // Assert
            item = queue.CreateTransaction(out key); // reserves next item in queue
            Assert.That(item, Is.EqualTo(2));
            queue.CloseTransaction(key);

            item = queue.CreateTransaction(out key); // reserves last item in queue
            Assert.That(item, Is.EqualTo(1));
        }

        [Test]
        public void CanEnqueueOnMultipleThreads()
        {
            const int threadCount = 16;
            const ulong iterations = 1000 * 100;

            var threads = new List<Thread>();
            var tcs = new TaskCompletionSource<int>();

            for (var i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(s =>
                {
                    tcs.Task.Wait();
                    for (ulong j = 0; j < iterations; j++)
                    {
                        queue.Enqueue(1);
                    }
                }));
            }

            threads.ForEach(t => t.Start());
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());

            Assert.That(queue.Count, Is.EqualTo(threadCount * (int) iterations));
        }

        [Test]
        public void OperationsReadWriteThreadSafe()
        {
            const int threadCount = 16;
            const ulong iterations = 1000 * 100;

            var threads = new List<Thread>();
            var tcs = new TaskCompletionSource<int>();

            for (var i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(s =>
                {
                    tcs.Task.Wait();
                    for (ulong j = 0; j < iterations; j++)
                    {
                        queue.Enqueue(1);
                        var value = queue.CreateTransaction(out var key);
                        queue.UpdateTransaction(key, value);
                        queue.DiscardTransaction(key);
                        queue.CreateTransaction(out key);
                        queue.CloseTransaction(key);
                    }
                }));
            }

            threads.ForEach(t => t.Start());
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());

            Assert.That(queue.Count, Is.EqualTo(0));
        }
    }
}