using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Util;

namespace Autofac.Core
{
	[DebuggerDisplay("Tag = {Tag}, IsDisposed = {IsDisposed}")]
	public class Container : Disposable, IContainer, ILifetimeScope, IComponentContext, IDisposable, IServiceProvider
	{
		private readonly IComponentRegistry _componentRegistry;

		private readonly ILifetimeScope _rootLifetimeScope;

		public IDisposer Disposer => _rootLifetimeScope.Disposer;

		public object Tag => _rootLifetimeScope.Tag;

		public IComponentRegistry ComponentRegistry => _componentRegistry;

		public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
		{
			add
			{
				_rootLifetimeScope.ChildLifetimeScopeBeginning += value;
			}
			remove
			{
				_rootLifetimeScope.ChildLifetimeScopeBeginning -= value;
			}
		}

		public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
		{
			add
			{
				_rootLifetimeScope.CurrentScopeEnding += value;
			}
			remove
			{
				_rootLifetimeScope.CurrentScopeEnding -= value;
			}
		}

		public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
		{
			add
			{
				_rootLifetimeScope.ResolveOperationBeginning += value;
			}
			remove
			{
				_rootLifetimeScope.ResolveOperationBeginning -= value;
			}
		}

		internal Container()
		{
			_componentRegistry = new ComponentRegistry();
			_componentRegistry.Register(new ComponentRegistration(LifetimeScope.SelfRegistrationId, new DelegateActivator(typeof(LifetimeScope), delegate
			{
				throw new InvalidOperationException(ContainerResources.SelfRegistrationCannotBeActivated);
			}), new CurrentScopeLifetime(), InstanceSharing.Shared, InstanceOwnership.ExternallyOwned, new Service[2]
			{
				new TypedService(typeof(ILifetimeScope)),
				new TypedService(typeof(IComponentContext))
			}, new Dictionary<string, object>()));
			_rootLifetimeScope = new LifetimeScope(_componentRegistry);
		}

		public ILifetimeScope BeginLifetimeScope()
		{
			return _rootLifetimeScope.BeginLifetimeScope();
		}

		public ILifetimeScope BeginLifetimeScope(object tag)
		{
			return _rootLifetimeScope.BeginLifetimeScope(tag);
		}

		public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
		{
			return _rootLifetimeScope.BeginLifetimeScope(configurationAction);
		}

		public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
		{
			return _rootLifetimeScope.BeginLifetimeScope(tag, configurationAction);
		}

		public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			return _rootLifetimeScope.ResolveComponent(registration, parameters);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_rootLifetimeScope.Dispose();
				_componentRegistry.Dispose();
			}
			base.Dispose(disposing);
		}

		public object GetService(Type serviceType)
		{
			return ((IServiceProvider)_rootLifetimeScope).GetService(serviceType);
		}
	}
}
