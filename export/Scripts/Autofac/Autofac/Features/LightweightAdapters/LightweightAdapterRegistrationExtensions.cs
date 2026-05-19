using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
	internal static class LightweightAdapterRegistrationExtensions
	{
		public static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter)
		{
			return RegisterAdapter(builder, adapter, new TypedService(typeof(TFrom)), new TypedService(typeof(TTo)));
		}

		public static IRegistrationBuilder<TService, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterDecorator<TService>(ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, TService, TService> decorator, object fromKey, object toKey)
		{
			return RegisterAdapter(builder, decorator, ServiceWithKey<TService>(fromKey), ServiceWithKey<TService>(toKey));
		}

		private static Service ServiceWithKey<TService>(object key)
		{
			if (key == null)
			{
				return new TypedService(typeof(TService));
			}
			return new KeyedService(key, typeof(TService));
		}

		private static IRegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterAdapter<TFrom, TTo>(ContainerBuilder builder, Func<IComponentContext, IEnumerable<Parameter>, TFrom, TTo> adapter, Service fromService, Service toService)
		{
			RegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle> rb = new RegistrationBuilder<TTo, LightweightAdapterActivatorData, DynamicRegistrationStyle>(toService, new LightweightAdapterActivatorData(fromService, (IComponentContext c, IEnumerable<Parameter> p, object f) => adapter(c, p, (TFrom)f)), new DynamicRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				cr.AddRegistrationSource(new LightweightAdapterRegistrationSource(rb.RegistrationData, rb.ActivatorData));
			});
			return rb;
		}
	}
}
