using Autofac.Core;

namespace Autofac.Builder
{
	public interface IConcreteActivatorData
	{
		IInstanceActivator Activator { get; }
	}
}
