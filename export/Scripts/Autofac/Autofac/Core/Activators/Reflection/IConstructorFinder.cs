using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
	public interface IConstructorFinder
	{
		ConstructorInfo[] FindConstructors(Type targetType);
	}
}
