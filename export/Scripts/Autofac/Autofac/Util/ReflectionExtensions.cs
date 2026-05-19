using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Autofac.Util
{
	internal static class ReflectionExtensions
	{
		public static bool TryGetDeclaringProperty(this ParameterInfo pi, out PropertyInfo prop)
		{
			if (pi.Member is MethodInfo methodInfo && methodInfo.IsSpecialName && methodInfo.Name.StartsWith("set_", StringComparison.Ordinal) && (object)methodInfo.DeclaringType != null)
			{
				prop = methodInfo.DeclaringType.GetProperty(methodInfo.Name.Substring(4));
				return true;
			}
			prop = null;
			return false;
		}

		public static PropertyInfo GetProperty<TDeclaring, TProperty>(Expression<Func<TDeclaring, TProperty>> propertyAccessor)
		{
			if (propertyAccessor == null)
			{
				throw new ArgumentNullException("propertyAccessor");
			}
			if (!(propertyAccessor.Body is MemberExpression memberExpression) || !(memberExpression.Member is PropertyInfo))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ReflectionExtensionsResources.ExpressionNotPropertyAccessor, new object[1] { propertyAccessor }));
			}
			return (PropertyInfo)memberExpression.Member;
		}

		public static MethodInfo GetMethod<TDeclaring>(Expression<Action<TDeclaring>> methodCallExpression)
		{
			if (methodCallExpression == null)
			{
				throw new ArgumentNullException("methodCallExpression");
			}
			if (!(methodCallExpression.Body is MethodCallExpression methodCallExpression2))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ReflectionExtensionsResources.ExpressionNotMethodCall, new object[1] { methodCallExpression }));
			}
			return methodCallExpression2.Method;
		}

		public static ConstructorInfo GetConstructor<TDeclaring>(Expression<Func<TDeclaring>> constructorCallExpression)
		{
			if (constructorCallExpression == null)
			{
				throw new ArgumentNullException("constructorCallExpression");
			}
			if (!(constructorCallExpression.Body is NewExpression newExpression))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ReflectionExtensionsResources.ExpressionNotConstructorCall, new object[1] { constructorCallExpression }));
			}
			return newExpression.Constructor;
		}

		public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
		{
			return (T)Attribute.GetCustomAttribute(element, typeof(T), inherit);
		}
	}
}
