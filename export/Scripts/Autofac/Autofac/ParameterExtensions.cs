using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
	public static class ParameterExtensions
	{
		public static T Named<T>(this IEnumerable<Parameter> parameters, string name)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			Enforce.ArgumentNotNullOrEmpty(name, "name");
			return ConstantValue<NamedParameter, T>(parameters, (NamedParameter c) => c.Name == name);
		}

		public static T Positional<T>(this IEnumerable<Parameter> parameters, int position)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			return ConstantValue<PositionalParameter, T>(parameters, (PositionalParameter c) => c.Position == position);
		}

		public static T TypedAs<T>(this IEnumerable<Parameter> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			return ConstantValue<TypedParameter, T>(parameters, (TypedParameter c) => (object)c.Type == typeof(T));
		}

		private static TValue ConstantValue<TParameter, TValue>(IEnumerable<Parameter> parameters, Func<TParameter, bool> predicate) where TParameter : ConstantParameter
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (predicate == null)
			{
				throw new ArgumentNullException("predicate");
			}
			return (from p in parameters.OfType<TParameter>().Where(predicate)
				select p.Value).Cast<TValue>().First();
		}
	}
}
