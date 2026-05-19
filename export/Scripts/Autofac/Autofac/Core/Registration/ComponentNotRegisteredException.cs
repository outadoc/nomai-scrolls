using System;
using System.Globalization;
using Autofac.Util;

namespace Autofac.Core.Registration
{
	public class ComponentNotRegisteredException : DependencyResolutionException
	{
		public ComponentNotRegisteredException(Service service)
			: base(string.Format(CultureInfo.CurrentCulture, ComponentNotRegisteredExceptionResources.Message, new object[1] { Enforce.ArgumentNotNull(service, "service") }))
		{
		}

		public ComponentNotRegisteredException(Service service, Exception innerException)
			: base(string.Format(CultureInfo.CurrentCulture, ComponentNotRegisteredExceptionResources.Message, new object[1] { Enforce.ArgumentNotNull(service, "service") }), innerException)
		{
		}
	}
}
