using System;

namespace Autofac.Core.Resolving
{
	public class ResolveOperationEndingEventArgs : EventArgs
	{
		private readonly IResolveOperation _resolveOperation;

		private readonly Exception _exception;

		public Exception Exception => _exception;

		public IResolveOperation ResolveOperation => _resolveOperation;

		public ResolveOperationEndingEventArgs(IResolveOperation resolveOperation, Exception exception = null)
		{
			_resolveOperation = resolveOperation;
			_exception = exception;
		}
	}
}
