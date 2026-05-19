using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OwnedInstances
{
	internal class OwnedInstanceRegistrationSource : IRegistrationSource
	{
		public bool IsAdapterForIndividualComponents => true;

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
			IServiceWithType ts = service as IServiceWithType;
			if (ts == null || !ts.ServiceType.IsGenericTypeDefinedBy(typeof(Owned<>)))
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Type newType = ts.ServiceType.GetGenericArguments()[0];
			Service ownedInstanceService = ts.ChangeType(newType);
			return registrationAccessor(ownedInstanceService).Select(delegate(IComponentRegistration r)
			{
				IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate(ts.ServiceType, delegate(IComponentContext c, IEnumerable<Parameter> p)
				{
					ILifetimeScope lifetimeScope = c.Resolve<ILifetimeScope>().BeginLifetimeScope(ownedInstanceService);
					try
					{
						object obj = lifetimeScope.ResolveComponent(r, p);
						return Activator.CreateInstance(ts.ServiceType, obj, lifetimeScope);
					}
					catch
					{
						lifetimeScope.Dispose();
						throw;
					}
				}).ExternallyOwned().As(service)
					.Targeting(r);
				return builder.CreateRegistration();
			});
		}

		public override string ToString()
		{
			return OwnedInstanceRegistrationSourceResources.OwnedInstanceRegistrationSourceDescription;
		}
	}
}
