using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Utils.TableDataCache.Protocol
{
	internal class ElementTableCache
	{
		private readonly ConcurrentDictionary<int, CachedTable> _tables = new ConcurrentDictionary<int, CachedTable>();

		private long _lastActivityUtcTicks = DateTime.UtcNow.Ticks;

		public DateTime LastActivityUtc
		{
			get => new DateTime(Interlocked.Read(ref _lastActivityUtcTicks));
			set => Interlocked.Exchange(ref _lastActivityUtcTicks, value.Ticks);
		}

		public CachedTable GetTable(int paramId)
		{
			LastActivityUtc = DateTime.UtcNow;

			CachedTable existingValue;
			CachedTable newValue = null;

			while (!_tables.TryGetValue(paramId, out existingValue))
			{
				if (newValue == null)
				{
					newValue = new CachedTable(paramId);
				}

				if (_tables.TryAdd(paramId, newValue))
				{
					return newValue;
				}
			}

			return existingValue;
		}
	}
}
