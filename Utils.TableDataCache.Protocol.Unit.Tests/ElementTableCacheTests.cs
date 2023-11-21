using System;
using System.Threading;
using NUnit.Framework;

namespace Utils.TableDataCache.Protocol.Unit.Tests
{
    [TestFixture]
    public class ElementTableCacheTests
    {
        [Test]
        public void StaleTimerTest()
        {
            var cache = new TableDataCache(new TimeSpan(0, 0, 500));

            var cachedTable = cache.GetTable(1, 1, 1);

            cachedTable.IsInitialized = true;
            
            Thread.Sleep(1000);
            
            Assert.IsFalse(cache.GetTable(1, 1, 1).IsInitialized);
        }
    }
}