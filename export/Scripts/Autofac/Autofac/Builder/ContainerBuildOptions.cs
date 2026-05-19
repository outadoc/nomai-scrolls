using System;

namespace Autofac.Builder
{
	[Flags]
	public enum ContainerBuildOptions
	{
		None = 0,
		ExcludeDefaultModules = 2,
		IgnoreStartableComponents = 4
	}
}
