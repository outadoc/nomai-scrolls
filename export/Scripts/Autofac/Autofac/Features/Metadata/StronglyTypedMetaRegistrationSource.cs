using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Metadata
{
	internal class StronglyTypedMetaRegistrationSource : IRegistrationSource
	{
		private delegate IComponentRegistration RegistrationCreator(Service service, IComponentRegistration valueRegistration);

		private static readonly MethodInfo CreateMetaRegistrationMethod = typeof(StronglyTypedMetaRegistrationSource).GetMethod("CreateMetaRegistration", BindingFlags.Static | BindingFlags.NonPublic);

		public bool IsAdapterForIndividualComponents => true;

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			if (!(service is IServiceWithType serviceWithType) || !serviceWithType.ServiceType.IsGenericTypeDefinedBy(typeof(Meta<, >)))
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
			RegistrationCreator registrationCreator = (RegistrationCreator)Delegate.CreateDelegate(typeof(RegistrationCreator), CreateMetaRegistrationMethod.MakeGenericMethod(type, type2));
			return from v in registrationAccessor(arg)
				select registrationCreator(service, v);
		}

		public override string ToString()
		{
			return MetaRegistrationSourceResources.StronglyTypedMetaRegistrationSourceDescription;
		}

		private static IComponentRegistration CreateMetaRegistration<T, TMetadata>(Service providedService, IComponentRegistration valueRegistration)
		{
			TMetadata metadata = MetadataViewProvider.GetMetadataViewProvider<TMetadata>()(valueRegistration.Target.Metadata);
			IRegistrationBuilder<Meta<T, TMetadata>, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate((IComponentContext c, IEnumerable<Parameter> p) => new Meta<T, TMetadata>((T)c.ResolveComponent(valueRegistration, p), metadata)).As(providedService).Targeting(valueRegistration);
			return builder.CreateRegistration();
		}
	}
}
