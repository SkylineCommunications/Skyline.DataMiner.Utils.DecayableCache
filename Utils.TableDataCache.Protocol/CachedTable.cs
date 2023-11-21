using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.TableDataCache.Protocol
{
	public class CachedTable
	{
		public CachedTable(int paramId)
		{
			ParameterID = paramId;
		}

		public int ParameterID { get; private set; }

		private object _lock = new object();

		public object Lock => _lock;

		public bool IsInitialized { get; set; }

		public readonly Dictionary<string, object[]> rows = new Dictionary<string, object[]>();
	}
}
