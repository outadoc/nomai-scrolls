using System;

namespace Autofac.Core.Resolving
{
	public class InstanceLookupBeginningEventArgs : EventArgs
	{
		private readonly IInstanceLookup _instanceLookup;

		public IInstanceLookup InstanceLookup => _instanceLookup;

		public InstanceLookupBeginningEventArgs(IInstanceLookup instanceLookup)
		{
			if (instanceLookup == null)
			{
				throw new ArgumentNullException("instanceLookup");
			}
			_instanceLookup = instanceLookup;
		}
	}
}
