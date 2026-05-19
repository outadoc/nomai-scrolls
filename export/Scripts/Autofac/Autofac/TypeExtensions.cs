using System;
using System.Globalization;
using System.Linq;
using Autofac.Util;

namespace Autofac
{
	public static class TypeExtensions
	{
		public static bool IsInNamespace(this Type @this, string @namespace)
		{
			if ((object)@this == null)
			{
				throw new ArgumentNullException("this");
			}
			if (@namespace == null)
			{
				throw new ArgumentNullException("namespace");
			}
			if (@this.Namespace != null)
			{
				if (!(@this.Namespace == @namespace))
				{
					return @this.Namespace.StartsWith(@namespace + ".", StringComparison.Ordinal);
				}
				return true;
			}
			return false;
		}

		public static bool IsInNamespaceOf<T>(this Type @this)
		{
			if ((object)@this == null)
			{
				throw new ArgumentNullException("this");
			}
			return @this.IsInNamespace(typeof(T).Namespace);
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
			if (!openGeneric.IsGenericTypeDefinition && !openGeneric.ContainsGenericParameters)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.NotOpenGenericType, new object[1] { openGeneric.FullName }));
			}
			return @this.GetTypesThatClose(openGeneric).Any();
		}

		public static bool IsAssignableTo<T>(this Type @this)
		{
			if ((object)@this == null)
			{
				throw new ArgumentNullException("this");
			}
			return typeof(T).IsAssignableFrom(@this);
		}
	}
}
