using System;
using System.Collections.Generic;

namespace Autofac.Core
{
	public interface IRegistrationSource
	{
		bool IsAdapterForIndividualComponents { get; }

		IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor);
	}
}
