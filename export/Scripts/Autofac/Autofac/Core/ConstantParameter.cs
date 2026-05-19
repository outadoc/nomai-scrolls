using System;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core
{
	public abstract class ConstantParameter : Parameter
	{
		private Predicate<ParameterInfo> _predicate;

		public object Value { get; private set; }

		protected ConstantParameter(object value, Predicate<ParameterInfo> predicate)
		{
			Value = value;
			_predicate = Enforce.ArgumentNotNull(predicate, "predicate");
		}

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
			if (_predicate(pi))
			{
				valueProvider = () => Value;
				return true;
			}
			valueProvider = null;
			return false;
		}
	}
}
