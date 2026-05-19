using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IActivatingEventArgs<out T>
	{
		IComponentContext Context { get; }

		IComponentRegistration Component { get; }

		T Instance { get; }

		IEnumerable<Parameter> Parameters { get; }

		void ReplaceInstance(object instance);
	}
}
