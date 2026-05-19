using System;
using Autofac.Core;

namespace Autofac.Features.Indexed
{
	internal class KeyedServiceIndex<TKey, TValue> : IIndex<TKey, TValue>
	{
		private readonly IComponentContext _context;

		public TValue this[TKey key] => (TValue)_context.ResolveService(GetService(key));

		public KeyedServiceIndex(IComponentContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			_context = context;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (_context.TryResolveService(GetService(key), out var instance))
			{
				value = (TValue)instance;
				return true;
			}
			value = default(TValue);
			return false;
		}

		private static KeyedService GetService(TKey key)
		{
			return new KeyedService(key, typeof(TValue));
		}
	}
}
