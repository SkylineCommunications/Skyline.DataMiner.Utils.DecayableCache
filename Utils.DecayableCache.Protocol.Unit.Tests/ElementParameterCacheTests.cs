using System;
using System.Threading;

using NUnit.Framework;

namespace Utils.DecayableCache.Protocol.Unit.Tests
{
	[TestFixture]
	public class ElementParameterCacheTests
	{
		[Test]
		public void StaleTimerRefreshTest()
		{
			var cache = new GlobalParameterCache<bool>(TimeSpan.FromMilliseconds(250));

			var cachedParameter = cache.GetParameter(1, 1, 1);

			cachedParameter.IsInitialized = true;

			Thread.Sleep(200);

			Assert.That(cache.GetParameter(1, 1, 1).IsInitialized, Is.True);

			Thread.Sleep(200);

			Assert.That(cache.GetParameter(1, 1, 1).IsInitialized, Is.True);

			// Wait long enough so 3 timer cycles have passed and our value is definitely deleted.
			// Trying to get the data too early would just extend its lifetime.
			Thread.Sleep(400);

			Assert.That(cache.GetParameter(1, 1, 1).IsInitialized, Is.False);
		}

		[Test]
		public void UniqueValueRetrievalTest()
		{
			var cache = new GlobalParameterCache<int>(TimeSpan.FromMilliseconds(250));

			cache.GetParameter(1, 1, 1).Value = 111;
			cache.GetParameter(1, 1, 2).Value = 112;
			cache.GetParameter(1, 2, 1).Value = 121;
			cache.GetParameter(2, 1, 1).Value = 211;

			Assert.That(cache.GetParameter(1, 1, 1).Value, Is.EqualTo(111));
			Assert.That(cache.GetParameter(1, 1, 2).Value, Is.EqualTo(112));
			Assert.That(cache.GetParameter(1, 2, 1).Value, Is.EqualTo(121));
			Assert.That(cache.GetParameter(2, 1, 1).Value, Is.EqualTo(211));
		}
	}
}