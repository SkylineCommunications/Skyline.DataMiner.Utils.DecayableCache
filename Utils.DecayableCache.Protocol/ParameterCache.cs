using System.Collections.Concurrent;

namespace Utils.DecayableCache.Protocol
{
	/// <summary>
	/// Holds a collection of parameters of a certain user-defined type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ParameterCache<T>
	{
		private readonly ConcurrentDictionary<int, CachedParameter<T>> _parameters = new ConcurrentDictionary<int, CachedParameter<T>>();
		
		public CachedParameter<T> GetParameter(int paramId)
		{
			CachedParameter<T> existingValue;
			CachedParameter<T> newValue = null;

			while (!_parameters.TryGetValue(paramId, out existingValue))
			{
				if (newValue == null)
				{
					newValue = new CachedParameter<T>(paramId);
				}

				if (_parameters.TryAdd(paramId, newValue))
				{
					return newValue;
				}
			}

			return existingValue;
		}
	}
}
