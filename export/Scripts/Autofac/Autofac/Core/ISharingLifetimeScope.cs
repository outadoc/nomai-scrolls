using System;

namespace Autofac.Core
{
	public interface ISharingLifetimeScope : ILifetimeScope, IComponentContext, IDisposable
	{
		ISharingLifetimeScope RootLifetimeScope { get; }

		ISharingLifetimeScope ParentLifetimeScope { get; }

		object GetOrCreateAndShare(Guid id, Func<object> creator);
	}
}
