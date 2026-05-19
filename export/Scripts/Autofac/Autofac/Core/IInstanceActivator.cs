using System;
using System.Collections.Generic;

namespace Autofac.Core
{
	public interface IInstanceActivator : IDisposable
	{
		Type LimitType { get; }

		object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters);
	}
}
