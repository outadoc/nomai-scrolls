using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
	public class ComponentRegistry : Disposable, IComponentRegistry, IDisposable
	{
		private readonly object _synchRoot = new object();

		private readonly IList<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

		private readonly ICollection<IComponentRegistration> _registrations = new List<IComponentRegistration>();

		private readonly IDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new Dictionary<Service, ServiceRegistrationInfo>();

		public IEnumerable<IComponentRegistration> Registrations
		{
			get
			{
				lock (_synchRoot)
				{
					return _registrations.ToArray();
				}
			}
		}

		public IEnumerable<IRegistrationSource> Sources
		{
			get
			{
				lock (_synchRoot)
				{
					return _dynamicRegistrationSources.ToArray();
				}
			}
		}

		public bool HasLocalComponents => true;

		public event EventHandler<ComponentRegisteredEventArgs> Registered;

		public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded;

		protected override void Dispose(bool disposing)
		{
			foreach (IComponentRegistration registration in _registrations)
			{
				registration.Dispose();
			}
			base.Dispose(disposing);
		}

		public bool TryGetRegistration(Service service, out IComponentRegistration registration)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			lock (_synchRoot)
			{
				ServiceRegistrationInfo initializedServiceInfo = GetInitializedServiceInfo(service);
				return initializedServiceInfo.TryGetRegistration(out registration);
			}
		}

		public bool IsRegistered(Service service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			lock (_synchRoot)
			{
				return GetInitializedServiceInfo(service).IsRegistered;
			}
		}

		public void Register(IComponentRegistration registration)
		{
			Register(registration, preserveDefaults: false);
		}

		public virtual void Register(IComponentRegistration registration, bool preserveDefaults)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			lock (_synchRoot)
			{
				AddRegistration(registration, preserveDefaults);
				UpdateInitialisedAdapters(registration);
			}
		}

		private void UpdateInitialisedAdapters(IComponentRegistration registration)
		{
			Service[] array = (from si in _serviceInfo
				where si.Value.ShouldRecalculateAdaptersOn(registration)
				select si.Key).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			AdaptationSandbox adaptationSandbox = new AdaptationSandbox(_dynamicRegistrationSources.Where((IRegistrationSource rs) => rs.IsAdapterForIndividualComponents), registration, array);
			IEnumerable<IComponentRegistration> adapters = adaptationSandbox.GetAdapters();
			foreach (IComponentRegistration item in adapters)
			{
				AddRegistration(item, preserveDefaults: true);
			}
		}

		private void AddRegistration(IComponentRegistration registration, bool preserveDefaults)
		{
			foreach (Service service in registration.Services)
			{
				ServiceRegistrationInfo serviceInfo = GetServiceInfo(service);
				serviceInfo.AddImplementation(registration, preserveDefaults);
			}
			_registrations.Add(registration);
			this.Registered?.Invoke(this, new ComponentRegisteredEventArgs(this, registration));
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			lock (_synchRoot)
			{
				ServiceRegistrationInfo initializedServiceInfo = GetInitializedServiceInfo(service);
				return initializedServiceInfo.Implementations.ToArray();
			}
		}

		public void AddRegistrationSource(IRegistrationSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			lock (_synchRoot)
			{
				_dynamicRegistrationSources.Insert(0, source);
				foreach (KeyValuePair<Service, ServiceRegistrationInfo> item in _serviceInfo)
				{
					item.Value.Include(source);
				}
				this.RegistrationSourceAdded?.Invoke(this, new RegistrationSourceAddedEventArgs(this, source));
			}
		}

		private ServiceRegistrationInfo GetInitializedServiceInfo(Service service)
		{
			ServiceRegistrationInfo serviceInfo = GetServiceInfo(service);
			if (serviceInfo.IsInitialized)
			{
				return serviceInfo;
			}
			if (!serviceInfo.IsInitializing)
			{
				serviceInfo.BeginInitialization(_dynamicRegistrationSources);
			}
			while (serviceInfo.HasSourcesToQuery)
			{
				IRegistrationSource next = serviceInfo.DequeueNextSource();
				foreach (IComponentRegistration item in next.RegistrationsFor(service, RegistrationsFor))
				{
					foreach (Service service2 in item.Services)
					{
						ServiceRegistrationInfo serviceInfo2 = GetServiceInfo(service2);
						if (serviceInfo2.IsInitialized)
						{
							continue;
						}
						if (!serviceInfo2.IsInitializing)
						{
							serviceInfo2.BeginInitialization(_dynamicRegistrationSources.Where((IRegistrationSource src) => src != next));
						}
						else
						{
							serviceInfo2.SkipSource(next);
						}
					}
					AddRegistration(item, preserveDefaults: true);
				}
			}
			serviceInfo.CompleteInitialization();
			return serviceInfo;
		}

		private ServiceRegistrationInfo GetServiceInfo(Service service)
		{
			if (_serviceInfo.TryGetValue(service, out var value))
			{
				return value;
			}
			ServiceRegistrationInfo serviceRegistrationInfo = new ServiceRegistrationInfo(service);
			_serviceInfo.Add(service, serviceRegistrationInfo);
			return serviceRegistrationInfo;
		}
	}
}
