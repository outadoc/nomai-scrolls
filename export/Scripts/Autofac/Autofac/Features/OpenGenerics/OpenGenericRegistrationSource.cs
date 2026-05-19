using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Features.OpenGenerics
{
	internal class OpenGenericRegistrationSource : IRegistrationSource
	{
		private readonly RegistrationData _registrationData;

		private readonly ReflectionActivatorData _activatorData;

		public bool IsAdapterForIndividualComponents => false;

		public OpenGenericRegistrationSource(RegistrationData registrationData, ReflectionActivatorData activatorData)
		{
			if (registrationData == null)
			{
				throw new ArgumentNullException("registrationData");
			}
			if (activatorData == null)
			{
				throw new ArgumentNullException("activatorData");
			}
			OpenGenericServiceBinder.EnforceBindable(activatorData.ImplementationType, registrationData.Services);
			_registrationData = registrationData;
			_activatorData = activatorData;
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (registrationAccessor == null)
			{
				throw new ArgumentNullException("registrationAccessor");
			}
			if (OpenGenericServiceBinder.TryBindServiceType(service, _registrationData.Services, _activatorData.ImplementationType, out var constructedImplementationType, out var services))
			{
				yield return RegistrationBuilder.CreateRegistration(Guid.NewGuid(), _registrationData, new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, _activatorData.ConfiguredParameters, _activatorData.ConfiguredProperties), services);
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, OpenGenericRegistrationSourceResources.OpenGenericRegistrationSourceDescription, new object[2]
			{
				_activatorData.ImplementationType.FullName,
				string.Join(", ", _registrationData.Services.Select((Service s) => s.Description).ToArray())
			});
		}
	}
}
