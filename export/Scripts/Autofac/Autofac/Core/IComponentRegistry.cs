using System;
using System.Collections.Generic;

namespace Autofac.Core
{
	public interface IComponentRegistry : IDisposable
	{
		IEnumerable<IComponentRegistration> Registrations { get; }

		IEnumerable<IRegistrationSource> Sources { get; }

		bool HasLocalComponents { get; }

		event EventHandler<ComponentRegisteredEventArgs> Registered;

		event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;

		bool TryGetRegistration(Service service, out IComponentRegistration registration);

		bool IsRegistered(Service service);

		void Register(IComponentRegistration registration);

		void Register(IComponentRegistration registration, bool preserveDefaults);

		IEnumerable<IComponentRegistration> RegistrationsFor(Service service);

		void AddRegistrationSource(IRegistrationSource source);
	}
}
