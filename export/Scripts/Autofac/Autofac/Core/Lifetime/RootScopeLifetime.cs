using System;

namespace Autofac.Core.Lifetime
{
	public class RootScopeLifetime : IComponentLifetime
	{
		public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
		{
			if (mostNestedVisibleScope == null)
			{
				throw new ArgumentNullException("mostNestedVisibleScope");
			}
			return mostNestedVisibleScope.RootLifetimeScope;
		}
	}
}
