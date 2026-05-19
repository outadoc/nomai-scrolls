namespace Autofac.Core.Registration
{
	public interface IModuleRegistrar
	{
		IModuleRegistrar RegisterModule(IModule module);
	}
}
