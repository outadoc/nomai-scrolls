using System.Collections.Generic;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
	internal class AdaptationSandbox
	{
		private readonly IEnumerable<IRegistrationSource> _adapters;

		private readonly IComponentRegistration _registration;

		private readonly IEnumerable<Service> _adapterServices;

		private readonly IDictionary<Service, IList<IRegistrationSource>> _adaptersToQuery = new Dictionary<Service, IList<IRegistrationSource>>();

		private readonly IList<IComponentRegistration> _registrations = new List<IComponentRegistration>();

		public AdaptationSandbox(IEnumerable<IRegistrationSource> adapters, IComponentRegistration registration, IEnumerable<Service> adapterServices)
		{
			_adapters = adapters;
			_registration = registration;
			_adapterServices = adapterServices;
			_registrations.Add(_registration);
		}

		public IEnumerable<IComponentRegistration> GetAdapters()
		{
			foreach (Service adapterService in _adapterServices)
			{
				GetAndInitialiseRegistrationsFor(adapterService);
			}
			return _registrations.Where((IComponentRegistration r) => r != _registration);
		}

		private IEnumerable<IComponentRegistration> GetAndInitialiseRegistrationsFor(Service service)
		{
			if (!_adaptersToQuery.TryGetValue(service, out var value))
			{
				value = new List<IRegistrationSource>(_adapters);
				_adaptersToQuery.Add(service, value);
			}
			foreach (IRegistrationSource adapter in _adapters)
			{
				value.Remove(adapter);
				IComponentRegistration[] items = adapter.RegistrationsFor(service, GetAndInitialiseRegistrationsFor).ToArray();
				_registrations.AddRange(items);
			}
			return _registrations.Where((IComponentRegistration r) => r.Services.Contains(service));
		}
	}
}
