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

			Assert.IsTrue(cache.GetParameter(1, 1, 1).IsInitialized);

			Thread.Sleep(200);

			Assert.IsTrue(cache.GetParameter(1, 1, 1).IsInitialized);

			// Wait long enough so 3 timer cycles have passed and our value is definitely deleted.
			// Trying to get the data too early would just extend its lifetime.
			Thread.Sleep(400);

			Assert.IsFalse(cache.GetParameter(1, 1, 1).IsInitialized);
		}

		[Test]
		public void UniqueValueRetrievalTest()
		{
			var cache = new GlobalParameterCache<int>(TimeSpan.FromMilliseconds(250));

			cache.GetParameter(1, 1, 1).Value = 111;
			cache.GetParameter(1, 1, 2).Value = 112;
			cache.GetParameter(1, 2, 1).Value = 121;
			cache.GetParameter(2, 1, 1).Value = 211;

			Assert.AreEqual(111, cache.GetParameter(1, 1, 1).Value);
			Assert.AreEqual(112, cache.GetParameter(1, 1, 2).Value);
			Assert.AreEqual(121, cache.GetParameter(1, 2, 1).Value);
			Assert.AreEqual(211, cache.GetParameter(2, 1, 1).Value);
		}
	}
}