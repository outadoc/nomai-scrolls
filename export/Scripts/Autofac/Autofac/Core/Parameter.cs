using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Autofac.Core
{
	public abstract class Parameter
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
		public abstract bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider);
	}
}
