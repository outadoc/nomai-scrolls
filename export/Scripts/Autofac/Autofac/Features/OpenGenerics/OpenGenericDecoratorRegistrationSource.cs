using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Features.OpenGenerics
{
	internal class OpenGenericDecoratorRegistrationSource : IRegistrationSource
	{
		private readonly RegistrationData _registrationData;

		private readonly OpenGenericDecoratorActivatorData _activatorData;

		public bool IsAdapterForIndividualComponents => true;

		public OpenGenericDecoratorRegistrationSource(RegistrationData registrationData, OpenGenericDecoratorActivatorData activatorData)
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
			if (registrationData.Services.Contains((Service)activatorData.FromService))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorRegistrationSourceResources.FromAndToMustDiffer, new object[1] { activatorData.FromService }));
			}
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
				IServiceWithType swt = (IServiceWithType)service;
				Service arg = _activatorData.FromService.ChangeType(swt.ServiceType);
				return from cr in registrationAccessor(arg)
					select RegistrationBuilder.CreateRegistration(Guid.NewGuid(), _registrationData, new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, AddDecoratedComponentParameter(swt.ServiceType, cr, _activatorData.ConfiguredParameters), _activatorData.ConfiguredProperties), services);
			}
			return Enumerable.Empty<IComponentRegistration>();
		}

		private static IEnumerable<Parameter> AddDecoratedComponentParameter(Type decoratedParameterType, IComponentRegistration decoratedComponent, IEnumerable<Parameter> configuredParameters)
		{
			ResolvedParameter resolvedParameter = new ResolvedParameter((ParameterInfo pi, IComponentContext c) => (object)pi.ParameterType == decoratedParameterType, (ParameterInfo pi, IComponentContext c) => c.ResolveComponent(decoratedComponent, Enumerable.Empty<Parameter>()));
			return new ResolvedParameter[1] { resolvedParameter }.Concat(configuredParameters);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorRegistrationSourceResources.OpenGenericDecoratorRegistrationSourceImplFromTo, new object[3]
			{
				_activatorData.ImplementationType.FullName,
				((Service)_activatorData.FromService).Description,
				string.Join(", ", _registrationData.Services.Select((Service s) => s.Description).ToArray())
			});
		}
	}
}
