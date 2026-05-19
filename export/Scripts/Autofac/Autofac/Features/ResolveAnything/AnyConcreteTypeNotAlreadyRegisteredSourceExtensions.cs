using System;
using Autofac.Builder;

namespace Autofac.Features.ResolveAnything
{
	public static class AnyConcreteTypeNotAlreadyRegisteredSourceExtensions
	{
		public static AnyConcreteTypeNotAlreadyRegisteredSource WithRegistrationsAs(this AnyConcreteTypeNotAlreadyRegisteredSource source, Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>> configurationAction)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			source.RegistrationConfiguration = configurationAction;
			return source;
		}
	}
}
