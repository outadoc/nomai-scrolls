using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

namespace Autofac.Builder
{
	public static class RegistrationBuilder
	{
		public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> ForDelegate<T>(Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
		{
			return new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(new TypedService(typeof(T)), new SimpleActivatorData(new DelegateActivator(typeof(T), (IComponentContext c, IEnumerable<Parameter> p) => @delegate(c, p))), new SingleRegistrationStyle());
		}

		public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> ForDelegate(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> @delegate)
		{
			return new RegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>(new TypedService(limitType), new SimpleActivatorData(new DelegateActivator(limitType, @delegate)), new SingleRegistrationStyle());
		}

		public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType<TImplementer>()
		{
			return new RegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>(new TypedService(typeof(TImplementer)), new ConcreteReflectionActivatorData(typeof(TImplementer)), new SingleRegistrationStyle());
		}

		public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> ForType(Type implementationType)
		{
			return new RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(new TypedService(implementationType), new ConcreteReflectionActivatorData(implementationType), new SingleRegistrationStyle());
		}

		public static IComponentRegistration CreateRegistration<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder) where TActivatorData : IConcreteActivatorData where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			TSingleRegistrationStyle registrationStyle = builder.RegistrationStyle;
			Guid id = registrationStyle.Id;
			RegistrationData registrationData = builder.RegistrationData;
			IInstanceActivator activator = builder.ActivatorData.Activator;
			IEnumerable<Service> services = builder.RegistrationData.Services;
			TSingleRegistrationStyle registrationStyle2 = builder.RegistrationStyle;
			return CreateRegistration(id, registrationData, activator, services, registrationStyle2.Target);
		}

		public static IComponentRegistration CreateRegistration(Guid id, RegistrationData data, IInstanceActivator activator, IEnumerable<Service> services)
		{
			return CreateRegistration(id, data, activator, services, null);
		}

