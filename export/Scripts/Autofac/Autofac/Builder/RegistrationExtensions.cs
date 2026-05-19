using System;
using Autofac.Core;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Util;

namespace Autofac.Builder
{
	public static class RegistrationExtensions
	{
		public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType)
		{
			if ((object)delegateType == null)
			{
				throw new ArgumentNullException("delegateType");
			}
			Enforce.ArgumentTypeIsFunction(delegateType);
			Type serviceType = delegateType.FunctionReturnType();
			return builder.RegisterGeneratedFactory(delegateType, new TypedService(serviceType));
		}

		public static IRegistrationBuilder<Delegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory(this ContainerBuilder builder, Type delegateType, Service service)
		{
			return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<Delegate>(builder, delegateType, service);
		}

		public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder, Service service) where TDelegate : class
		{
			return GeneratedFactoryRegistrationExtensions.RegisterGeneratedFactory<TDelegate>(builder, typeof(TDelegate), service);
		}

		public static IRegistrationBuilder<TDelegate, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder) where TDelegate : class
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			Enforce.ArgumentTypeIsFunction(typeof(TDelegate));
			Type serviceType = typeof(TDelegate).FunctionReturnType();
			return builder.RegisterGeneratedFactory<TDelegate>(new TypedService(serviceType));
		}

		public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> NamedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration) where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TGeneratedFactoryActivatorData activatorData = registration.ActivatorData;
			activatorData.ParameterMapping = ParameterMapping.ByName;
			return registration;
		}

		public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> PositionalParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration) where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TGeneratedFactoryActivatorData activatorData = registration.ActivatorData;
			activatorData.ParameterMapping = ParameterMapping.ByPosition;
			return registration;
		}

		public static IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> TypedParameterMapping<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TDelegate, TGeneratedFactoryActivatorData, TSingleRegistrationStyle> registration) where TGeneratedFactoryActivatorData : GeneratedFactoryActivatorData where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			TGeneratedFactoryActivatorData activatorData = registration.ActivatorData;
			activatorData.ParameterMapping = ParameterMapping.ByType;
			return registration;
		}

		public static IRegistrationBuilder<object[], SimpleActivatorData, SingleRegistrationStyle> RegisterCollection(this ContainerBuilder builder, string collectionName, Type elementType)
		{
			return CollectionRegistrationExtensions.RegisterCollection<object>(builder, collectionName, elementType);
		}

		public static IRegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle> RegisterCollection<T>(this ContainerBuilder builder, string collectionName)
		{
			return CollectionRegistrationExtensions.RegisterCollection<T>(builder, collectionName, typeof(T));
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, string collectionName) where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			return CollectionRegistrationExtensions.MemberOf(registration, collectionName);
		}
	}
}
