using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Metadata
{
	internal static class MetadataViewProvider
	{
		private static readonly MethodInfo GetMetadataValueMethod = typeof(MetadataViewProvider).GetMethod("GetMetadataValue", BindingFlags.Static | BindingFlags.NonPublic);

		public static Func<IDictionary<string, object>, TMetadata> GetMetadataViewProvider<TMetadata>()
		{
			if ((object)typeof(TMetadata) == typeof(IDictionary<string, object>))
			{
				return (IDictionary<string, object> m) => (TMetadata)m;
			}
			if (!typeof(TMetadata).IsClass)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, new object[1] { typeof(TMetadata).Name }));
			}
			Type typeFromHandle = typeof(TMetadata);
			ConstructorInfo constructorInfo = typeFromHandle.GetConstructors().SingleOrDefault(delegate(ConstructorInfo ci)
			{
				ParameterInfo[] parameters = ci.GetParameters();
				return ci.IsPublic && parameters.Length == 1 && (object)parameters[0].ParameterType == typeof(IDictionary<string, object>);
			});
			if ((object)constructorInfo != null)
			{
				ParameterExpression parameterExpression = Expression.Parameter(typeof(IDictionary<string, object>), "metadata");
				return Expression.Lambda<Func<IDictionary<string, object>, TMetadata>>(Expression.New(constructorInfo, parameterExpression), new ParameterExpression[1] { parameterExpression }).Compile();
			}
			ConstructorInfo constructorInfo2 = typeFromHandle.GetConstructors().SingleOrDefault((ConstructorInfo ci) => ci.IsPublic && ci.GetParameters().Length == 0);
			if ((object)constructorInfo2 != null)
			{
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(IDictionary<string, object>), "metadata");
				ParameterExpression parameterExpression3 = Expression.Variable(typeof(TMetadata), "result");
				BinaryExpression item = Expression.Assign(parameterExpression3, Expression.New(constructorInfo2));
				List<Expression> list = new List<Expression>();
				list.Add(item);
				List<Expression> list2 = list;
				foreach (PropertyInfo item3 in from prop in typeof(TMetadata).GetProperties()
					where (object)prop.GetGetMethod(nonPublic: false) != null && !prop.GetGetMethod().IsStatic && (object)prop.GetSetMethod(nonPublic: false) != null && !prop.GetSetMethod().IsStatic
					select prop)
				{
					ConstantExpression arg = Expression.Constant(ReflectionExtensions.GetCustomAttribute<DefaultValueAttribute>(item3, inherit: false), typeof(DefaultValueAttribute));
					ConstantExpression arg2 = Expression.Constant(item3.Name, typeof(string));
					MethodInfo method = GetMetadataValueMethod.MakeGenericMethod(item3.PropertyType);
					BinaryExpression item2 = Expression.Assign(Expression.Property(parameterExpression3, item3), Expression.Call(null, method, parameterExpression2, arg2, arg));
					list2.Add(item2);
				}
				list2.Add(parameterExpression3);
				return Expression.Lambda<Func<IDictionary<string, object>, TMetadata>>(Expression.Block(new ParameterExpression[1] { parameterExpression3 }, list2), new ParameterExpression[1] { parameterExpression2 }).Compile();
			}
			throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.InvalidViewImplementation, new object[1] { typeof(TMetadata).Name }));
		}

		private static TValue GetMetadataValue<TValue>(IDictionary<string, object> metadata, string name, DefaultValueAttribute defaultValue)
		{
			if (metadata.TryGetValue(name, out var value))
			{
				return (TValue)value;
			}
			if (defaultValue != null)
			{
				return (TValue)defaultValue.Value;
			}
			throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MetadataViewProviderResources.MissingMetadata, new object[1] { name }));
		}
	}
}
