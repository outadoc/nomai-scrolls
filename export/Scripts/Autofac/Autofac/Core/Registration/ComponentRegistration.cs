using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Util;

namespace Autofac.Core.Registration
{
	public class ComponentRegistration : Disposable, IComponentRegistration, IDisposable
	{
		private readonly IComponentRegistration _target;

		public IComponentRegistration Target => _target ?? this;

		public Guid Id { get; private set; }

		public IInstanceActivator Activator { get; set; }

		public IComponentLifetime Lifetime { get; private set; }

		public InstanceSharing Sharing { get; private set; }

		public InstanceOwnership Ownership { get; private set; }

		public IEnumerable<Service> Services { get; private set; }

		public IDictionary<string, object> Metadata { get; private set; }

		public event EventHandler<PreparingEventArgs> Preparing;

		public event EventHandler<ActivatingEventArgs<object>> Activating;

		public event EventHandler<ActivatedEventArgs<object>> Activated;

		public ComponentRegistration(Guid id, IInstanceActivator activator, IComponentLifetime lifetime, InstanceSharing sharing, InstanceOwnership ownership, IEnumerable<Service> services, IDictionary<string, object> metadata)
		{
			Id = id;
			Activator = Enforce.ArgumentNotNull(activator, "activator");
			Lifetime = Enforce.ArgumentNotNull(lifetime, "lifetime");
			Sharing = sharing;
			Ownership = ownership;
			Services = Enforce.ArgumentElementNotNull(Enforce.ArgumentNotNull(services, "services"), "services").ToList();
			Metadata = new Dictionary<string, object>(Enforce.ArgumentNotNull(metadata, "metadata"));
		}

		public ComponentRegistration(Guid id, IInstanceActivator activator, IComponentLifetime lifetime, InstanceSharing sharing, InstanceOwnership ownership, IEnumerable<Service> services, IDictionary<string, object> metadata, IComponentRegistration target)
			: this(id, activator, lifetime, sharing, ownership, services, metadata)
		{
			_target = Enforce.ArgumentNotNull(target, "target");
		}

		public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
		{
			EventHandler<PreparingEventArgs> preparing = this.Preparing;
			if (preparing != null)
			{
				PreparingEventArgs e = new PreparingEventArgs(context, this, parameters);
				preparing(this, e);
				parameters = e.Parameters;
			}
		}

		public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
		{
			EventHandler<ActivatingEventArgs<object>> activating = this.Activating;
			if (activating != null)
			{
				ActivatingEventArgs<object> e = new ActivatingEventArgs<object>(context, this, parameters, instance);
				activating(this, e);
				instance = e.Instance;
			}
		}

		public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
		{
			EventHandler<ActivatedEventArgs<object>> activated = this.Activated;
			if (activated != null)
			{
				ActivatedEventArgs<object> e = new ActivatedEventArgs<object>(context, this, parameters, instance);
				activated(this, e);
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, ComponentRegistrationResources.ToStringFormat, Activator, Services.Select((Service s) => s.Description).JoinWith(", "), Lifetime, Sharing, Ownership);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Activator.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
