using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.ResolveAnything
{
	public class AnyConcreteTypeNotAlreadyRegisteredSource : IRegistrationSource
	{
		private readonly Func<Type, bool> _predicate;

		public bool IsAdapterForIndividualComponents => false;

		public Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> RegistrationConfiguration { get; set; }

		public AnyConcreteTypeNotAlreadyRegisteredSource()
			: this((Type t) => true)
		{
		}

		public AnyConcreteTypeNotAlreadyRegisteredSource(Func<Type, bool> predicate)
		{
			_predicate = Enforce.ArgumentNotNull(predicate, "predicate");
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			TypedService typedService = service as TypedService;
			if (typedService == null || !typedService.ServiceType.IsClass || typedService.ServiceType.IsSubclassOf(typeof(Delegate)) || typedService.ServiceType.IsAbstract || !_predicate(typedService.ServiceType) || registrationAccessor(service).Any())
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrationBuilder = RegistrationBuilder.ForType(typedService.ServiceType);
			if (RegistrationConfiguration != null)
			{
				RegistrationConfiguration(registrationBuilder);
			}
			return new IComponentRegistration[1] { registrationBuilder.CreateRegistration() };
		}

		public override string ToString()
		{
			return AnyConcreteTypeNotAlreadyRegisteredSourceResources.AnyConcreteTypeNotAlreadyRegisteredSourceDescription;
		}
	}
}
