using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
	public class DefaultConstructorFinder : IConstructorFinder
	{
		private readonly Func<Type, ConstructorInfo[]> _finder;

		public DefaultConstructorFinder()
			: this((Type type) => type.GetConstructors())
		{
		}

		public DefaultConstructorFinder(Func<Type, ConstructorInfo[]> finder)
		{
			if (finder == null)
			{
				throw new ArgumentNullException("finder");
			}
			_finder = finder;
		}

		public ConstructorInfo[] FindConstructors(Type targetType)
		{
			return _finder(targetType);
		}
	}
}
