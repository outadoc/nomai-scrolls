using System;

namespace Autofac.Core.Resolving
{
	public class InstanceLookupCompletionBeginningEventArgs : EventArgs
	{
		private readonly IInstanceLookup _instanceLookup;

		public IInstanceLookup InstanceLookup => _instanceLookup;

		public InstanceLookupCompletionBeginningEventArgs(IInstanceLookup instanceLookup)
		{
			if (instanceLookup == null)
			{
				throw new ArgumentNullException("instanceLookup");
			}
			_instanceLookup = instanceLookup;
		}
	}
}
