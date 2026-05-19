using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Autofac.Util;

namespace Autofac.Core.Activators.Delegate
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
	public class DelegateActivator : InstanceActivator, IInstanceActivator, IDisposable
	{
		private readonly Func<IComponentContext, IEnumerable<Parameter>, object> _activationFunction;

		public DelegateActivator(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> activationFunction)
			: base(limitType)
		{
			_activationFunction = Enforce.ArgumentNotNull(activationFunction, "activationFunction");
		}

		public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			object obj = _activationFunction(context, parameters);
			if (obj == null)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, DelegateActivatorResources.NullFromActivationDelegateFor, new object[1] { base.LimitType }));
			}
			return obj;
		}
	}
}
