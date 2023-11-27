using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Utils.DecayableCache.Common
{
	/// <summary>
	/// Builds a temporary cache for objects of type T. Items are considered fresh when they are accessed by a TryGet within the stale time span. Stale items are occasionally removed using a timer.
	/// </summary>
	/// <typeparam name="TKey">The type to index values with.</typeparam>
	/// /// <typeparam name="TValue">The type to store for a unique key.</typeparam>
	public class DecayableCache<TKey, TValue> : IDisposable
	{
		private readonly ConcurrentDictionary<TKey, CacheValue<TValue>> _temporaryValues = new ConcurrentDictionary<TKey, CacheValue<TValue>>();

		private readonly TimeSpan _staleDataLifeTime;

		private readonly Timer _staleDataTimer;

		private readonly bool _forceGc;

		private volatile bool _isBusyPruning;

		/// <summary>
		/// Initializes the cache where stale data is deleted automatically if it has not been fetched for more than <paramref name="staleDataLifeTime"/> time.
		/// The minimum life time is 250 milliseconds.
		/// The age of the objects is checked every <paramref name="staleDataLifeTime"/> divided by two.
		/// The garbage collection is not forced.
		/// </summary>
		/// <param name="staleDataLifeTime">The time over which an item is considered stale if it has not been accessed.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public DecayableCache(TimeSpan staleDataLifeTime) : this(staleDataLifeTime, new TimeSpan(staleDataLifeTime.Ticks / 2), false)
		{
		}

		/// <summary>
		/// Initializes the cache where stale data is deleted automatically if it has not been fetched for more than <paramref name="staleDataLifeTime"/> time.
		/// The minimum life time is 250 milliseconds.
		/// The age of the objects is checked every <paramref name="staleTimerInterval"/>.
		/// </summary>
		/// <param name="staleDataLifeTime">The time over which an item is considered stale if it has not been accessed.</param>
		/// <param name="staleTimerInterval">The interval between each stale item cleanup cycle. The resolution is in milliseconds.</param>
		/// <param name="forceGarbageCollection">Set whether the garbage collection needs to be forced after stale items have been removed.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public DecayableCache(TimeSpan staleDataLifeTime, TimeSpan staleTimerInterval, bool forceGarbageCollection)
		{
			if (staleDataLifeTime < TimeSpan.FromMilliseconds(250d))
			{
				throw new ArgumentOutOfRangeException(nameof(staleDataLifeTime),
					"The lifetime timespan should not be less than 250 ms to make use of this cache in a reasonable fashion.");
			}

			if (staleTimerInterval < TimeSpan.FromMilliseconds(100d))
			{
				throw new ArgumentOutOfRangeException(nameof(staleDataLifeTime),
					"The stale timer interval timespan should not be less than 100 ms to make use of this cache in a reasonable fashion.");
			}

			_forceGc = forceGarbageCollection;
			_staleDataLifeTime = staleDataLifeTime;
			var timerInterval = Convert.ToInt32(staleTimerInterval.TotalMilliseconds);
			_staleDataTimer = new Timer(StaleDataTimerCallback, null, timerInterval, timerInterval);
		}

		private void StaleDataTimerCallback(object state)
		{
			try
			{
				// In case the collection or GC.Collect are taking a long time, we abort a second simultaneous callback. 
				if (_isBusyPruning)
				{
					return;
				}
				
				// The flag is marked volatile and thus, atomic. We only expect a timer callback every >100ms, so no further synchronization is necessary.
				// Executing this function concurrently is still thread-safe, but we don't want to waste the threadpool on it.
				_isBusyPruning = true;

				bool hasRemovedAny = false;
				var staleDateTime = DateTime.UtcNow.Subtract(_staleDataLifeTime);

				foreach (var element in _temporaryValues)
				{

					if (element.Value.LastActivityUtc < staleDateTime)
					{
						hasRemovedAny |= _temporaryValues.TryRemove(element.Key, out _);
					}
				}

				if (_forceGc && hasRemovedAny)
				{
					GC.Collect();
				}
			}
			catch(Exception e)
			{
				// Throwing exceptions in the timer callback will not get logged and may cause process or thread termination.
				Console.WriteLine("DecayableCache StaleDataTimerCallback encountered a problem: " + e);
			}
			finally
			{
				_isBusyPruning = false;
			}
		}

		/// <summary>
		/// Attempts to add the specified key and value to the <see cref="DecayableCache{TKey,TValue}"/>.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. The value can be a null reference for reference types.</param>
		/// <returns>
		/// true if the key/value pair was added to the <see cref="DecayableCache{TKey,TValue}"/> successfully; otherwise, false.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		/// <exception cref="OverflowException">The <see cref="DecayableCache{TKey,TValue}"/> contains too many elements.</exception>
		public bool TryAdd(TKey key, TValue value)
		{
			return _temporaryValues.TryAdd(key, new CacheValue<TValue>(value));
		}

		/// <summary>
		/// Attempts to get the value associated with the specified key from the <see cref="DecayableCache{TKey,TValue}"/>.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">
		/// When this method returns, <paramref name="value"/> contains the object from
		/// the <see cref="DecayableCache{TKey,TValue}"/> with the specified key or the default value of
		/// <typeparamref name="TValue"/>, if the operation failed.
		/// </param>
		/// <returns>true if the key was found in the <see cref="DecayableCache{TKey,TValue}"/>; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (_temporaryValues.TryGetValue(key, out var cacheValue))
			{
				cacheValue.LastActivityUtc = DateTime.UtcNow;

				value = cacheValue.Value;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Attempts to remove and return the value with the specified key from the <see cref="DecayableCache{TKey,TValue}"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove and return.</param>
		/// <param name="value">
		/// When this method returns, <paramref name="value"/> contains the object removed from the
		/// <see cref="DecayableCache{TKey,TValue}"/> or the default value of <typeparamref
		/// name="TValue"/> if the operation failed.
		/// </param>
		/// <returns>true if an object was removed successfully; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		public bool TryRemove(TKey key, out TValue value)
		{
			if (_temporaryValues.TryRemove(key, out var cacheValue))
			{
				value = cacheValue.Value;
				return true;
			}

			value = default;
			return false;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_staleDataTimer?.Dispose();
		}
	}
}
