using System;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Builder
{
	public class ConcreteReflectionActivatorData : ReflectionActivatorData, IConcreteActivatorData
	{
		public IInstanceActivator Activator => new ReflectionActivator(base.ImplementationType, base.ConstructorFinder, base.ConstructorSelector, base.ConfiguredParameters, base.ConfiguredProperties);

		public ConcreteReflectionActivatorData(Type implementer)
			: base(implementer)
		{
		}
	}
}
