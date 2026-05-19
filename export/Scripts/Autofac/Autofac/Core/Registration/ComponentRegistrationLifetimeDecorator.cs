using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core.Registration
{
	internal class ComponentRegistrationLifetimeDecorator : Disposable, IComponentRegistration, IDisposable
	{
		private readonly IComponentLifetime _lifetime;

		private readonly IComponentRegistration _inner;

		public Guid Id => _inner.Id;

		public IInstanceActivator Activator => _inner.Activator;

		public IComponentLifetime Lifetime => _lifetime;

		public InstanceSharing Sharing => _inner.Sharing;

		public InstanceOwnership Ownership => _inner.Ownership;

		public IEnumerable<Service> Services => _inner.Services;

		public IDictionary<string, object> Metadata => _inner.Metadata;

		public IComponentRegistration Target
		{
			get
			{
				if (_inner.IsAdapting())
				{
					return _inner.Target;
				}
				return this;
			}
		}

		public event EventHandler<PreparingEventArgs> Preparing
		{
			add
			{
				_inner.Preparing += value;
			}
			remove
			{
				_inner.Preparing -= value;
			}
		}

		public event EventHandler<ActivatingEventArgs<object>> Activating
		{
			add
			{
				_inner.Activating += value;
			}
			remove
			{
				_inner.Activating -= value;
			}
		}

		public event EventHandler<ActivatedEventArgs<object>> Activated
		{
			add
			{
				_inner.Activated += value;
			}
			remove
			{
				_inner.Activated -= value;
			}
		}

		public ComponentRegistrationLifetimeDecorator(IComponentRegistration inner, IComponentLifetime lifetime)
		{
			if (inner == null)
			{
				throw new ArgumentNullException("inner");
			}
			if (lifetime == null)
			{
				throw new ArgumentNullException("lifetime");
			}
			_inner = inner;
			_lifetime = lifetime;
		}

		public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
		{
			_inner.RaisePreparing(context, ref parameters);
		}

		public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
		{
			_inner.RaiseActivating(context, parameters, ref instance);
		}

		public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
		{
			_inner.RaiseActivated(context, parameters, instance);
		}
	}
}
