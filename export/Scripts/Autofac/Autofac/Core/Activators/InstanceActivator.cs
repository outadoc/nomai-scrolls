using System;
using Autofac.Util;

namespace Autofac.Core.Activators
{
	public abstract class InstanceActivator : Disposable
	{
		private readonly Type _limitType;

		public Type LimitType => _limitType;

		protected InstanceActivator(Type limitType)
		{
			_limitType = Enforce.ArgumentNotNull(limitType, "limitType");
		}

		public override string ToString()
		{
			return LimitType.Name + " (" + GetType().Name + ")";
		}
	}
}
