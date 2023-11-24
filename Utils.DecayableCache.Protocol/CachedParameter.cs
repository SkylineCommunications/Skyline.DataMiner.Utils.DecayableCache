namespace Utils.DecayableCache.Protocol
{
	public class CachedParameter<T>
	{
		public CachedParameter(int paramId)
		{
			ParameterID = paramId;
		}

		public int ParameterID { get; private set; }

		private object _lock = new object();

		public object Lock => _lock;

		public bool IsInitialized { get; set; }

		public T Value { get; set; }
	}
}
