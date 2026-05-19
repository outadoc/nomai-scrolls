using System;
using System.Reflection;
using Autofac.Core;

namespace Autofac
{
	public abstract class Module : IModule
	{
		protected virtual Assembly ThisAssembly
		{
			get
			{
				Type type = GetType();
				if ((object)type.BaseType != typeof(Module))
				{
					throw new InvalidOperationException(ModuleResources.ThisAssemblyUnavailable);
				}
				return type.Assembly;
			}
		}

		public void Configure(IComponentRegistry componentRegistry)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			ContainerBuilder containerBuilder = new ContainerBuilder();
			Load(containerBuilder);
			containerBuilder.Update(componentRegistry);
			AttachToRegistrations(componentRegistry);
			AttachToSources(componentRegistry);
		}

		protected virtual void Load(ContainerBuilder builder)
		{
		}

		protected virtual void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
		}

		protected virtual void AttachToRegistrationSource(IComponentRegistry componentRegistry, IRegistrationSource registrationSource)
		{
		}

		private void AttachToRegistrations(IComponentRegistry componentRegistry)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			foreach (IComponentRegistration registration in componentRegistry.Registrations)
			{
				AttachToComponentRegistration(componentRegistry, registration);
			}
			componentRegistry.Registered += delegate(object sender, ComponentRegisteredEventArgs e)
			{
				AttachToComponentRegistration(e.ComponentRegistry, e.ComponentRegistration);
			};
		}

		private void AttachToSources(IComponentRegistry componentRegistry)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			foreach (IRegistrationSource source in componentRegistry.Sources)
			{
				AttachToRegistrationSource(componentRegistry, source);
			}
			componentRegistry.RegistrationSourceAdded += delegate(object sender, RegistrationSourceAddedEventArgs e)
			{
				AttachToRegistrationSource(e.ComponentRegistry, e.RegistrationSource);
			};
		}
	}
}
