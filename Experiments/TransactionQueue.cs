using System.Collections.Generic;

namespace Experiments
{
	/// <summary>
	/// Represents a queue whose items can be reserved through the creation of a transaction. 
	/// </summary>
	/// <typeparam name="TValue">The type of item to be added to the queue.</typeparam>
	/// <remarks>
	/// This is the creation of a thought experiment. We wanted to know if we could create a queue where we could
	/// reserve an item on the queue, not remove it, process the item and remove it from the queue once completed
	/// processing.
	///
	/// We also wanted to be able to re-add the item back onto the queue if we could not process it at that time
	/// for some reason which would allow the item to be processed at a later point.
	///
	/// It works by containing both a queue to store items in the queue and also by containing a dictionary of
	/// all transactions that have been created. When a transaction is created it is removed from the queue and added
	/// to the collection of transactions. Each transaction contains a key associated with it and which is used to
	/// close or discard a transaction or update the value associated with the transaction i.e. the queue item.
	/// </remarks>
	public class TransactionQueue<TValue>
	{
		private ulong keyCounter;
		
		private readonly object locker = new();
		private readonly Queue<Item> queue = new();
		private readonly Dictionary<ulong, Item> transactions = new();

		/// <summary>
		/// The total number of items in the queue.
		/// </summary>
		public int Count 
		{
			get
			{
				lock (locker)
				{
					return queue.Count;
				}
			}
		}

		/// <summary>
		/// The total number of currently created transactions.
		/// </summary>
		public int TransactionCount
		{
			get
			{
				lock (locker)
				{
					return transactions.Count;
				}
			}
		}

		/// <summary>
		/// Add a value to the end of the queue.
		/// </summary>
		public ulong Enqueue(TValue value)
		{
			lock (locker)
			{
				var item = new Item(++keyCounter, value);
				queue.Enqueue(item);
			
				return keyCounter;
			}
		}

		/// <summary>
		/// Remove a value from the queue and place it in the transaction collection.
		/// </summary>
		public TValue CreateTransaction(out ulong key)
		{
			lock (locker)
			{
				var item = queue.Dequeue();
				transactions.Add(item.Key, item);
			
				key = item.Key;
				return item.Value;
			}
		}

		/// <summary>
		/// Update the value in the transaction collection.
		/// </summary>
		public void UpdateTransaction(ulong key, TValue value)
		{
			lock (locker)
			{
				transactions[key].Value = value;
			}
		}

		/// <summary>
		/// Put the value associated with the key back onto the queue for processing and remove it from the transaction collection.
		/// </summary>
		public void DiscardTransaction(ulong key)
		{
			lock (locker)
			{
				var item = transactions[key];
				queue.Enqueue(item);
			
				transactions.Remove(key);
			}
		}

		/// <summary>
		/// Remove the value from the transaction collection.
		/// </summary>
		public void CloseTransaction(ulong key)
		{
			lock (locker)
			{
				transactions.Remove(key);	
			}
		}

		/// <summary>
		/// Removes all objects from the collection.
		/// </summary>
		public void Clear()
		{
			lock (locker)
			{
				keyCounter = 0;
				queue.Clear();
				transactions.Clear();	
			}
		}

		private class Item(ulong key, TValue value)
		{
			public ulong Key { get; } = key;

			public TValue Value { get; set; } = value;
		}
	}
}
