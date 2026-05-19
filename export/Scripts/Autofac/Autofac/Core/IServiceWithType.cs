using System;

namespace Autofac.Core
{
	public interface IServiceWithType
	{
		Type ServiceType { get; }

		Service ChangeType(Type newType);
	}
}
