using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Utils.TableDataCache.Protocol
{
	/// <summary>
	/// A thread-safe cache for table data.
	/// Requires a DataMiner and element ID to reference tables.
	/// Can be configured to clear inactive entries.
	/// </summary>
	public class TableDataCache
	{
		private readonly ConcurrentDictionary<long, ElementTableCache> _tablesPerElement = new ConcurrentDictionary<long, ElementTableCache>();

		private readonly TimeSpan _staleDataLifeTime;

		private Timer _staleDataTimer;

		/// <summary>
		/// 
		/// </summary>
		public TableDataCache()
		{
			_staleDataLifeTime = TimeSpan.MaxValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="staleDataLifeTime"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public TableDataCache(TimeSpan staleDataLifeTime)
		{
			if (staleDataLifeTime < TimeSpan.FromMilliseconds(500d))
			{
				throw new ArgumentOutOfRangeException(nameof(staleDataLifeTime),
					"The lifetime timespan should not be less than 500 ms to make use of this cache in a reasonable fashion. For an unlimited duration, use the empty constructor.");
			}

			_staleDataLifeTime = staleDataLifeTime;
			var timerInterval = Convert.ToInt32(staleDataLifeTime.TotalMilliseconds) / 2;
			_staleDataTimer = new Timer(StaleDataTimerCallback, null, timerInterval, timerInterval);
			_staleDataTimer.
		}

		private void StaleDataTimerCallback(object state)
		{
			foreach (var element in _tablesPerElement)
			{
				var now = DateTime.UtcNow;

				if (element.Value.LastActivityUtc.Add(_staleDataLifeTime) < now)
				{
					_tablesPerElement.TryRemove(element.Key, out _);
				}
			}
		}

		/// <summary>
		/// Gets the object that may hold the table data. Check <b>CachedTable.IsInitialized</b> to know if data has been loaded for this table already.
		/// </summary>
		/// <param name="dataminerId">The DataMiner ID of the element.</param>
		/// <param name="elementId">The element ID.</param>
		/// <param name="parameterId">The parameter ID of the table.</param>
		/// <returns></returns>
		public CachedTable GetTable(int dataminerId, int elementId, int parameterId)
		{
			var tablesForElement = GetTableCacheForElement(dataminerId, elementId);
			return tablesForElement.GetTable(parameterId);
		}

		private ElementTableCache GetTableCacheForElement(int dataminerId, int elementId)
		{
			var key = (long)dataminerId << 32 | (long)elementId;

			ElementTableCache existingValue;
			ElementTableCache newValue = null;

			while (!_tablesPerElement.TryGetValue(key, out existingValue))
			{
				if (newValue == null)
				{
					newValue = new ElementTableCache();
				}

				if (_tablesPerElement.TryAdd(key, newValue))
				{
					return newValue;
				}
			}

			return existingValue;
		}
	}
}