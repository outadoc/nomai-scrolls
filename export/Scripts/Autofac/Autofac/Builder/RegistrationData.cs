using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Util;

namespace Autofac.Builder
{
	public class RegistrationData
	{
		private bool _defaultServiceOverridden;

		private Service _defaultService;

		private readonly ICollection<Service> _services = new HashSet<Service>();

		private InstanceOwnership _ownership = InstanceOwnership.OwnedByLifetimeScope;

		private IComponentLifetime _lifetime = new CurrentScopeLifetime();

		private InstanceSharing _sharing;

		private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

		private readonly ICollection<EventHandler<PreparingEventArgs>> _preparingHandlers = new List<EventHandler<PreparingEventArgs>>();

		private readonly ICollection<EventHandler<ActivatingEventArgs<object>>> _activatingHandlers = new List<EventHandler<ActivatingEventArgs<object>>>();

		private readonly ICollection<EventHandler<ActivatedEventArgs<object>>> _activatedHandlers = new List<EventHandler<ActivatedEventArgs<object>>>();

		public IEnumerable<Service> Services
		{
			get
			{
				if (_defaultServiceOverridden)
				{
					return _services;
				}
				return _services.DefaultIfEmpty(_defaultService);
			}
		}

		public InstanceOwnership Ownership
		{
			get
			{
				return _ownership;
			}
			set
			{
				_ownership = value;
			}
		}

		public IComponentLifetime Lifetime
		{
			get
			{
				return _lifetime;
			}
			set
			{
				_lifetime = Enforce.ArgumentNotNull(value, "lifetime");
			}
		}

		public InstanceSharing Sharing
		{
			get
			{
				return _sharing;
			}
			set
			{
				_sharing = value;
			}
		}

		public IDictionary<string, object> Metadata => _metadata;

		public ICollection<EventHandler<PreparingEventArgs>> PreparingHandlers => _preparingHandlers;

		public ICollection<EventHandler<ActivatingEventArgs<object>>> ActivatingHandlers => _activatingHandlers;

		public ICollection<EventHandler<ActivatedEventArgs<object>>> ActivatedHandlers => _activatedHandlers;

		public RegistrationData(Service defaultService)
		{
			if (defaultService == null)
			{
				throw new ArgumentNullException("defaultService");
			}
			_defaultService = defaultService;
		}

		public void AddServices(IEnumerable<Service> services)
		{
			if (services == null)
			{
				throw new ArgumentNullException("services");
			}
			_defaultServiceOverridden = true;
			foreach (Service service in services)
			{
				AddService(service);
			}
		}

		public void AddService(Service service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			_defaultServiceOverridden = true;
			_services.Add(service);
		}

		public void CopyFrom(RegistrationData that, bool includeDefaultService)
		{
			if (that == null)
			{
				throw new ArgumentNullException("that");
			}
			Ownership = that.Ownership;
			Sharing = that.Sharing;
			Lifetime = that.Lifetime;
			_defaultServiceOverridden |= that._defaultServiceOverridden;
			if (includeDefaultService)
			{
				_defaultService = that._defaultService;
			}
			AddAll(_services, that._services);
			AddAll(Metadata, that.Metadata);
			AddAll(PreparingHandlers, that.PreparingHandlers);
			AddAll(ActivatingHandlers, that.ActivatingHandlers);
			AddAll(ActivatedHandlers, that.ActivatedHandlers);
		}

		private static void AddAll<T>(ICollection<T> to, IEnumerable<T> from)
		{
			foreach (T item in from)
			{
				to.Add(item);
			}
		}

		public void ClearServices()
		{
			_services.Clear();
			_defaultServiceOverridden = true;
		}
	}
}
