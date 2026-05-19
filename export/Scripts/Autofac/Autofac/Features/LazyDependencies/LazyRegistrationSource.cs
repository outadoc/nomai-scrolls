using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.LazyDependencies
{
	internal class LazyRegistrationSource : IRegistrationSource
	{
		private static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyRegistrationSource).GetMethod("CreateLazyRegistration", BindingFlags.Static | BindingFlags.NonPublic);

		public bool IsAdapterForIndividualComponents => true;

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			if (!(service is IServiceWithType serviceWithType) || !serviceWithType.ServiceType.IsGenericTypeDefinedBy(typeof(Lazy<>)))
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Type type = serviceWithType.ServiceType.GetGenericArguments()[0];
			Service arg = serviceWithType.ChangeType(type);
			MethodInfo registrationCreator = CreateLazyRegistrationMethod.MakeGenericMethod(type);
			return (from v in registrationAccessor(arg)
				select registrationCreator.Invoke(null, new object[2] { service, v })).Cast<IComponentRegistration>();
		}

		public override string ToString()
		{
			return LazyRegistrationSourceResources.LazyRegistrationSourceDescription;
		}

		private static IComponentRegistration CreateLazyRegistration<T>(Service providedService, IComponentRegistration valueRegistration)
		{
			IRegistrationBuilder<Lazy<T>, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate(delegate(IComponentContext c, IEnumerable<Parameter> p)
			{
				IComponentContext context = c.Resolve<IComponentContext>();
				return new Lazy<T>(() => (T)context.ResolveComponent(valueRegistration, p));
			}).As(providedService).Targeting(valueRegistration);
			return builder.CreateRegistration();
		}
	}
}
