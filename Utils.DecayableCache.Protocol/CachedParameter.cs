namespace Utils.DecayableCache.Protocol
{
	/// <summary>
	/// A wrapper for a cached parameter that adds context regarding locking and initialization.
	/// </summary>
	/// <typeparam name="T">The stored data type.</typeparam>
	public class CachedParameter<T>
	{
		/// <summary>
		/// Initializes a cached parameter with the specified parameter ID. The value will be remain empty (default), and IsInitialized will be false.
		/// </summary>
		/// <param name="paramId">The parameter identifier, a positive integer.</param>
		public CachedParameter(int paramId)
		{
			ParameterID = paramId;
		}

		/// <summary>
		/// The parameter ID to identify a value after being retrieved from the cache.
		/// </summary>
		public int ParameterID { get; private set; }

		private object _lock = new object();

		/// <summary>
		/// An object to lock the thread against when accessing this data in a concurrent context.
		/// </summary>
		public object Lock => _lock;

		/// <summary>
		/// A flag to know if the value has been initialized in case of values types that have no empty default value.
		/// </summary>
		public bool IsInitialized { get; set; }

		/// <summary>
		/// The stored value, not inherently thread-safe, unless locked appropriately.
		/// </summary>
		public T Value { get; set; }
	}
}
