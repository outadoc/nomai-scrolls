using System.Collections.Generic;

namespace Autofac.Util
{
	internal class SafeDictionary<TKey, TValue>
	{
		private readonly object _syncLock = new object();

		private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

		public TValue this[TKey key]
		{
			set
			{
				lock (_syncLock)
				{
					_dictionary[key] = value;
				}
			}
		}

		public IEnumerable<TKey> Keys => _dictionary.Keys;

		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (_syncLock)
			{
				return _dictionary.TryGetValue(key, out value);
			}
		}

		public bool Remove(TKey key)
		{
			lock (_syncLock)
			{
				return _dictionary.Remove(key);
			}
		}

		public void Clear()
		{
			lock (_syncLock)
			{
				_dictionary.Clear();
			}
		}
	}
}
