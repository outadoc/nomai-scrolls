using System;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core
{
	public class ResolvedParameter : Parameter
	{
		private readonly Func<ParameterInfo, IComponentContext, bool> _predicate;

		private readonly Func<ParameterInfo, IComponentContext, object> _valueAccessor;

		public ResolvedParameter(Func<ParameterInfo, IComponentContext, bool> predicate, Func<ParameterInfo, IComponentContext, object> valueAccessor)
		{
			_predicate = Enforce.ArgumentNotNull(predicate, "predicate");
			_valueAccessor = Enforce.ArgumentNotNull(valueAccessor, "valueAccessor");
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
			if (_predicate(pi, context))
			{
				valueProvider = () => _valueAccessor(pi, context);
				return true;
			}
			valueProvider = null;
			return false;
		}

		public static ResolvedParameter ForNamed<TService>(string serviceName)
		{
			if (serviceName == null)
			{
				throw new ArgumentNullException("serviceName");
			}
			return ForKeyed<TService>(serviceName);
		}

		public static ResolvedParameter ForKeyed<TService>(object serviceKey)
		{
			if (serviceKey == null)
			{
				throw new ArgumentNullException("serviceKey");
			}
			KeyedService ks = new KeyedService(serviceKey, typeof(TService));
			return new ResolvedParameter((ParameterInfo pi, IComponentContext c) => (object)pi.ParameterType == typeof(TService) && c.IsRegisteredService(ks), (ParameterInfo pi, IComponentContext c) => c.ResolveService(ks));
		}
	}
}
