using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
	internal class GeneratedFactoryRegistrationSource : IRegistrationSource
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
			if (ts != null && ts.ServiceType.IsDelegate())
			{
				Type newType = ts.ServiceType.FunctionReturnType();
				Service arg = ts.ChangeType(newType);
				return registrationAccessor(arg).Select(delegate(IComponentRegistration r)
				{
					FactoryGenerator factoryGenerator = new FactoryGenerator(ts.ServiceType, r, ParameterMapping.Adaptive);
					IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> builder = RegistrationBuilder.ForDelegate(ts.ServiceType, factoryGenerator.GenerateFactory).InstancePerLifetimeScope().ExternallyOwned()
						.As(service)
						.Targeting(r);
					return builder.CreateRegistration();
				});
			}
			return Enumerable.Empty<IComponentRegistration>();
		}

		public override string ToString()
		{
			return GeneratedFactoryRegistrationSourceResources.GeneratedFactoryRegistrationSourceDescription;
		}
	}
}
