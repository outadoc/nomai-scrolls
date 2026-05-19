namespace Autofac.Core
{
	public interface IComponentLifetime
	{
		ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope);
	}
}
