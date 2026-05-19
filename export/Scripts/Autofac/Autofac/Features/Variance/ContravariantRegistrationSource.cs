using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Variance
{
	public class ContravariantRegistrationSource : IRegistrationSource
	{
		private const string IsContravariantAdapter = "IsContravariantAdapter";

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
			IServiceWithType swt = service as IServiceWithType;
			if (swt == null || !IsCompatibleInterfaceType(swt.ServiceType, out var contravariantParameterIndex))
			{
				return Enumerable.Empty<IComponentRegistration>();
			}
			Type[] args = swt.ServiceType.GetGenericArguments();
			Type definition = swt.ServiceType.GetGenericTypeDefinition();
			Type type = args[contravariantParameterIndex];
			IEnumerable<Type> typesAssignableFrom = GetTypesAssignableFrom(type);
			IEnumerable<Type> source = from s in typesAssignableFrom
				select SubstituteArrayElementAt(args, s, contravariantParameterIndex) into a
				where definition.IsCompatibleWithGenericParameterConstraints(a)
				select definition.MakeGenericType(a);
			IEnumerable<IComponentRegistration> source2 = from r in source.SelectMany((Type v) => registrationAccessor(swt.ChangeType(v)))
				where !r.Metadata.ContainsKey("IsContravariantAdapter")
				select r;
			return source2.Select((IComponentRegistration vr) => RegistrationBuilder.ForDelegate((IComponentContext c, IEnumerable<Parameter> p) => c.ResolveComponent(vr, p)).Targeting(vr).As(service)
				.WithMetadata("IsContravariantAdapter", true)
				.CreateRegistration());
		}

		private static Type[] SubstituteArrayElementAt(Type[] array, Type newElement, int index)
		{
			Type[] array2 = array.ToArray();
			array2[index] = newElement;
			return array2;
		}

		private static IEnumerable<Type> GetTypesAssignableFrom(Type type)
		{
			return GetBagOfTypesAssignableFrom(type).Distinct();
		}

		private static IEnumerable<Type> GetBagOfTypesAssignableFrom(Type type)
		{
			if ((object)type.BaseType != null)
			{
				yield return type.BaseType;
				foreach (Type item in GetBagOfTypesAssignableFrom(type.BaseType))
				{
					yield return item;
				}
			}
			else if ((object)type != typeof(object))
			{
				yield return typeof(object);
			}
			try
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type ifce in interfaces)
				{
					if ((object)ifce == type)
					{
						continue;
					}
					yield return ifce;
					foreach (Type item2 in GetBagOfTypesAssignableFrom(ifce))
					{
						yield return item2;
					}
				}
			}
			finally
			{
			}
		}

		private static bool IsCompatibleInterfaceType(Type type, out int contravariantParameterIndex)
		{
			if (type.IsGenericType && type.IsInterface)
			{
				var array = (from cwi in type.GetGenericTypeDefinition().GetGenericArguments().Select((Type c, int i) => new
					{
						IsContravariant = ((c.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0),
						Index = i
					})
					where cwi.IsContravariant
					select cwi).ToArray();
				if (array.Length == 1)
				{
					contravariantParameterIndex = array[0].Index;
					return true;
				}
			}
			contravariantParameterIndex = 0;
			return false;
		}
	}
}
