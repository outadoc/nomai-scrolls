using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
	public class DefaultValueParameter : Parameter
	{
		public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
		{
			if (pi == null)
			{
				throw new ArgumentNullException("pi");
			}
			if (pi.DefaultValue == null || pi.DefaultValue.GetType().FullName != "System.DBNull")
			{
				valueProvider = () => pi.DefaultValue;
				return true;
			}
			valueProvider = null;
			return false;
		}
	}
}
