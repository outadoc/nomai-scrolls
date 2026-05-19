using System;

namespace Autofac.Core.Lifetime
{
	public class LifetimeScopeEndingEventArgs : EventArgs
	{
		private readonly ILifetimeScope _lifetimeScope;

		public ILifetimeScope LifetimeScope => _lifetimeScope;

		public LifetimeScopeEndingEventArgs(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}
	}
}
