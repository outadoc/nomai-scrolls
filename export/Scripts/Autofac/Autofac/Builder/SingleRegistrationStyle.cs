using System;
using System.Collections.Generic;
using Autofac.Core;

namespace Autofac.Builder
{
	public class SingleRegistrationStyle
	{
		private Guid _id = Guid.NewGuid();

		private readonly ICollection<EventHandler<ComponentRegisteredEventArgs>> _registeredHandlers = new List<EventHandler<ComponentRegisteredEventArgs>>();

		private bool _preserveDefaults;

		public Guid Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public ICollection<EventHandler<ComponentRegisteredEventArgs>> RegisteredHandlers => _registeredHandlers;

		public bool PreserveDefaults
		{
			get
			{
				return _preserveDefaults;
			}
			set
			{
				_preserveDefaults = value;
			}
		}

		public IComponentRegistration Target { get; set; }
	}
}
