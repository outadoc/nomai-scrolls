using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
	public class TypedParameter : ConstantParameter
	{
		[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public Type Type { get; private set; }

		public TypedParameter(Type type, object value)
			: base(value, (ParameterInfo pi) => (object)pi.ParameterType == type)
		{
			Type = Enforce.ArgumentNotNull(type, "type");
		}

		public static TypedParameter From<T>(T value)
		{
			return new TypedParameter(typeof(T), value);
		}
	}
}
