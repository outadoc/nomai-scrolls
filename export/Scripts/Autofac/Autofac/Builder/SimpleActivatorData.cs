using Autofac.Core;
using Autofac.Util;

namespace Autofac.Builder
{
	public class SimpleActivatorData : IConcreteActivatorData
	{
		private readonly IInstanceActivator _activator;

		public IInstanceActivator Activator => _activator;

		public SimpleActivatorData(IInstanceActivator activator)
		{
			_activator = Enforce.ArgumentNotNull(activator, "activator");
		}
	}
}
