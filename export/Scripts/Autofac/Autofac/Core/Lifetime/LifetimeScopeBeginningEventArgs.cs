using System;

namespace Autofac.Core.Lifetime
{
	public class LifetimeScopeBeginningEventArgs : EventArgs
	{
		private readonly ILifetimeScope _lifetimeScope;

		public ILifetimeScope LifetimeScope => _lifetimeScope;

		public LifetimeScopeBeginningEventArgs(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}
	}
}
