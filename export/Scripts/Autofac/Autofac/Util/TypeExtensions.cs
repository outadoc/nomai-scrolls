using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
	internal static class TypeExtensions
	{
		public static readonly Type[] EmptyTypes = new Type[0];

		private static readonly Type ReadOnlyCollectionType = Type.GetType("System.Collections.Generic.IReadOnlyCollection`1", throwOnError: false);

		private static readonly Type ReadOnlyListType = Type.GetType("System.Collections.Generic.IReadOnlyList`1", throwOnError: false);

		public static IEnumerable<Type> GetTypesThatClose(this Type @this, Type openGeneric)
		{
			return FindAssignableTypesThatClose(@this, openGeneric);
		}

		private static IEnumerable<Type> FindAssignableTypesThatClose(Type candidateType, Type openGenericServiceType)
		{
			return from t in TypesAssignableFrom(candidateType)
				where t.IsClosedTypeOf(openGenericServiceType)
				select t;
		}

		private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
		{
			return candidateType.GetInterfaces().Concat(Traverse.Across(candidateType, (Type t) => t.BaseType));
		}

		public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
		{
			if ((object)@this == null)
			{
				throw new ArgumentNullException("this");
			}
			if ((object)openGeneric == null)
			{
				throw new ArgumentNullException("openGeneric");
			}
			if (!@this.ContainsGenericParameters && @this.IsGenericType)
			{
				return (object)@this.GetGenericTypeDefinition() == openGeneric;
			}
			return false;
		}

		public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
		{
			if ((object)@this == null)
			{
				throw new ArgumentNullException("this");
			}
			if ((object)openGeneric == null)
			{
				throw new ArgumentNullException("openGeneric");
			}
			return TypesAssignableFrom(@this).Any((Type t) => t.IsGenericType && !@this.ContainsGenericParameters && (object)t.GetGenericTypeDefinition() == openGeneric);
		}

		public static bool IsDelegate(this Type type)
		{
			if ((object)type == null)
			{
				throw new ArgumentNullException("type");
			}
			return type.IsSubclassOf(typeof(Delegate));
		}

		public static Type FunctionReturnType(this Type type)
		{
			if ((object)type == null)
			{
				throw new ArgumentNullException("type");
			}
			MethodInfo method = type.GetMethod("Invoke");
			Enforce.NotNull(method);
			return method.ReturnType;
		}

		public static bool IsCompatibleWithGenericParameterConstraints(this Type genericTypeDefinition, Type[] parameters)
		{
			Type[] genericArguments = genericTypeDefinition.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				Type type = genericArguments[i];
				Type parameter = parameters[i];
				if (type.GetGenericParameterConstraints().Any((Type constraint) => !ParameterCompatibleWithTypeConstraint(parameter, constraint)))
				{
					return false;
				}
				GenericParameterAttributes genericParameterAttributes = type.GenericParameterAttributes;
				if ((genericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None && !parameter.IsValueType && (object)parameter.GetConstructor(EmptyTypes) == null)
				{
					return false;
				}
				if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None && parameter.IsValueType)
				{
					return false;
				}
				if ((genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None && (!parameter.IsValueType || (parameter.IsGenericType && parameter.IsGenericTypeDefinedBy(typeof(Nullable<>)))))
				{
					return false;
				}
			}
			return true;
		}

		private static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
		{
			if (!constraint.IsAssignableFrom(parameter))
			{
				return Traverse.Across(parameter, (Type p) => p.BaseType).Concat(parameter.GetInterfaces()).Any((Type p) => ParameterEqualsConstraint(p, constraint));
			}
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031", Justification = "Implementing a real TryMakeGenericType is not worth the effort.")]
		private static bool ParameterEqualsConstraint(Type parameter, Type constraint)
		{
			Type[] genericArguments = parameter.GetGenericArguments();
			if (genericArguments.Length > 0 && constraint.IsGenericType)
			{
				Type genericTypeDefinition = constraint.GetGenericTypeDefinition();
				if (genericTypeDefinition.GetGenericArguments().Length == genericArguments.Length)
				{
					try
					{
						Type type = genericTypeDefinition.MakeGenericType(genericArguments);
						return (object)type == parameter;
					}
					catch (Exception)
					{
						return false;
					}
				}
			}
			return false;
		}

		public static bool IsGenericEnumerableInterfaceType(this Type type)
		{
			if (!type.IsGenericTypeDefinedBy(typeof(IEnumerable<>)))
			{
				return type.IsGenericListOrCollectionInterfaceType();
			}
			return true;
		}

		public static bool IsGenericListOrCollectionInterfaceType(this Type type)
		{
			if (!type.IsGenericTypeDefinedBy(typeof(IList<>)) && !type.IsGenericTypeDefinedBy(typeof(ICollection<>)) && ((object)ReadOnlyCollectionType == null || !type.IsGenericTypeDefinedBy(ReadOnlyCollectionType)))
			{
				if ((object)ReadOnlyListType != null)
				{
					return type.IsGenericTypeDefinedBy(ReadOnlyListType);
				}
				return false;
			}
			return true;
		}
	}
}
