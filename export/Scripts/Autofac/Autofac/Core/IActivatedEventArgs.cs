using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IActivatedEventArgs<out T>
	{
		IComponentContext Context { get; }

		IComponentRegistration Component { get; }

		IEnumerable<Parameter> Parameters { get; }

		T Instance { get; }
	}
}
