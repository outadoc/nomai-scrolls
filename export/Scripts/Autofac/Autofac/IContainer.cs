using System;

namespace Autofac
{
	public interface IContainer : ILifetimeScope, IComponentContext, IDisposable
	{
	}
}
