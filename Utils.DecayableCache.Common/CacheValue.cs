using System;
using System.Threading;

namespace Utils.DecayableCache.Common
{
	internal class CacheValue<T>
	{
		private long _lastActivityUtcTicks = DateTime.UtcNow.Ticks;

		public CacheValue(T value)
		{
			Value = value;
		}

		public DateTime LastActivityUtc
		{
			get => new DateTime(Interlocked.Read(ref _lastActivityUtcTicks));
			set => Interlocked.Exchange(ref _lastActivityUtcTicks, value.Ticks);
		}

		public T Value { get; private set; }
	}
}
