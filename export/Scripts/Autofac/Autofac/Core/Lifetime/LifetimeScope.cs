using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Util;

namespace Autofac.Core.Lifetime
{
	[DebuggerDisplay("Tag = {Tag}, IsDisposed = {IsDisposed}")]
	public class LifetimeScope : Disposable, ISharingLifetimeScope, ILifetimeScope, IComponentContext, IDisposable, IServiceProvider
	{
		private readonly object _synchRoot = new object();

		private readonly IDictionary<Guid, object> _sharedInstances = new Dictionary<Guid, object>();

		private readonly IComponentRegistry _componentRegistry;

		private readonly ISharingLifetimeScope _root;

		private readonly ISharingLifetimeScope _parent;

		private readonly IDisposer _disposer = new Disposer();

		private readonly object _tag;

		internal static Guid SelfRegistrationId = Guid.NewGuid();

		private static readonly Action<ContainerBuilder> NoConfiguration = delegate
		{
		};

		public static readonly object RootTag = "root";

		public ISharingLifetimeScope ParentLifetimeScope => _parent;

		public ISharingLifetimeScope RootLifetimeScope => _root;

		public IDisposer Disposer => _disposer;

		public object Tag => _tag;

		public IComponentRegistry ComponentRegistry => _componentRegistry;

		public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;

		public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;

		public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

		private static object MakeAnonymousTag()
		{
			return new object();
		}

		private LifetimeScope()
		{
			_sharedInstances[SelfRegistrationId] = this;
		}

		protected LifetimeScope(IComponentRegistry componentRegistry, LifetimeScope parent, object tag)
			: this(componentRegistry, tag)
		{
			_parent = Enforce.ArgumentNotNull(parent, "parent");
			_root = _parent.RootLifetimeScope;
		}

		public LifetimeScope(IComponentRegistry componentRegistry, object tag)
			: this()
		{
			_componentRegistry = Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
			_root = this;
			_tag = Enforce.ArgumentNotNull(tag, "tag");
		}

		public LifetimeScope(IComponentRegistry componentRegistry)
			: this(componentRegistry, RootTag)
		{
		}

		public ILifetimeScope BeginLifetimeScope()
		{
			return BeginLifetimeScope(MakeAnonymousTag());
		}

		public ILifetimeScope BeginLifetimeScope(object tag)
		{
			CheckNotDisposed();
			CopyOnWriteRegistry componentRegistry = new CopyOnWriteRegistry(_componentRegistry, () => CreateScopeRestrictedRegistry(tag, NoConfiguration));
			LifetimeScope lifetimeScope = new LifetimeScope(componentRegistry, this, tag);
			RaiseBeginning(lifetimeScope);
			return lifetimeScope;
		}

		private void RaiseBeginning(ILifetimeScope scope)
		{
			this.ChildLifetimeScopeBeginning?.Invoke(this, new LifetimeScopeBeginningEventArgs(scope));
		}

		public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
		{
			return BeginLifetimeScope(MakeAnonymousTag(), configurationAction);
		}

		public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
		{
			if (configurationAction == null)
			{
				throw new ArgumentNullException("configurationAction");
			}
			CheckNotDisposed();
			ScopeRestrictedRegistry componentRegistry = CreateScopeRestrictedRegistry(tag, configurationAction);
			LifetimeScope lifetimeScope = new LifetimeScope(componentRegistry, this, tag);
			RaiseBeginning(lifetimeScope);
			return lifetimeScope;
		}

		private ScopeRestrictedRegistry CreateScopeRestrictedRegistry(object tag, Action<ContainerBuilder> configurationAction)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			foreach (IRegistrationSource item in ComponentRegistry.Sources.Where((IRegistrationSource src) => src.IsAdapterForIndividualComponents))
			{
				containerBuilder.RegisterSource(item);
			}
			IEnumerable<ExternalRegistrySource> enumerable = (from s in Traverse.Across(this, (ISharingLifetimeScope s) => s.ParentLifetimeScope)
				where s.ComponentRegistry.HasLocalComponents
				select new ExternalRegistrySource(s.ComponentRegistry)).Reverse();
			foreach (ExternalRegistrySource item2 in enumerable)
			{
				containerBuilder.RegisterSource(item2);
			}
			configurationAction(containerBuilder);
			ScopeRestrictedRegistry scopeRestrictedRegistry = new ScopeRestrictedRegistry(tag);
			containerBuilder.Update(scopeRestrictedRegistry);
			return scopeRestrictedRegistry;
		}

		public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			CheckNotDisposed();
			ResolveOperation resolveOperation = new ResolveOperation(this);
			this.ResolveOperationBeginning?.Invoke(this, new ResolveOperationBeginningEventArgs(resolveOperation));
			return resolveOperation.Execute(registration, parameters);
		}

		public object GetOrCreateAndShare(Guid id, Func<object> creator)
		{
			if (creator == null)
			{
				throw new ArgumentNullException("creator");
			}
			lock (_synchRoot)
			{
				if (!_sharedInstances.TryGetValue(id, out var value))
				{
					value = creator();
					_sharedInstances.Add(id, value);
				}
				return value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.CurrentScopeEnding?.Invoke(this, new LifetimeScopeEndingEventArgs(this));
				_disposer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void CheckNotDisposed()
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(LifetimeScopeResources.ScopeIsDisposed, (Exception)null);
			}
		}

		public object GetService(Type serviceType)
		{
			if ((object)serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			return this.ResolveOptional(serviceType);
		}
	}
}
