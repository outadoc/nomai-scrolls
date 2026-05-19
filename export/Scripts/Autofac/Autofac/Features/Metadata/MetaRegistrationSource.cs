using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Metadata
{
	internal class MetaRegistrationSource : IRegistrationSource
	{
		private static readonly MethodInfo CreateMetaRegistrationMethod = typeof(MetaRegistrationSource).GetMethod("CreateMetaRegistration", BindingFlags.Static | BindingFlags.NonPublic);

		public bool IsAdapterForIndividualComponents => true;

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			if (!(service is IServiceWithType serviceWithType) || !serviceWithType.ServiceType.IsGenericTypeDefinedBy(typeof(Meta<>)))
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Type type = serviceWithType.ServiceType.GetGenericArguments()[0];
			Service arg = serviceWithType.ChangeType(type);
			MethodInfo registrationCreator = CreateMetaRegistrationMethod.MakeGenericMethod(type);
			return (from v in registrationAccessor(arg)
				select registrationCreator.Invoke(null, new object[2] { service, v })).Cast<IComponentRegistration>();
		}

		public override string ToString()
		{
			return MetaRegistrationSourceResources.MetaRegistrationSourceDescription;
		}

		private static IComponentRegistration CreateMetaRegistration<T>(Service providedService, IComponentRegistration valueRegistration)
		{
			IRegistrationBuilder<Meta<T>, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate((IComponentContext c, IEnumerable<Parameter> p) => new Meta<T>((T)c.ResolveComponent(valueRegistration, p), valueRegistration.Target.Metadata)).As(providedService).Targeting(valueRegistration);
			return builder.CreateRegistration();
		}
	}
}
