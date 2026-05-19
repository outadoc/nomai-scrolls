using System;

namespace Autofac
{
	[Flags]
	public enum PropertyWiringOptions
	{
		None = 0,
		AllowCircularDependencies = 1,
		PreserveSetValues = 2
	}
}
