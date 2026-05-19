using System;

namespace Autofac.Core.Resolving
{
	public class InstanceLookupEndingEventArgs : EventArgs
	{
		private readonly IInstanceLookup _instanceLookup;

		private readonly bool _newInstanceActivated;

		public bool NewInstanceActivated => _newInstanceActivated;

		public IInstanceLookup InstanceLookup => _instanceLookup;

		public InstanceLookupEndingEventArgs(IInstanceLookup instanceLookup, bool newInstanceActivated)
		{
			if (instanceLookup == null)
			{
				throw new ArgumentNullException("instanceLookup");
			}
			_instanceLookup = instanceLookup;
			_newInstanceActivated = newInstanceActivated;
		}
	}
}
