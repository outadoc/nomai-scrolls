using System;
using System.Threading;
using Autofac.Util;

namespace Autofac.Features.OwnedInstances
{
	public class Owned<T> : Disposable
	{
		private T _value;

		private IDisposable _lifetime;

		public T Value => _value;

		public Owned(T value, IDisposable lifetime)
		{
			_value = value;
			_lifetime = Enforce.ArgumentNotNull(lifetime, "lifetime");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				IDisposable disposable = Interlocked.Exchange(ref _lifetime, null);
				if (disposable != null)
				{
					_value = default(T);
					disposable.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
