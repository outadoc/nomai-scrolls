using System;

namespace Autofac.Core.Resolving
{
	public class ResolveOperationBeginningEventArgs : EventArgs
	{
		private readonly IResolveOperation _resolveOperation;

		public IResolveOperation ResolveOperation => _resolveOperation;

		public ResolveOperationBeginningEventArgs(IResolveOperation resolveOperation)
		{
			_resolveOperation = resolveOperation;
		}
	}
}
