using System;
using System.Linq;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
	internal class AutowiringPropertyInjector
	{
		public static void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			Type type = instance.GetType();
			foreach (PropertyInfo item in from pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where pi.CanWrite
				select pi)
			{
				Type propertyType = item.PropertyType;
				if ((!propertyType.IsValueType || propertyType.IsEnum) && (!propertyType.IsArray || !propertyType.GetElementType().IsValueType) && (!propertyType.IsGenericEnumerableInterfaceType() || !propertyType.GetGenericArguments()[0].IsValueType) && item.GetIndexParameters().Length == 0 && context.IsRegistered(propertyType))
				{
					MethodInfo[] accessors = item.GetAccessors(nonPublic: false);
					if ((accessors.Length != 1 || (object)accessors[0].ReturnType == typeof(void)) && (overrideSetValues || accessors.Length != 2 || item.GetValue(instance, null) == null))
					{
						object value = context.Resolve(propertyType);
						item.SetValue(instance, value, null);
					}
				}
			}
		}
	}
}
