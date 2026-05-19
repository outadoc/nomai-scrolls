using System;
using System.Globalization;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
	public class OpenGenericDecoratorActivatorData : ReflectionActivatorData
	{
		private readonly IServiceWithType _fromService;

		public IServiceWithType FromService => _fromService;

		public OpenGenericDecoratorActivatorData(Type implementer, IServiceWithType fromService)
			: base(implementer)
		{
			if (fromService == null)
			{
				throw new ArgumentNullException("fromService");
			}
			if (!fromService.ServiceType.IsGenericTypeDefinition)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorActivatorDataResources.DecoratedServiceIsNotOpenGeneric, new object[1] { fromService }));
			}
			_fromService = fromService;
		}
	}
}
