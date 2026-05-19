using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
	internal static class Enforce
	{
		public static T ArgumentNotNull<T>([ValidatedNotNull] T value, string name) where T : class
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (value == null)
			{
				throw new ArgumentNullException(name);
			}
			return value;
		}

		public static T ArgumentElementNotNull<T>(T value, string name) where T : class, IEnumerable
		{
			if (value == null)
			{
				throw new ArgumentNullException(name);
			}
			if (value.Cast<object>().Any((object v) => v == null))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.ElementCannotBeNull, new object[1] { name }));
			}
			return value;
		}

		public static T NotNull<T>([ValidatedNotNull] T value) where T : class
		{
			if (value == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeNull, new object[1] { typeof(T).FullName }));
			}
			return value;
		}

		public static string ArgumentNotNullOrEmpty([ValidatedNotNull] string value, string description)
		{
			if (description == null)
			{
				throw new ArgumentNullException("description");
			}
			if (value == null)
			{
				throw new ArgumentNullException(description);
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeEmpty, new object[1] { description }));
			}
			return value;
		}

		public static void ArgumentTypeIsFunction(Type delegateType)
		{
			if ((object)delegateType == null)
			{
				throw new ArgumentNullException("delegateType");
			}
			MethodInfo method = delegateType.GetMethod("Invoke");
			if ((object)method == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.NotDelegate, new object[1] { delegateType }));
			}
			if ((object)method.ReturnType == typeof(void))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.DelegateReturnsVoid, new object[1] { delegateType }));
			}
		}
	}
}
