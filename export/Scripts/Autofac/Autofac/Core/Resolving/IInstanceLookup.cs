using System;
using System.Collections.Generic;

namespace Autofac.Core.Resolving
{
	public interface IInstanceLookup
	{
		IComponentRegistration ComponentRegistration { get; }

		ILifetimeScope ActivationScope { get; }

		IEnumerable<Parameter> Parameters { get; }

		event EventHandler<InstanceLookupEndingEventArgs> InstanceLookupEnding;

		event EventHandler<InstanceLookupCompletionBeginningEventArgs> CompletionBeginning;

		event EventHandler<InstanceLookupCompletionEndingEventArgs> CompletionEnding;
	}
}
