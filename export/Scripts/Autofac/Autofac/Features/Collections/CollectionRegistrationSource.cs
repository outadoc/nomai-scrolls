using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.Collections
{
	internal class CollectionRegistrationSource : IRegistrationSource
	{
		public bool IsAdapterForIndividualComponents => false;

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
			if (service is IServiceWithType serviceWithType)
			{
				Type serviceType = serviceWithType.ServiceType;
				Type elementType = null;
				if (serviceType.IsGenericEnumerableInterfaceType())
				{
					elementType = serviceType.GetGenericArguments()[0];
				}
				else if (serviceType.IsArray)
				{
					elementType = serviceType.GetElementType();
				}
				if ((object)elementType != null)
				{
					Service elementTypeService = serviceWithType.ChangeType(elementType);
					Type limitType = elementType.MakeArrayType();
					Type listType = typeof(List<>).MakeGenericType(elementType);
					bool serviceTypeIsList = serviceType.IsGenericListOrCollectionInterfaceType();
					ComponentRegistration componentRegistration = new ComponentRegistration(Guid.NewGuid(), new DelegateActivator(limitType, delegate(IComponentContext c, IEnumerable<Parameter> p)
					{
						IEnumerable<IComponentRegistration> source = c.ComponentRegistry.RegistrationsFor(elementTypeService);
						object[] array = source.Select((IComponentRegistration cr) => c.ResolveComponent(cr, p)).ToArray();
						Array array2 = Array.CreateInstance(elementType, array.Length);
						array.CopyTo(array2, 0);
						return (!serviceTypeIsList) ? array2 : Activator.CreateInstance(listType, array2);
					}), new CurrentScopeLifetime(), InstanceSharing.None, InstanceOwnership.ExternallyOwned, new Service[1] { service }, new Dictionary<string, object>());
					return new IComponentRegistration[1] { componentRegistration };
				}
			}
			return Enumerable.Empty<IComponentRegistration>();
		}

		public override string ToString()
		{
			return CollectionRegistrationSourceResources.CollectionRegistrationSourceDescription;
		}
	}
}
