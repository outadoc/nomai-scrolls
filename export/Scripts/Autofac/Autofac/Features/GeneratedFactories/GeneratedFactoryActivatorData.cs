using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
	public class GeneratedFactoryActivatorData : IConcreteActivatorData
	{
		private ParameterMapping _parameterMapping;

		private Type _delegateType;

		private Service _productService;

		public ParameterMapping ParameterMapping
		{
			get
			{
				return _parameterMapping;
			}
			set
			{
				_parameterMapping = value;
			}
		}

		public IInstanceActivator Activator
		{
			get
			{
				FactoryGenerator factory = new FactoryGenerator(_delegateType, _productService, ParameterMapping);
				return new DelegateActivator(_delegateType, (IComponentContext c, IEnumerable<Parameter> p) => factory.GenerateFactory(c, p));
			}
		}

		public GeneratedFactoryActivatorData(Type delegateType, Service productService)
		{
			_delegateType = Enforce.ArgumentNotNull(delegateType, "delegateType");
			_productService = Enforce.ArgumentNotNull(productService, "productService");
		}
	}
}
