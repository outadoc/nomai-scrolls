using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
	internal class LightweightAdapterRegistrationSource : IRegistrationSource
	{
		private readonly RegistrationData _registrationData;

		private readonly LightweightAdapterActivatorData _activatorData;

		public bool IsAdapterForIndividualComponents => true;

		public LightweightAdapterRegistrationSource(RegistrationData registrationData, LightweightAdapterActivatorData activatorData)
		{
			if (registrationData == null)
			{
				throw new ArgumentNullException("registrationData");
			}
			if (activatorData == null)
			{
				throw new ArgumentNullException("activatorData");
			}
			_registrationData = registrationData;
			_activatorData = activatorData;
			if (registrationData.Services.Contains(activatorData.FromService))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, LightweightAdapterRegistrationSourceResources.FromAndToMustDiffer, new object[1] { activatorData.FromService }));
			}
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
			if (_registrationData.Services.Contains(service))
			{
				return registrationAccessor(_activatorData.FromService).Select(delegate(IComponentRegistration r)
				{
					IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registrationBuilder = RegistrationBuilder.ForDelegate((IComponentContext c, IEnumerable<Parameter> p) => _activatorData.Adapter(c, p, c.ResolveComponent(r, Enumerable.Empty<Parameter>()))).Targeting(r);
					registrationBuilder.RegistrationData.CopyFrom(_registrationData, includeDefaultService: true);
					return registrationBuilder.CreateRegistration();
				});
			}
			IServiceWithType requestedServiceWithType = service as IServiceWithType;
			IServiceWithType serviceWithType = _activatorData.FromService as IServiceWithType;
			if (requestedServiceWithType != null && serviceWithType != null && (object)requestedServiceWithType.ServiceType != serviceWithType.ServiceType && _registrationData.Services.OfType<IServiceWithType>().Any((IServiceWithType s) => (object)s.ServiceType == requestedServiceWithType.ServiceType))
			{
				Service arg = requestedServiceWithType.ChangeType(serviceWithType.ServiceType);
				return registrationAccessor(arg).Select(delegate(IComponentRegistration r)
				{
					IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registrationBuilder = RegistrationBuilder.ForDelegate((IComponentContext c, IEnumerable<Parameter> p) => _activatorData.Adapter(c, p, c.ResolveComponent(r, Enumerable.Empty<Parameter>()))).Targeting(r);
					registrationBuilder.RegistrationData.CopyFrom(_registrationData, includeDefaultService: true);
					registrationBuilder.RegistrationData.AddService(service);
					return registrationBuilder.CreateRegistration();
				});
			}
			return new IComponentRegistration[0];
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, LightweightAdapterRegistrationSourceResources.AdapterFromToDescription, new object[2]
			{
				_activatorData.FromService.Description,
				string.Join(", ", _registrationData.Services.Select((Service s) => s.Description).ToArray())
			});
		}
	}
}
