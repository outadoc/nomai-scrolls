using System;
using Autofac.Util;

namespace Autofac.Core
{
	public class ComponentRegisteredEventArgs : EventArgs
	{
		private readonly IComponentRegistry _componentRegistry;

		private readonly IComponentRegistration _componentRegistration;

		public IComponentRegistry ComponentRegistry => _componentRegistry;

		public IComponentRegistration ComponentRegistration => _componentRegistration;

		public ComponentRegisteredEventArgs(IComponentRegistry registry, IComponentRegistration componentRegistration)
		{
			_componentRegistry = Enforce.ArgumentNotNull(registry, "registry");
			_componentRegistration = Enforce.ArgumentNotNull(componentRegistration, "componentRegistration");
		}
	}
}
