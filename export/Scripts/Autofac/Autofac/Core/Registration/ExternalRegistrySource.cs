using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;

namespace Autofac.Core.Registration
{
	internal class ExternalRegistrySource : IRegistrationSource
	{
		private readonly IComponentRegistry _registry;

		public bool IsAdapterForIndividualComponents => false;

		public ExternalRegistrySource(IComponentRegistry registry)
		{
			if (registry == null)
			{
				throw new ArgumentNullException("registry");
			}
			_registry = registry;
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			HashSet<IComponentRegistration> seenRegistrations = new HashSet<IComponentRegistration>();
			HashSet<Service> seenServices = new HashSet<Service>();
			List<Service> lastRunServices = new List<Service> { service };
			while (lastRunServices.Any())
			{
				Service nextService = lastRunServices.First();
				lastRunServices.Remove(nextService);
				seenServices.Add(nextService);
				foreach (IComponentRegistration registration in from componentRegistration in _registry.RegistrationsFor(nextService)
					where !componentRegistration.IsAdapting()
					select componentRegistration)
				{
					if (!seenRegistrations.Contains(registration))
					{
						seenRegistrations.Add(registration);
						lastRunServices.AddRange(registration.Services.Where((Service s) => !seenServices.Contains(s)));
						IComponentRegistration r = registration;
						yield return RegistrationBuilder.ForDelegate(r.Activator.LimitType, (IComponentContext c, IEnumerable<Parameter> p) => c.ResolveComponent(r, p)).Targeting(r).As(r.Services.ToArray())
							.ExternallyOwned()
							.CreateRegistration();
					}
				}
			}
		}
	}
}
