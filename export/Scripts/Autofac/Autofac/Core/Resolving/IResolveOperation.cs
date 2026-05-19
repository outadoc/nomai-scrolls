using System;
using System.Collections.Generic;

namespace Autofac.Core.Resolving
{
	public interface IResolveOperation
	{
		event EventHandler<ResolveOperationEndingEventArgs> CurrentOperationEnding;

		event EventHandler<InstanceLookupBeginningEventArgs> InstanceLookupBeginning;

		object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, IComponentRegistration registration, IEnumerable<Parameter> parameters);
	}
}
