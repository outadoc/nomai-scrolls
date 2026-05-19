using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics
{
	internal static class OpenGenericServiceBinder
	{
		public static bool TryBindServiceType(Service service, IEnumerable<Service> configuredOpenGenericServices, Type openGenericImplementationType, out Type constructedImplementationType, out IEnumerable<Service> constructedServices)
		{
			if (service is IServiceWithType serviceWithType && serviceWithType.ServiceType.IsGenericType)
			{
				IServiceWithType definitionService = (IServiceWithType)serviceWithType.ChangeType(serviceWithType.ServiceType.GetGenericTypeDefinition());
				Type[] serviceGenericArguments = serviceWithType.ServiceType.GetGenericArguments();
				if (configuredOpenGenericServices.Cast<IServiceWithType>().Any((IServiceWithType s) => s.Equals(definitionService)))
				{
					Type[] array = TryMapImplementationGenericArguments(openGenericImplementationType, serviceWithType.ServiceType, definitionService.ServiceType, serviceGenericArguments);
					if (!array.Any((Type a) => (object)a == null) && openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(array))
					{
						Type constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(array);
						Service[] array2 = (from IServiceWithType s in configuredOpenGenericServices
							let genericService = s.ServiceType.MakeGenericType(serviceGenericArguments)
							where genericService.IsAssignableFrom(constructedImplementationTypeTmp)
							select s.ChangeType(genericService)).ToArray();
						if (array2.Length > 0)
						{
							constructedImplementationType = constructedImplementationTypeTmp;
							constructedServices = array2;
							return true;
						}
					}
				}
			}
			constructedImplementationType = null;
			constructedServices = null;
			return false;
		}

		private static Type[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
		{
			if ((object)serviceTypeDefinition == implementationType)
			{
				return serviceGenericArguments;
			}
			Type[] genericArguments = implementationType.GetGenericArguments();
			Type[] first = (serviceType.IsInterface ? GetInterface(implementationType, serviceType).GetGenericArguments() : serviceTypeDefinition.GetGenericArguments());
			IEnumerable<KeyValuePair<Type, Type>> serviceArgumentDefinitionToArgumentMapping = first.Zip(serviceGenericArguments, (Type a, Type b) => new KeyValuePair<Type, Type>(a, b));
			return genericArguments.Select((Type implementationGenericArgumentDefinition) => TryFindServiceArgumentForImplementationArgumentDefinition(implementationGenericArgumentDefinition, serviceArgumentDefinitionToArgumentMapping)).ToArray();
		}

		private static Type GetInterface(Type implementationType, Type serviceType)
		{
			try
			{
				return implementationType.GetInterfaces().Single((Type i) => i.Name == serviceType.Name && i.Namespace == serviceType.Namespace);
			}
			catch (InvalidOperationException)
			{
				string message = string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorDoesntImplementService, new object[2] { implementationType.FullName, serviceType.FullName });
				throw new InvalidOperationException(message);
			}
		}

		private static Type TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, IEnumerable<KeyValuePair<Type, Type>> serviceArgumentDefinitionToArgument)
		{
			Type type = (from argdef in serviceArgumentDefinitionToArgument
				where !argdef.Key.IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name
				select argdef.Value).FirstOrDefault();
			if ((object)type != null)
			{
				return type;
			}
			return (from argdef in serviceArgumentDefinitionToArgument
				where argdef.Key.IsGenericType && argdef.Value.GetGenericArguments().Length > 0
				select TryFindServiceArgumentForImplementationArgumentDefinition(implementationGenericArgumentDefinition, argdef.Key.GetGenericArguments().Zip(argdef.Value.GetGenericArguments(), (Type a, Type b) => new KeyValuePair<Type, Type>(a, b)))).FirstOrDefault();
		}

		public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
		{
			if ((object)implementationType == null)
			{
				throw new ArgumentNullException("implementationType");
			}
			if (services == null)
			{
				throw new ArgumentNullException("services");
			}
			if (!implementationType.IsGenericTypeDefinition)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, new object[1] { implementationType }));
			}
			foreach (IServiceWithType service in services)
			{
				if (!service.ServiceType.IsGenericTypeDefinition)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, new object[1] { service }));
				}
				if (service.ServiceType.IsInterface)
				{
					if ((object)GetInterface(implementationType, service.ServiceType) == null)
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.InterfaceIsNotImplemented, new object[2] { implementationType, service }));
					}
				}
				else if (!Traverse.Across(implementationType, (Type t) => t.BaseType).Any((Type t) => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.TypesAreNotConvertible, new object[2] { implementationType, service }));
				}
			}
		}

		private static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
		{
			if ((object)implementor != serviceType)
			{
				if (implementor.IsGenericType)
				{
					return (object)implementor.GetGenericTypeDefinition() == serviceType;
				}
				return false;
			}
			return true;
		}
	}
}
