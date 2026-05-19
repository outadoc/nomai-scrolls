using System;
using System.Collections.Generic;

namespace Autofac.Core.Registration
{
	internal class CopyOnWriteRegistry : IComponentRegistry, IDisposable
	{
		private readonly IComponentRegistry _readRegistry;

		private readonly Func<IComponentRegistry> _createWriteRegistry;

		private IComponentRegistry _writeRegistry;

		private IComponentRegistry Registry => _writeRegistry ?? _readRegistry;

		private IComponentRegistry WriteRegistry
		{
			get
			{
				if (_writeRegistry == null)
				{
					_writeRegistry = _createWriteRegistry();
				}
				return _writeRegistry;
			}
		}

		public IEnumerable<IComponentRegistration> Registrations => Registry.Registrations;

		public IEnumerable<IRegistrationSource> Sources => Registry.Sources;

		public bool HasLocalComponents => _writeRegistry != null;

		public event EventHandler<ComponentRegisteredEventArgs> Registered
		{
			add
			{
				WriteRegistry.Registered += value;
			}
			remove
			{
				WriteRegistry.Registered -= value;
			}
		}

		public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
		{
			add
			{
				WriteRegistry.RegistrationSourceAdded += value;
			}
			remove
			{
				WriteRegistry.RegistrationSourceAdded -= value;
			}
		}

		public CopyOnWriteRegistry(IComponentRegistry readRegistry, Func<IComponentRegistry> createWriteRegistry)
		{
			if (readRegistry == null)
			{
				throw new ArgumentNullException("readRegistry");
			}
			if (createWriteRegistry == null)
			{
				throw new ArgumentNullException("createWriteRegistry");
			}
			_readRegistry = readRegistry;
			_createWriteRegistry = createWriteRegistry;
		}

		public void Dispose()
		{
			if (_readRegistry != null)
			{
				_readRegistry.Dispose();
			}
		}

		public bool TryGetRegistration(Service service, out IComponentRegistration registration)
		{
			return Registry.TryGetRegistration(service, out registration);
		}

		public bool IsRegistered(Service service)
		{
			return Registry.IsRegistered(service);
		}

		public void Register(IComponentRegistration registration)
		{
			WriteRegistry.Register(registration);
		}

		public void Register(IComponentRegistration registration, bool preserveDefaults)
		{
			WriteRegistry.Register(registration, preserveDefaults);
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
		{
			return Registry.RegistrationsFor(service);
		}

		public void AddRegistrationSource(IRegistrationSource source)
		{
			WriteRegistry.AddRegistrationSource(source);
		}
	}
}
