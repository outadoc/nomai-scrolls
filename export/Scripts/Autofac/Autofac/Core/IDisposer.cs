using System;

namespace Autofac.Core
{
	public interface IDisposer : IDisposable
	{
		void AddInstanceForDisposal(IDisposable instance);
	}
}
