using System;
using System.Globalization;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
	internal static class OpenGenericRegistrationExtensions
	{
		public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> RegisterGeneric(ContainerBuilder builder, Type implementor)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if ((object)implementor == null)
			{
				throw new ArgumentNullException("implementor");
			}
			if (!implementor.IsGenericTypeDefinition)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericRegistrationExtensionsResources.ImplementorMustBeOpenGenericType, new object[1] { implementor }));
			}
			RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(new TypedService(implementor), new ReflectionActivatorData(implementor), new DynamicRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				cr.AddRegistrationSource(new OpenGenericRegistrationSource(rb.RegistrationData, rb.ActivatorData));
			});
			return rb;
		}

		public static IRegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle> RegisterGenericDecorator(ContainerBuilder builder, Type decoratorType, Type decoratedServiceType, object fromKey, object toKey)
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
			RegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle> rb = new RegistrationBuilder<object, OpenGenericDecoratorActivatorData, DynamicRegistrationStyle>((Service)GetServiceWithKey(decoratedServiceType, toKey), new OpenGenericDecoratorActivatorData(decoratorType, GetServiceWithKey(decoratedServiceType, fromKey)), new DynamicRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				cr.AddRegistrationSource(new OpenGenericDecoratorRegistrationSource(rb.RegistrationData, rb.ActivatorData));
			});
			return rb;
		}

		private static IServiceWithType GetServiceWithKey(Type serviceType, object key)
		{
			if (key == null)
			{
				return new TypedService(serviceType);
			}
			return new KeyedService(key, serviceType);
		}
	}
}
