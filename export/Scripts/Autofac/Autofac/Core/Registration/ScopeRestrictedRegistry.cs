using System;
using Autofac.Core.Lifetime;

namespace Autofac.Core.Registration
{
	internal class ScopeRestrictedRegistry : ComponentRegistry
	{
		private readonly IComponentLifetime _restrictedRootScopeLifetime;

		public ScopeRestrictedRegistry(object scopeTag)
		{
			_restrictedRootScopeLifetime = new MatchingScopeLifetime(scopeTag);
		}

		public override void Register(IComponentRegistration registration, bool preserveDefaults)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			IComponentRegistration registration2 = registration;
			if (registration.Lifetime is RootScopeLifetime)
			{
				registration2 = new ComponentRegistrationLifetimeDecorator(registration, _restrictedRootScopeLifetime);
			}
			base.Register(registration2, preserveDefaults);
		}
	}
}
