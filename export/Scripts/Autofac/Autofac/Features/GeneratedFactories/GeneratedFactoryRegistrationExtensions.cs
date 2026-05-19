using System;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.GeneratedFactories
{
	internal static class GeneratedFactoryRegistrationExtensions
	{
		internal static IRegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle> RegisterGeneratedFactory<TLimit>(ContainerBuilder builder, Type delegateType, Service service)
		{
			GeneratedFactoryActivatorData activatorData = new GeneratedFactoryActivatorData(delegateType, service);
			RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle> rb = new RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>(new TypedService(delegateType), activatorData, new SingleRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb.InstancePerLifetimeScope();
		}
	}
}
