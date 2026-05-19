using System;

namespace Autofac.Core.Resolving
{
	public class InstanceLookupCompletionEndingEventArgs : EventArgs
	{
		private readonly IInstanceLookup _instanceLookup;

		public IInstanceLookup InstanceLookup => _instanceLookup;

		public InstanceLookupCompletionEndingEventArgs(IInstanceLookup instanceLookup)
		{
			if (instanceLookup == null)
			{
				throw new ArgumentNullException("instanceLookup");
			}
			_instanceLookup = instanceLookup;
		}
	}
}
