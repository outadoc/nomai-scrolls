using System;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;

namespace Autofac
{
	public interface ILifetimeScope : IComponentContext, IDisposable
	{
		IDisposer Disposer { get; }

		object Tag { get; }

		event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;

		event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;

		event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

		ILifetimeScope BeginLifetimeScope();

		ILifetimeScope BeginLifetimeScope(object tag);

		ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction);

		ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction);
	}
}
