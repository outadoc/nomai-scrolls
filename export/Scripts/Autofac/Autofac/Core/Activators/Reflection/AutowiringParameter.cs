using System;
using System.Linq;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
	public class AutowiringParameter : Parameter
	{
		public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
		{
			if (pi == null)
			{
				throw new ArgumentNullException("pi");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (context.ComponentRegistry.TryGetRegistration(new TypedService(pi.ParameterType), out var registration))
			{
				valueProvider = () => context.ResolveComponent(registration, Enumerable.Empty<Parameter>());
				return true;
			}
			valueProvider = null;
			return false;
		}
	}
}
