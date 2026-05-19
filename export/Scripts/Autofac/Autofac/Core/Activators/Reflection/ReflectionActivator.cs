using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
	public class ReflectionActivator : InstanceActivator, IInstanceActivator, IDisposable
	{
		private readonly Type _implementationType;

		private readonly IConstructorSelector _constructorSelector;

		private readonly IConstructorFinder _constructorFinder;

		private readonly IEnumerable<Parameter> _configuredParameters;

		private readonly IEnumerable<Parameter> _configuredProperties;

		private readonly IEnumerable<Parameter> _defaultParameters;

		public IConstructorFinder ConstructorFinder => _constructorFinder;

		public IConstructorSelector ConstructorSelector => _constructorSelector;

		public ReflectionActivator(Type implementationType, IConstructorFinder constructorFinder, IConstructorSelector constructorSelector, IEnumerable<Parameter> configuredParameters, IEnumerable<Parameter> configuredProperties)
			: base(Enforce.ArgumentNotNull(implementationType, "implementationType"))
		{
			_implementationType = implementationType;
			_constructorFinder = Enforce.ArgumentNotNull(constructorFinder, "constructorFinder");
			_constructorSelector = Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");
			_configuredParameters = Enforce.ArgumentNotNull(configuredParameters, "configuredParameters");
			_configuredProperties = Enforce.ArgumentNotNull(configuredProperties, "configuredProperties");
			_defaultParameters = _configuredParameters.Concat(new Parameter[2]
			{
				new AutowiringParameter(),
				new DefaultValueParameter()
			});
		}

		public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			ConstructorInfo[] array = _constructorFinder.FindConstructors(_implementationType);
			if (array.Length == 0)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ReflectionActivatorResources.NoConstructorsAvailable, new object[2] { _implementationType, _constructorFinder }));
			}
			IEnumerable<ConstructorParameterBinding> constructorBindings = GetConstructorBindings(context, parameters, array);
			ConstructorParameterBinding[] array2 = constructorBindings.Where((ConstructorParameterBinding cb) => cb.CanInstantiate).ToArray();
			if (array2.Length == 0)
			{
				throw new DependencyResolutionException(GetBindingFailureMessage(constructorBindings));
			}
			ConstructorParameterBinding constructorParameterBinding = _constructorSelector.SelectConstructorBinding(array2);
			object obj = constructorParameterBinding.Instantiate();
			InjectProperties(obj, context);
			return obj;
		}

		private string GetBindingFailureMessage(IEnumerable<ConstructorParameterBinding> constructorBindings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ConstructorParameterBinding item in constructorBindings.Where((ConstructorParameterBinding cb) => !cb.CanInstantiate))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(item.Description);
			}
			return string.Format(CultureInfo.CurrentCulture, ReflectionActivatorResources.NoConstructorsBindable, new object[3] { _constructorFinder, _implementationType, stringBuilder });
		}

		private IEnumerable<ConstructorParameterBinding> GetConstructorBindings(IComponentContext context, IEnumerable<Parameter> parameters, IEnumerable<ConstructorInfo> constructorInfo)
		{
			IEnumerable<Parameter> prioritisedParameters = parameters.Concat(_defaultParameters);
			return constructorInfo.Select((ConstructorInfo ci) => new ConstructorParameterBinding(ci, prioritisedParameters, context));
		}

		private void InjectProperties(object instance, IComponentContext context)
		{
			if (!_configuredProperties.Any())
			{
				return;
			}
			List<PropertyInfo> list = (from pi in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where pi.CanWrite
				select pi).ToList();
			foreach (Parameter configuredProperty in _configuredProperties)
			{
				foreach (PropertyInfo item in list)
				{
					MethodInfo setMethod = item.GetSetMethod();
					if ((object)setMethod != null && configuredProperty.CanSupplyValue(setMethod.GetParameters().First(), context, out var valueProvider))
					{
						list.Remove(item);
						item.SetValue(instance, valueProvider(), null);
						break;
					}
				}
			}
		}
	}
}
