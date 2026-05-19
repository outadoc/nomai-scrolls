using System;
using System.Collections.Generic;
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
	public class LightweightAdapterActivatorData
	{
		private readonly Service _fromService;

		private readonly Func<IComponentContext, IEnumerable<Parameter>, object, object> _adapter;

		public Func<IComponentContext, IEnumerable<Parameter>, object, object> Adapter => _adapter;

		public Service FromService => _fromService;

		public LightweightAdapterActivatorData(Service fromService, Func<IComponentContext, IEnumerable<Parameter>, object, object> adapter)
		{
			_fromService = fromService;
			_adapter = adapter;
		}
	}
}
