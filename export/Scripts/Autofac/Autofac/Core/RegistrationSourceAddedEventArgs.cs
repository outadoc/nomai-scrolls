using System;

namespace Autofac.Core
{
	public class RegistrationSourceAddedEventArgs : EventArgs
	{
		private readonly IComponentRegistry _componentRegistry;

		private readonly IRegistrationSource _registrationSource;

		public IRegistrationSource RegistrationSource => _registrationSource;

		public IComponentRegistry ComponentRegistry => _componentRegistry;

		public RegistrationSourceAddedEventArgs(IComponentRegistry componentRegistry, IRegistrationSource registrationSource)
		{
			if (componentRegistry == null)
			{
				throw new ArgumentNullException("componentRegistry");
			}
			if (registrationSource == null)
			{
				throw new ArgumentNullException("registrationSource");
			}
			_componentRegistry = componentRegistry;
			_registrationSource = registrationSource;
		}
	}
}
