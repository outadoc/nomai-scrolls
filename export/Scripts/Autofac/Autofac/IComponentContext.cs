using System.Collections.Generic;
using Autofac.Core;

namespace Autofac
{
	public interface IComponentContext
	{
		IComponentRegistry ComponentRegistry { get; }

		object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters);
	}
}
