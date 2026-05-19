using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;

namespace Autofac.Builder
{
	public class ReflectionActivatorData
	{
		private Type _implementer;

		private IConstructorFinder _constructorFinder = new DefaultConstructorFinder();

		private IConstructorSelector _constructorSelector = new MostParametersConstructorSelector();

		private readonly IList<Parameter> _configuredParameters = new List<Parameter>();

		private readonly IList<Parameter> _configuredProperties = new List<Parameter>();

		public Type ImplementationType
		{
			get
			{
				return _implementer;
			}
			set
			{
				_implementer = Enforce.ArgumentNotNull(value, "value");
			}
		}

		public IConstructorFinder ConstructorFinder
		{
			get
			{
				return _constructorFinder;
			}
			set
			{
				_constructorFinder = Enforce.ArgumentNotNull(value, "value");
			}
		}

		public IConstructorSelector ConstructorSelector
		{
			get
			{
				return _constructorSelector;
			}
			set
			{
				_constructorSelector = Enforce.ArgumentNotNull(value, "value");
			}
		}

		public IList<Parameter> ConfiguredParameters => _configuredParameters;

		public IList<Parameter> ConfiguredProperties => _configuredProperties;

		public ReflectionActivatorData(Type implementer)
		{
			ImplementationType = implementer;
		}
	}
}
