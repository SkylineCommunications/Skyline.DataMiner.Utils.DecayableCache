using System;
using System.Threading;
using NUnit.Framework;

namespace Utils.TableDataCache.Protocol.Unit.Tests
{
	[TestFixture]
	public class ElementTableCacheTests
	{
		[Test]
		public void StaleTimerRefreshTest()
		{
			var cache = new TableDataCache(TimeSpan.FromMilliseconds(250));

			var cachedTable = cache.GetTable(1, 1, 1);

			cachedTable.IsInitialized = true;

			Thread.Sleep(200);

			Assert.IsTrue(cache.GetTable(1, 1, 1).IsInitialized);

			Thread.Sleep(200);

			Assert.IsTrue(cache.GetTable(1, 1, 1).IsInitialized);

			// Wait long enough so 3 timer cycles have passed and our value is definitely deleted.
			// Trying to get the data too early would just extend its lifetime.
			Thread.Sleep(400);

			Assert.IsFalse(cache.GetTable(1, 1, 1).IsInitialized);
		}
	}
}