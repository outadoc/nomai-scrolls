using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Util;

namespace Autofac.Features.LazyDependencies
{
	internal class LazyWithMetadataRegistrationSource : IRegistrationSource
	{
		private delegate IComponentRegistration RegistrationCreator(Service service, IComponentRegistration valueRegistration);

		private static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetMethod("CreateLazyRegistration", BindingFlags.Static | BindingFlags.NonPublic);

		public bool IsAdapterForIndividualComponents => true;

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			IServiceWithType serviceWithType = service as IServiceWithType;
			Type lazyType = GetLazyType(serviceWithType);
			if (serviceWithType == null || (object)lazyType == null || !serviceWithType.ServiceType.IsGenericTypeDefinedBy(lazyType))
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Type type = serviceWithType.ServiceType.GetGenericArguments()[0];
			Type type2 = serviceWithType.ServiceType.GetGenericArguments()[1];
			if (!type2.IsClass)
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Service arg = serviceWithType.ChangeType(type);
			RegistrationCreator registrationCreator = (RegistrationCreator)Delegate.CreateDelegate(typeof(RegistrationCreator), CreateLazyRegistrationMethod.MakeGenericMethod(type, type2));
			return from v in registrationAccessor(arg)
				select registrationCreator(service, v);
		}

		public override string ToString()
		{
			return LazyWithMetadataRegistrationSourceResources.LazyWithMetadataRegistrationSourceDescription;
		}

		private static IComponentRegistration CreateLazyRegistration<T, TMeta>(Service providedService, IComponentRegistration valueRegistration)
		{
			Func<IDictionary<string, object>, TMeta> metadataViewProvider = MetadataViewProvider.GetMetadataViewProvider<TMeta>();
			TMeta metadata = metadataViewProvider(valueRegistration.Target.Metadata);
			IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate(delegate(IComponentContext c, IEnumerable<Parameter> p)
			{
				IComponentContext context = c.Resolve<IComponentContext>();
				Type serviceType = ((IServiceWithType)providedService).ServiceType;
				Func<T> func = () => (T)context.ResolveComponent(valueRegistration, p);
				return Activator.CreateInstance(serviceType, func, metadata);
			}).As(providedService).Targeting(valueRegistration);
			return builder.CreateRegistration();
		}

		private static Type GetLazyType(IServiceWithType serviceWithType)
		{
			if (serviceWithType == null || !serviceWithType.ServiceType.IsGenericType || !(serviceWithType.ServiceType.GetGenericTypeDefinition().FullName == "System.Lazy`2"))
			{
				return null;
			}
			return serviceWithType.ServiceType.GetGenericTypeDefinition();
		}
	}
}
