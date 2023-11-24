using System;
using Utils.DecayableCache.Common;

namespace Utils.DecayableCache.Protocol
{
	/// <summary>
	/// A thread-safe cache for parameter data that can hold multiple elements.
	/// Requires a DataMiner and element ID to reference a particular ParameterCache.
	/// Will clear inactive entries when they breach the stale data lifetime.
	/// </summary>
	public class GlobalParameterCache<T> : IDisposable
	{
		private readonly DecayableCache<long, ParameterCache<T>> _cachesPerElement;

		/// <summary>
		/// Initializes the cache where stale data is deleted automatically if it has not been fetched for more than <paramref name="staleDataLifeTime"/> time.
		/// The minimum life time is 250 milliseconds.
		/// The actual timespan should take into consideration how often this data is accessed to allow for a swift deletion when an element has become inactive.
		/// For example, take 3 times the poll cycle time, assuming an element in timeout will still access the data.
		/// The age of the objects is checked every <paramref name="staleDataLifeTime"/> divided by two.
		/// </summary>
		/// <param name="staleDataLifeTime"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public GlobalParameterCache(TimeSpan staleDataLifeTime)
		{
			_cachesPerElement = new DecayableCache<long, ParameterCache<T>>(staleDataLifeTime);
		}

		/// <summary>
		/// Gets the object that can hold the parameter data. The contained IsInitialized will be false when newly constructed.
		/// The DataMiner ID and element ID combination will be refreshed and postpone its data deletion.
		/// </summary>
		/// <param name="dataminerId">The DataMiner ID of the element.</param>
		/// <param name="elementId">The element ID.</param>
		/// <param name="parameterId">The parameter ID of the cached data.</param>
		/// <returns></returns>
		public CachedParameter<T> GetParameter(int dataminerId, int elementId, int parameterId)
		{
			var cacheForElement = GetParameterCacheForElement(dataminerId, elementId);
			return cacheForElement.GetParameter(parameterId);
		}

		private ParameterCache<T> GetParameterCacheForElement(int dataminerId, int elementId)
		{
			var key = (long)dataminerId << 32 | (long)elementId;

			ParameterCache<T> existingValue;
			ParameterCache<T> newValue = null;

			while (!_cachesPerElement.TryGetValue(key, out existingValue))
			{
				if (newValue == null)
				{
					newValue = new ParameterCache<T>();
				}

				if (_cachesPerElement.TryAdd(key, newValue))
				{
					return newValue;
				}
			}

			return existingValue;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_cachesPerElement?.Dispose();
		}
	}
}