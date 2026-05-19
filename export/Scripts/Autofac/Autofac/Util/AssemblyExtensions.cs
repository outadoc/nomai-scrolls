using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
	public static class AssemblyExtensions
	{
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			if ((object)assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				return ex.Types.Where((Type t) => (object)t != null);
			}
		}
	}
}
