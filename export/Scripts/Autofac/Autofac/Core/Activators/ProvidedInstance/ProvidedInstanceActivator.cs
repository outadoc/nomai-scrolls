using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core.Activators.ProvidedInstance
{
	public class ProvidedInstanceActivator : InstanceActivator, IInstanceActivator, IDisposable
	{
		private readonly object _instance;

		private bool _activated;

		private bool _disposeInstance;

		public bool DisposeInstance
		{
			get
			{
				return _disposeInstance;
			}
			set
			{
				_disposeInstance = value;
			}
		}

		public ProvidedInstanceActivator(object instance)
			: base(Enforce.ArgumentNotNull(instance, "instance").GetType())
		{
			_instance = instance;
		}

		public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (_activated)
			{
				throw new InvalidOperationException(ProvidedInstanceActivatorResources.InstanceAlreadyActivated);
			}
			_activated = true;
			return _instance;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _disposeInstance && _instance is IDisposable && !_activated)
			{
				((IDisposable)_instance).Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
