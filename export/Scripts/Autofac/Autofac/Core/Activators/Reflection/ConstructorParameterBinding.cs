using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
	public class ConstructorParameterBinding
	{
		private readonly ConstructorInfo _ci;

		private readonly Func<object>[] _valueRetrievers;

		private readonly bool _canInstantiate;

		private static readonly SafeDictionary<ConstructorInfo, Func<object[], object>> _constructorInvokers = new SafeDictionary<ConstructorInfo, Func<object[], object>>();

		private readonly ParameterInfo _firstNonBindableParameter;

		public ConstructorInfo TargetConstructor => _ci;

		public bool CanInstantiate => _canInstantiate;

		public string Description
		{
			get
			{
				if (!CanInstantiate)
				{
					return string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.NonBindableConstructor, new object[2] { _ci, _firstNonBindableParameter });
				}
				return string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.BoundConstructor, new object[1] { _ci });
			}
		}

		public ConstructorParameterBinding(ConstructorInfo ci, IEnumerable<Parameter> availableParameters, IComponentContext context)
		{
			_canInstantiate = true;
			_ci = Enforce.ArgumentNotNull(ci, "ci");
			if (availableParameters == null)
			{
				throw new ArgumentNullException("availableParameters");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			ParameterInfo[] parameters = ci.GetParameters();
			_valueRetrievers = new Func<object>[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				bool flag = false;
				foreach (Parameter availableParameter in availableParameters)
				{
					if (availableParameter.CanSupplyValue(parameterInfo, context, out var valueProvider))
					{
						_valueRetrievers[i] = valueProvider;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_canInstantiate = false;
					_firstNonBindableParameter = parameterInfo;
					break;
				}
			}
		}

		public object Instantiate()
		{
			if (!CanInstantiate)
			{
				throw new InvalidOperationException(ConstructorParameterBindingResources.CannotInstantitate);
			}
			object[] array = new object[_valueRetrievers.Length];
			for (int i = 0; i < _valueRetrievers.Length; i++)
			{
				array[i] = _valueRetrievers[i]();
			}
			if (!_constructorInvokers.TryGetValue(TargetConstructor, out var value))
			{
				value = GetConstructorInvoker(TargetConstructor);
				_constructorInvokers[TargetConstructor] = value;
			}
			try
			{
				return value(array);
			}
			catch (TargetInvocationException ex)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, new object[2]
				{
					TargetConstructor,
					TargetConstructor.DeclaringType.Name
				}), ex.InnerException);
			}
			catch (Exception innerException)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, new object[2]
				{
					TargetConstructor,
					TargetConstructor.DeclaringType.Name
				}), innerException);
			}
		}

		public override string ToString()
		{
			return Description;
		}

		private static Func<object[], object> GetConstructorInvoker(ConstructorInfo constructorInfo)
		{
			ParameterInfo[] parameters = constructorInfo.GetParameters();
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
			Expression[] array = new Expression[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ConstantExpression index = Expression.Constant(i);
				Type parameterType = parameters[i].ParameterType;
				BinaryExpression binaryExpression = Expression.ArrayIndex(parameterExpression, index);
				UnaryExpression ifFalse = (UnaryExpression)(array[i] = Expression.Convert(binaryExpression, parameterType));
				if (parameterType.IsValueType)
				{
					BinaryExpression test = Expression.Equal(binaryExpression, Expression.Constant(null));
					array[i] = Expression.Condition(test, Expression.Default(parameterType), ifFalse);
				}
			}
			NewExpression body = Expression.New(constructorInfo, array);
			Expression<Func<object[], object>> expression = Expression.Lambda<Func<object[], object>>(body, new ParameterExpression[1] { parameterExpression });
			return expression.Compile();
		}
	}
}
