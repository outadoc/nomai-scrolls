using System;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Activators.Reflection
{
	public class MostParametersConstructorSelector : IConstructorSelector
	{
		public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings)
		{
			if (constructorBindings == null)
			{
				throw new ArgumentNullException("constructorBindings");
			}
			if (constructorBindings.Length == 0)
			{
				throw new ArgumentOutOfRangeException("constructorBindings");
			}
			if (constructorBindings.Length == 1)
			{
				return constructorBindings[0];
			}
			var source = constructorBindings.Select((ConstructorParameterBinding binding) => new
			{
				Binding = binding,
				ConstructorParameterLength = binding.TargetConstructor.GetParameters().Length
			});
			int maxLength = source.Max(binding => binding.ConstructorParameterLength);
			ConstructorParameterBinding[] array = (from ctor in source
				where ctor.ConstructorParameterLength == maxLength
				select ctor.Binding).ToArray();
			if (array.Length == 1)
			{
				return array[0];
			}
			throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MostParametersConstructorSelectorResources.UnableToChooseFromMultipleConstructors, new object[2]
			{
				maxLength,
				array[0].TargetConstructor.DeclaringType
			}));
		}
	}
}