		public static IComponentRegistration CreateRegistration(Guid id, RegistrationData data, IInstanceActivator activator, IEnumerable<Service> services, IComponentRegistration target)
		{
			if (activator == null)
			{
				throw new ArgumentNullException("activator");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			Type limitType = activator.LimitType;
			if ((object)limitType != typeof(object))
			{
				foreach (IServiceWithType item in services.OfType<IServiceWithType>())
				{
					if (!item.ServiceType.IsAssignableFrom(limitType))
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationBuilderResources.ComponentDoesNotSupportService, new object[2] { limitType, item }));
					}
				}
			}
			IComponentRegistration componentRegistration = ((target != null) ? new ComponentRegistration(id, activator, data.Lifetime, data.Sharing, data.Ownership, services, data.Metadata, target) : new ComponentRegistration(id, activator, data.Lifetime, data.Sharing, data.Ownership, services, data.Metadata));
			foreach (EventHandler<PreparingEventArgs> preparingHandler in data.PreparingHandlers)
			{
				componentRegistration.Preparing += preparingHandler;
			}
			foreach (EventHandler<ActivatingEventArgs<object>> activatingHandler in data.ActivatingHandlers)
			{
				componentRegistration.Activating += activatingHandler;
			}
			foreach (EventHandler<ActivatedEventArgs<object>> activatedHandler in data.ActivatedHandlers)
			{
				componentRegistration.Activated += activatedHandler;
			}
			return componentRegistration;
		}

		public static void RegisterSingleComponent<TLimit, TActivatorData, TSingleRegistrationStyle>(IComponentRegistry cr, IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> builder) where TActivatorData : IConcreteActivatorData where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (cr == null)
			{
				throw new ArgumentNullException("cr");
			}
			IComponentRegistration componentRegistration = builder.CreateRegistration();
			TSingleRegistrationStyle registrationStyle = builder.RegistrationStyle;
			cr.Register(componentRegistration, registrationStyle.PreserveDefaults);
			ComponentRegisteredEventArgs e = new ComponentRegisteredEventArgs(cr, componentRegistration);
			TSingleRegistrationStyle registrationStyle2 = builder.RegistrationStyle;
			foreach (EventHandler<ComponentRegisteredEventArgs> registeredHandler in registrationStyle2.RegisteredHandlers)
			{
				registeredHandler(cr, e);
			}
		}
	}
	internal class RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> : IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>, IHideObjectMembers
	{
		private readonly TActivatorData _activatorData;

		private readonly TRegistrationStyle _registrationStyle;

		private readonly RegistrationData _registrationData;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public TActivatorData ActivatorData => _activatorData;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public TRegistrationStyle RegistrationStyle => _registrationStyle;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RegistrationData RegistrationData => _registrationData;

		public RegistrationBuilder(Service defaultService, TActivatorData activatorData, TRegistrationStyle style)
		{
			if (defaultService == null)
			{
				throw new ArgumentNullException("defaultService");
			}
			if (activatorData == null)
			{
				throw new ArgumentNullException("activatorData");
			}
			if (style == null)
			{
				throw new ArgumentNullException("style");
			}
			_activatorData = activatorData;
			_registrationStyle = style;
			_registrationData = new RegistrationData(defaultService);
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned()
		{
			RegistrationData.Ownership = InstanceOwnership.ExternallyOwned;
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope()
		{
			RegistrationData.Ownership = InstanceOwnership.OwnedByLifetimeScope;
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerDependency()
		{
			RegistrationData.Sharing = InstanceSharing.None;
			RegistrationData.Lifetime = new CurrentScopeLifetime();
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance()
		{
			RegistrationData.Sharing = InstanceSharing.Shared;
			RegistrationData.Lifetime = new RootScopeLifetime();
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope()
		{
			RegistrationData.Sharing = InstanceSharing.Shared;
			RegistrationData.Lifetime = new CurrentScopeLifetime();
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(params object[] lifetimeScopeTag)
		{
			if (lifetimeScopeTag == null)
			{
				throw new ArgumentNullException("lifetimeScopeTag");
			}
			RegistrationData.Sharing = InstanceSharing.Shared;
			RegistrationData.Lifetime = new MatchingScopeLifetime(lifetimeScopeTag);
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>()
		{
			return InstancePerOwned(typeof(TService));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(Type serviceType)
		{
			return InstancePerMatchingLifetimeScope(new TypedService(serviceType));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(object serviceKey)
		{
			return InstancePerOwned(serviceKey, typeof(TService));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(object serviceKey, Type serviceType)
		{
			return InstancePerMatchingLifetimeScope(new KeyedService(serviceKey, serviceType));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>()
		{
			return As(new TypedService(typeof(TService)));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>()
		{
			return As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>()
		{
			return As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)), new TypedService(typeof(TService3)));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services)
		{
			return As(services.Select((Type t) => (t.FullName == null) ? new TypedService(t.GetGenericTypeDefinition()) : new TypedService(t)).Cast<Service>().ToArray());
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services)
		{
			if (services == null)
			{
				throw new ArgumentNullException("services");
			}
			RegistrationData.AddServices(services);
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName, Type serviceType)
		{
			if (serviceName == null)
			{
				throw new ArgumentNullException("serviceName");
			}
			if ((object)serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			return As(new KeyedService(serviceName, serviceType));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named<TService>(string serviceName)
		{
			return Named(serviceName, typeof(TService));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed(object serviceKey, Type serviceType)
		{
			if (serviceKey == null)
			{
				throw new ArgumentNullException("serviceKey");
			}
			if ((object)serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			return As(new KeyedService(serviceKey, serviceType));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed<TService>(object serviceKey)
		{
			return Keyed(serviceKey, typeof(TService));
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			RegistrationData.PreparingHandlers.Add(delegate(object s, PreparingEventArgs e)
			{
				handler(e);
			});
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<IActivatingEventArgs<TLimit>> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			RegistrationData.ActivatingHandlers.Add(delegate(object s, ActivatingEventArgs<object> e)
			{
				ActivatingEventArgs<TLimit> e2 = new ActivatingEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance);
				handler(e2);
				e.Instance = e2.Instance;
			});
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<IActivatedEventArgs<TLimit>> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			RegistrationData.ActivatedHandlers.Add(delegate(object s, ActivatedEventArgs<object> e)
			{
				handler(new ActivatedEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance));
			});
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(PropertyWiringOptions wiringFlags)
		{
			bool flag = 0 != (wiringFlags & PropertyWiringOptions.AllowCircularDependencies);
			bool preserveSetValues = 0 != (wiringFlags & PropertyWiringOptions.PreserveSetValues);
			if (flag)
			{
				RegistrationData.ActivatedHandlers.Add(delegate(object s, ActivatedEventArgs<object> e)
				{
					AutowiringPropertyInjector.InjectProperties(e.Context, e.Instance, !preserveSetValues);
				});
			}
			else
			{
				RegistrationData.ActivatingHandlers.Add(delegate(object s, ActivatingEventArgs<object> e)
				{
					AutowiringPropertyInjector.InjectProperties(e.Context, e.Instance, !preserveSetValues);
				});
			}
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(string key, object value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			RegistrationData.Metadata.Add(key, value);
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(IEnumerable<KeyValuePair<string, object>> properties)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			foreach (KeyValuePair<string, object> property in properties)
			{
				WithMetadata(property.Key, property.Value);
			}
			return this;
		}

		public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata<TMetadata>(Action<MetadataConfiguration<TMetadata>> configurationAction)
		{
			if (configurationAction == null)
			{
				throw new ArgumentNullException("configurationAction");
			}
			MetadataConfiguration<TMetadata> metadataConfiguration = new MetadataConfiguration<TMetadata>();
			configurationAction(metadataConfiguration);
			return WithMetadata(metadataConfiguration.Properties);
		}

		Type IHideObjectMembers.GetType()
		{
			return GetType();
		}
	}
}
