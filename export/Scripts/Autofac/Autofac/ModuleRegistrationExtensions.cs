using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
	public static class ModuleRegistrationExtensions
	{
		public static IModuleRegistrar RegisterAssemblyModules(this ContainerBuilder builder, params Assembly[] assemblies)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			ModuleRegistrar registrar = new ModuleRegistrar(builder);
			return registrar.RegisterAssemblyModules<IModule>(assemblies);
		}

		public static IModuleRegistrar RegisterAssemblyModules(this IModuleRegistrar registrar, params Assembly[] assemblies)
		{
			if (registrar == null)
			{
				throw new ArgumentNullException("registrar");
			}
			return registrar.RegisterAssemblyModules<IModule>(assemblies);
		}

		public static IModuleRegistrar RegisterAssemblyModules<TModule>(this ContainerBuilder builder, params Assembly[] assemblies) where TModule : IModule
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			ModuleRegistrar registrar = new ModuleRegistrar(builder);
			return registrar.RegisterAssemblyModules(typeof(TModule), assemblies);
		}

		public static IModuleRegistrar RegisterAssemblyModules<TModule>(this IModuleRegistrar registrar, params Assembly[] assemblies) where TModule : IModule
		{
			if (registrar == null)
			{
				throw new ArgumentNullException("registrar");
			}
			return registrar.RegisterAssemblyModules(typeof(TModule), assemblies);
		}

		public static IModuleRegistrar RegisterAssemblyModules(this ContainerBuilder builder, Type moduleType, params Assembly[] assemblies)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if ((object)moduleType == null)
			{
				throw new ArgumentNullException("moduleType");
			}
			ModuleRegistrar registrar = new ModuleRegistrar(builder);
			return registrar.RegisterAssemblyModules(moduleType, assemblies);
		}

		public static IModuleRegistrar RegisterAssemblyModules(this IModuleRegistrar registrar, Type moduleType, params Assembly[] assemblies)
		{
			if (registrar == null)
			{
				throw new ArgumentNullException("registrar");
			}
			if ((object)moduleType == null)
			{
				throw new ArgumentNullException("moduleType");
			}
			ContainerBuilder containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterAssemblyTypes(assemblies).Where(moduleType.IsAssignableFrom).As<IModule>();
			using (IContainer context = containerBuilder.Build())
			{
				foreach (IModule item in context.Resolve<IEnumerable<IModule>>())
				{
					registrar.RegisterModule(item);
				}
				return registrar;
			}
		}

		public static IModuleRegistrar RegisterModule<TModule>(this ContainerBuilder builder) where TModule : IModule, new()
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			ModuleRegistrar registrar = new ModuleRegistrar(builder);
			return registrar.RegisterModule<TModule>();
		}

		public static IModuleRegistrar RegisterModule<TModule>(this IModuleRegistrar registrar) where TModule : IModule, new()
		{
			if (registrar == null)
			{
				throw new ArgumentNullException("registrar");
			}
			return registrar.RegisterModule(new TModule());
		}

		public static IModuleRegistrar RegisterModule(this ContainerBuilder builder, IModule module)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (module == null)
			{
				throw new ArgumentNullException("module");
			}
			ModuleRegistrar moduleRegistrar = new ModuleRegistrar(builder);
			return moduleRegistrar.RegisterModule(module);
		}
	}
}
