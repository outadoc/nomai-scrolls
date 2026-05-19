using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Scanning
{
	internal static class ScanningRegistrationExtensions
	{
		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (assemblies == null)
			{
				throw new ArgumentNullException("assemblies");
			}
			RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(new TypedService(typeof(object)), new ScanningActivatorData(), new DynamicRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				ScanAssemblies(assemblies, cr, rb);
			});
			return rb;
		}

		public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterTypes(ContainerBuilder builder, params Type[] types)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(new TypedService(typeof(object)), new ScanningActivatorData(), new DynamicRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				ScanTypes(types, cr, rb);
			});
			return rb;
		}

		private static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
		{
			ScanTypes(assemblies.SelectMany((Assembly a) => a.GetLoadableTypes()), cr, rb);
		}

		private static void ScanTypes(IEnumerable<Type> types, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
		{
			rb.ActivatorData.Filters.Add((Type t) => rb.RegistrationData.Services.OfType<IServiceWithType>().All((IServiceWithType swt) => swt.ServiceType.IsAssignableFrom(t)));
			foreach (Type item in types.Where((Type t) => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && !t.IsDelegate() && rb.ActivatorData.Filters.All((Func<Type, bool> p) => p(t))))
			{
				IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder = RegistrationBuilder.ForType(item).FindConstructorsWith(rb.ActivatorData.ConstructorFinder).UsingConstructor(rb.ActivatorData.ConstructorSelector)
					.WithParameters(rb.ActivatorData.ConfiguredParameters)
					.WithProperties(rb.ActivatorData.ConfiguredProperties);
				registrationBuilder.RegistrationData.CopyFrom(rb.RegistrationData, includeDefaultService: false);
				foreach (Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> configurationAction in rb.ActivatorData.ConfigurationActions)
				{
					configurationAction(item, registrationBuilder);
				}
				if (registrationBuilder.RegistrationData.Services.Any())
				{
					RegistrationBuilder.RegisterSingleComponent(cr, registrationBuilder);
				}
			}
			foreach (Action<IComponentRegistry> postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
			{
				postScanningCallback(cr);
			}
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType) where TScanningActivatorData : ScanningActivatorData
		{
			if ((object)openGenericServiceType == null)
			{
				throw new ArgumentNullException("openGenericServiceType");
			}
			return registration.Where((Type candidateType) => candidateType.IsClosedTypeOf(openGenericServiceType)).As((Type candidateType) => candidateType.GetTypesThatClose(openGenericServiceType).Select((Func<Type, Service>)((Type t) => new TypedService(t))));
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type type) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TScanningActivatorData activatorData = registration.ActivatorData;
			activatorData.Filters.Add(type.IsAssignableFrom);
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> As<TLimit, TScanningActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, IEnumerable<Service>> serviceMapping) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (serviceMapping == null)
			{
				throw new ArgumentNullException("serviceMapping");
			}
			TScanningActivatorData activatorData = registration.ActivatorData;
			activatorData.ConfigurationActions.Add(delegate(Type t, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb)
			{
				IEnumerable<Service> source = serviceMapping(t);
				Type impl = rb.ActivatorData.ImplementationType;
				IEnumerable<Service> source2 = source.Where(delegate(Service s)
				{
					IServiceWithType serviceWithType = s as IServiceWithType;
					return (serviceWithType == null && s != null) || serviceWithType.ServiceType.IsAssignableFrom(impl);
				});
				rb.As(source2.ToArray());
			});
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> PreserveExistingDefaults<TLimit, TScanningActivatorData, TRegistrationStyle>(IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration) where TScanningActivatorData : ScanningActivatorData
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TScanningActivatorData activatorData = registration.ActivatorData;
			activatorData.ConfigurationActions.Add(delegate(Type t, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> r)
			{
				r.PreserveExistingDefaults();
			});
			return registration;
		}
	}
}
