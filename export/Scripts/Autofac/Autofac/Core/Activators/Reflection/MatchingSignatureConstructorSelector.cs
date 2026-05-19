using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
	public class MatchingSignatureConstructorSelector : IConstructorSelector
	{
		private readonly Type[] _signature;

		public MatchingSignatureConstructorSelector(params Type[] signature)
		{
			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}
			_signature = signature;
		}

		public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings)
		{
			if (constructorBindings == null)
			{
				throw new ArgumentNullException("constructorBindings");
			}
			ConstructorParameterBinding[] array = constructorBindings.Where((ConstructorParameterBinding b) => (from p in b.TargetConstructor.GetParameters()
				select p.ParameterType).SequenceEqual(_signature)).ToArray();
			if (array.Length == 1)
			{
				return array[0];
			}
			if (!constructorBindings.Any())
			{
				throw new ArgumentException(MatchingSignatureConstructorSelectorResources.AtLeastOneBindingRequired);
			}
			string name = constructorBindings.First().TargetConstructor.DeclaringType.Name;
			string text = string.Join(", ", _signature.Select((Type t) => t.Name).ToArray());
			if (array.Length == 0)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.RequiredConstructorNotAvailable, new object[2] { name, text }));
			}
			throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.TooManyConstructorsMatch, new object[1] { text }));
		}
	}
}
