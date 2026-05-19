using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;
using Autofac.Util;

namespace Autofac
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public static class RegistrationExtensions
	{
		public static void RegisterComponent(this ContainerBuilder builder, IComponentRegistration registration)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				cr.Register(registration);
			});
		}

		public static void RegisterSource(this ContainerBuilder builder, IRegistrationSource registrationSource)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (registrationSource == null)
			{
				throw new ArgumentNullException("registrationSource");
			}
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				cr.AddRegistrationSource(registrationSource);
			});
		}

		public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterInstance<T>(this ContainerBuilder builder, T instance) where T : class
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			ProvidedInstanceActivator activator = new ProvidedInstanceActivator(instance);
			RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> rb = new RegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>(new TypedService(typeof(T)), new SimpleActivatorData(activator), new SingleRegistrationStyle());
			rb.SingleInstance();
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) || rb.RegistrationData.Sharing != InstanceSharing.Shared)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.InstanceRegistrationsAreSingleInstanceOnly, new object[1] { instance }));
				}
				activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb;
		}

		public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TImplementer>(this ContainerBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb = RegistrationBuilder.ForType<TImplementer>();
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb;
		}

		public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(this ContainerBuilder builder, Type implementationType)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if ((object)implementationType == null)
			{
				throw new ArgumentNullException("implementationType");
			}
			IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb = RegistrationBuilder.ForType(implementationType);
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb;
		}

		public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilder builder, Func<IComponentContext, T> @delegate)
		{
			if (@delegate == null)
			{
				throw new ArgumentNullException("delegate");
			}
			return builder.Register((IComponentContext c, IEnumerable<Parameter> p) => @delegate(c));
		}

		public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> Register<T>(this ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, T> @delegate)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (@delegate == null)
			{
				throw new ArgumentNullException("delegate");
			}
			IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> rb = RegistrationBuilder.ForDelegate(@delegate);
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb;
		}

		public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> RegisterGeneric(this ContainerBuilder builder, Type implementer)
		{
			return OpenGenericRegistrationExtensions.RegisterGeneric(builder, implementer);
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> PreserveExistingDefaults<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration) where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TSingleRegistrationStyle registrationStyle = registration.RegistrationStyle;
			registrationStyle.PreserveDefaults = true;
			return registration;
		}

		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> PreserveExistingDefaults<TLimit, TRegistrationStyle>(this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
		{
			return ScanningRegistrationExtensions.PreserveExistingDefaults(registration);
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyTypes(this ContainerBuilder builder, params Assembly[] assemblies)
		{
			return ScanningRegistrationExtensions.RegisterAssemblyTypes(builder, assemblies);
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterTypes(this ContainerBuilder builder, params Type[] types)
		{
			return ScanningRegistrationExtensions.RegisterTypes(builder, types);
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> Where<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, bool> predicate) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TScanningActivatorData activatorData = registration.ActivatorData;
			activatorData.Filters.Add(predicate);
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> As<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, IEnumerable<Service>> serviceMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (serviceMapping == null)
			{
				throw new ArgumentNullException("serviceMapping");
			}
			return ScanningRegistrationExtensions.As(registration, serviceMapping);
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> As<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, Service> serviceMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As((Type t) => new Service[1] { serviceMapping(t) });
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> As<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, Type> serviceMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As((Type t) => new TypedService(serviceMapping(t)));
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> As<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, IEnumerable<Type>> serviceMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As((Type t) => serviceMapping(t).Select((Func<Type, Service>)((Type s) => new TypedService(s))));
		}

		public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As((Type t) => t);
		}

		public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> AsSelf<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration) where TConcreteActivatorData : IConcreteActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As(registration.ActivatorData.Activator.LimitType);
		}

		public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As(registration.ActivatorData.ImplementationType);
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, IEnumerable<KeyValuePair<string, object>>> metadataMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TScanningActivatorData activatorData = registration.ActivatorData;
			activatorData.ConfigurationActions.Add(delegate(Type t, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb)
			{
				rb.WithMetadata(metadataMapping(t));
			});
			return registration;
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> WithMetadataFrom<TAttribute>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			Type typeFromHandle = typeof(TAttribute);
			IEnumerable<PropertyInfo> metadataProperties = from pi in typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where pi.CanRead
				select pi;
			return registration.WithMetadata(delegate(Type t)
			{
				TAttribute[] array = t.GetCustomAttributes(inherit: true).OfType<TAttribute>().ToArray();
				if (array.Length == 0)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MetadataAttributeNotFound, new object[2]
					{
						typeof(TAttribute),
						t
					}));
				}
				if (array.Length != 1)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MultipleMetadataAttributesSameType, new object[2]
					{
						typeof(TAttribute),
						t
					}));
				}
				TAttribute attr = array[0];
				return metadataProperties.Select((PropertyInfo p) => new KeyValuePair<string, object>(p.Name, p.GetValue(attr, null)));
			});
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> WithMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, string metadataKey, Func<Type, object> metadataValueMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.WithMetadata((Type t) => new KeyValuePair<string, object>[1]
			{
				new KeyValuePair<string, object>(metadataKey, metadataValueMapping(t))
			});
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> Named<TService>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration, Func<Type, string> serviceNameMapping)
		{
			return registration.Named(serviceNameMapping, typeof(TService));
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> Named<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, string> serviceNameMapping, Type serviceType) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (serviceNameMapping == null)
			{
				throw new ArgumentNullException("serviceNameMapping");
			}
			if ((object)serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			return registration.As((Type t) => new KeyedService(serviceNameMapping(t), serviceType));
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> Keyed<TService>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration, Func<Type, object> serviceKeyMapping)
		{
			return Keyed(registration, serviceKeyMapping, typeof(TService));
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> Keyed<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, object> serviceKeyMapping, Type serviceType) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (serviceKeyMapping == null)
			{
				throw new ArgumentNullException("serviceKeyMapping");
			}
			if ((object)serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			return registration.AssignableTo(serviceType).As((Type t) => new KeyedService(serviceKeyMapping(t), serviceType));
		}

		public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As((Type t) => GetImplementedInterfaces(t));
		}

		public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> AsImplementedInterfaces<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration) where TConcreteActivatorData : IConcreteActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.As(GetImplementedInterfaces(registration.ActivatorData.Activator.LimitType));
		}

		public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			Type implementationType = registration.ActivatorData.ImplementationType;
			return registration.As(GetImplementedInterfaces(implementationType));
		}

		private static Type[] GetImplementedInterfaces(Type type)
		{
			return (from i in type.GetInterfaces()
				where (object)i != typeof(IDisposable)
				select i).ToArray();
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, IConstructorFinder constructorFinder) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (constructorFinder == null)
			{
				throw new ArgumentNullException("constructorFinder");
			}
			TReflectionActivatorData activatorData = registration.ActivatorData;
			activatorData.ConstructorFinder = constructorFinder;
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> FindConstructorsWith<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Func<Type, ConstructorInfo[]> finder) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.FindConstructorsWith(new DefaultConstructorFinder(finder));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, params Type[] signature) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}
			TReflectionActivatorData activatorData = registration.ActivatorData;
			if ((object)activatorData.ImplementationType.GetConstructor(signature) == null)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string noMatchingConstructorExists = RegistrationExtensionsResources.NoMatchingConstructorExists;
				object[] array = new object[1];
				TReflectionActivatorData activatorData2 = registration.ActivatorData;
				array[0] = activatorData2.ImplementationType;
				throw new ArgumentException(string.Format(currentCulture, noMatchingConstructorExists, array));
			}
			return registration.UsingConstructor(new MatchingSignatureConstructorSelector(signature));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, IConstructorSelector constructorSelector) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (constructorSelector == null)
			{
				throw new ArgumentNullException("constructorSelector");
			}
			TReflectionActivatorData activatorData = registration.ActivatorData;
			activatorData.ConstructorSelector = constructorSelector;
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> UsingConstructor<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Expression<Func<TLimit>> constructorSelector) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (constructorSelector == null)
			{
				throw new ArgumentNullException("constructorSelector");
			}
			ConstructorInfo constructor = ReflectionExtensions.GetConstructor(constructorSelector);
			Type[] signature = (from p in constructor.GetParameters()
				select p.ParameterType).ToArray();
			return registration.UsingConstructor(signature);
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameter<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, string parameterName, object parameterValue) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(new NamedParameter(parameterName, parameterValue));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameter<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Parameter parameter) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (parameter == null)
			{
				throw new ArgumentNullException("parameter");
			}
			TReflectionActivatorData activatorData = registration.ActivatorData;
			activatorData.ConfiguredParameters.Add(parameter);
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameter<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Func<ParameterInfo, IComponentContext, bool> parameterSelector, Func<ParameterInfo, IComponentContext, object> valueProvider) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (parameterSelector == null)
			{
				throw new ArgumentNullException("parameterSelector");
			}
			if (valueProvider == null)
			{
				throw new ArgumentNullException("valueProvider");
			}
			return registration.WithParameter(new ResolvedParameter(parameterSelector, valueProvider));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithParameters<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, IEnumerable<Parameter> parameters) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			foreach (Parameter parameter in parameters)
			{
				registration.WithParameter(parameter);
			}
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithProperty<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, string propertyName, object propertyValue) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithProperty(new NamedPropertyParameter(propertyName, propertyValue));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithProperty<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Parameter property) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			TReflectionActivatorData activatorData = registration.ActivatorData;
			activatorData.ConfiguredProperties.Add(property);
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithProperties<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, IEnumerable<Parameter> properties) where TReflectionActivatorData : ReflectionActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			foreach (Parameter property in properties)
			{
				registration.WithProperty(property);
			}
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> Targeting<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, IComponentRegistration target) where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			TSingleRegistrationStyle registrationStyle = registration.RegistrationStyle;
			registrationStyle.Target = target.Target;
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> OnRegistered<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, Action<ComponentRegisteredEventArgs> handler) where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			TSingleRegistrationStyle registrationStyle = registration.RegistrationStyle;
			registrationStyle.RegisteredHandlers.Add(delegate(object s, ComponentRegisteredEventArgs e)
			{
				handler(e);
			});
			return registration;
		}

		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> OnRegistered<TLimit, TRegistrationStyle>(this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration, Action<ComponentRegisteredEventArgs> handler)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			registration.ActivatorData.ConfigurationActions.Add(delegate(Type t, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb)
			{
				rb.OnRegistered(handler);
			});
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType) where TScanningActivatorData : ScanningActivatorData
		{
			return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType);
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type type) where TScanningActivatorData : ScanningActivatorData
		{
			return ScanningRegistrationExtensions.AssignableTo(registration, type);
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> AssignableTo<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			return registration.AssignableTo(typeof(T));
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> Except<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			return registration.Where((Type t) => (object)t != typeof(T));
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> Except<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration, Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>> customizedRegistration)
		{
			IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registrationBuilder = registration.Except<T>();
			registrationBuilder.ActivatorData.PostScanningCallbacks.Add(delegate(IComponentRegistry cr)
			{
				IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder2 = RegistrationBuilder.ForType<T>();
				customizedRegistration(registrationBuilder2);
				RegistrationBuilder.RegisterSingleComponent(cr, registrationBuilder2);
			});
			return registrationBuilder;
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> InNamespaceOf<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			return registration.InNamespace(typeof(T).Namespace);
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> InNamespace<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, string ns) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (ns == null)
			{
				throw new ArgumentNullException("ns");
			}
			return registration.Where((Type t) => t.IsInNamespace(ns));
		}

		public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(this ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (adapter == null)
			{
				throw new ArgumentNullException("adapter");
			}
			return LightweightAdapterRegistrationExtensions.RegisterAdapter(builder, adapter);
		}

		public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(this ContainerBuilder builder, Func<IComponentContext, TFrom, TTo> adapter)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (adapter == null)
			{
				throw new ArgumentNullException("adapter");
			}
			return builder.RegisterAdapter((IComponentContext c, IEnumerable<Parameter> p, TFrom f) => adapter(c, f));
		}

		public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(this ContainerBuilder builder, Func<TFrom, TTo> adapter)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (adapter == null)
			{
				throw new ArgumentNullException("adapter");
			}
			return builder.RegisterAdapter((IComponentContext c, IEnumerable<Parameter> p, TFrom f) => adapter(f));
		}

		public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle> RegisterGenericDecorator(this ContainerBuilder builder, Type decoratorType, Type decoratedServiceType, object fromKey, object toKey = null)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if ((object)decoratorType == null)
			{
				throw new ArgumentNullException("decoratorType");
			}
			if ((object)decoratedServiceType == null)
			{
				throw new ArgumentNullException("decoratedServiceType");
			}
			return OpenGenericRegistrationExtensions.RegisterGenericDecorator(builder, decoratorType, decoratedServiceType, fromKey, toKey);
		}

		public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(this ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator, object fromKey, object toKey = null)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (decorator == null)
			{
				throw new ArgumentNullException("decorator");
			}
			return LightweightAdapterRegistrationExtensions.RegisterDecorator(builder, decorator, fromKey, toKey);
		}

		public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(this ContainerBuilder builder, Func<IComponentContext, TService, TService> decorator, object fromKey, object toKey = null)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (decorator == null)
			{
				throw new ArgumentNullException("decorator");
			}
			return LightweightAdapterRegistrationExtensions.RegisterDecorator(builder, (IComponentContext c, IEnumerable<Parameter> p, TService f) => decorator(c, f), fromKey, toKey);
		}

		public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(this ContainerBuilder builder, Func<TService, TService> decorator, object fromKey, object toKey = null)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (decorator == null)
			{
				throw new ArgumentNullException("decorator");
			}
			return LightweightAdapterRegistrationExtensions.RegisterDecorator(builder, (IComponentContext c, IEnumerable<Parameter> p, TService f) => decorator(f), fromKey, toKey);
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnRelease<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, Action<TLimit> releaseAction)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (releaseAction == null)
			{
				throw new ArgumentNullException("releaseAction");
			}
			return registration.ExternallyOwned().OnActivating(delegate(IActivatingEventArgs<TLimit> e)
			{
				ReleaseAction instance = new ReleaseAction(delegate
				{
					releaseAction(e.Instance);
				});
				e.Context.Resolve<ILifetimeScope>().Disposer.AddInstanceForDisposal(instance);
			});
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AutoActivate<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			registration.RegistrationData.AddService(new AutoActivateService());
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerRequest<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			object[] lifetimeScopeTag = new object[1] { MatchingScopeLifetimeTags.RequestLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
			return registration.InstancePerMatchingLifetimeScope(lifetimeScopeTag);
		}
	}
}
