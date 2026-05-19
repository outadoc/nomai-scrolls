using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.Indexed;
using Autofac.Features.LazyDependencies;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Autofac.Util;

namespace Autofac
{
	public class ContainerBuilder
	{
		private readonly IList<Action<IComponentRegistry>> _configurationCallbacks = new List<Action<IComponentRegistry>>();

		private bool _wasBuilt;

		public virtual void RegisterCallback(Action<IComponentRegistry> configurationCallback)
		{
			_configurationCallbacks.Add(Enforce.ArgumentNotNull(configurationCallback, "configurationCallback"));
		}

		public IContainer Build(ContainerBuildOptions options = ContainerBuildOptions.None)
		{
			Container container = new Container();
			Build(container.ComponentRegistry, (options & ContainerBuildOptions.ExcludeDefaultModules) != 0);
			if ((options & ContainerBuildOptions.IgnoreStartableComponents) == 0)
			{
				StartStartableComponents(container);
			}
			return container;
		}

		private static void StartStartableComponents(IComponentContext componentContext)
		{
			object meta = null;
			foreach (IComponentRegistration item in from r in componentContext.ComponentRegistry.RegistrationsFor(new TypedService(typeof(IStartable)))
				where !r.Metadata.TryGetValue("__AutoActivated", out meta)
				select r)
			{
				try
				{
					IStartable startable = (IStartable)componentContext.ResolveComponent(item, Enumerable.Empty<Parameter>());
					startable.Start();
				}
				finally
				{
					item.Metadata["__AutoActivated"] = true;
				}
			}
			foreach (IComponentRegistration item2 in from r in componentContext.ComponentRegistry.RegistrationsFor(new AutoActivateService())
				where !r.Metadata.TryGetValue("__AutoActivated", out meta)
				select r)
			{
				try
				{
					componentContext.ResolveComponent(item2, Enumerable.Empty<Parameter>());
				}
				catch (DependencyResolutionException innerException)
				{
					throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ContainerBuilderResources.ErrorAutoActivating, new object[1] { item2 }), innerException);
				}
				finally
				{
					item2.Metadata["__AutoActivated"] = true;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "You can't update any arbitrary context, only containers.")]
		public void Update(IContainer container)
		{
			Update(container, ContainerBuildOptions.None);
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "You can't update any arbitrary context, only containers.")]
		public void Update(IContainer container, ContainerBuildOptions options)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}
			Update(container.ComponentRegistry);
			if ((options & ContainerBuildOptions.IgnoreStartableComponents) == 0)
			{
				StartStartableComponents(container);
			}
		}

		public void Update(IComponentRegistry componentRegistry)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			Build(componentRegistry, excludeDefaultModules: true);
		}

		private void Build(IComponentRegistry componentRegistry, bool excludeDefaultModules)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			if (_wasBuilt)
			{
				throw new InvalidOperationException(ContainerBuilderResources.BuildCanOnlyBeCalledOnce);
			}
			_wasBuilt = true;
			if (!excludeDefaultModules)
			{
				RegisterDefaultAdapters(componentRegistry);
			}
			foreach (Action<IComponentRegistry> configurationCallback in _configurationCallbacks)
			{
				configurationCallback(componentRegistry);
			}
		}

		private void RegisterDefaultAdapters(IComponentRegistry componentRegistry)
		{
			this.RegisterGeneric(typeof(KeyedServiceIndex<, >)).As(typeof(IIndex<, >)).InstancePerLifetimeScope();
			componentRegistry.AddRegistrationSource(new CollectionRegistrationSource());
			componentRegistry.AddRegistrationSource(new OwnedInstanceRegistrationSource());
			componentRegistry.AddRegistrationSource(new MetaRegistrationSource());
			componentRegistry.AddRegistrationSource(new LazyRegistrationSource());
			componentRegistry.AddRegistrationSource(new LazyWithMetadataRegistrationSource());
			componentRegistry.AddRegistrationSource(new StronglyTypedMetaRegistrationSource());
			componentRegistry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
		}
	}
}
