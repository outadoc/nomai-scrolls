using System;

namespace Autofac.Core.Registration
{
	internal class ModuleRegistrar : IModuleRegistrar
	{
		private ContainerBuilder _builder;

		public ModuleRegistrar(ContainerBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			_builder = builder;
		}

		public IModuleRegistrar RegisterModule(IModule module)
		{
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			_builder.RegisterCallback(module.Configure);
			return this;
		}
	}
}
