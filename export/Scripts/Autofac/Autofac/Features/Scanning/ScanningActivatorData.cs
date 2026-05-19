using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.Scanning
{
	public class ScanningActivatorData : ReflectionActivatorData
	{
		private readonly ICollection<Func<Type, bool>> _filters = new List<Func<Type, bool>>();

		private readonly ICollection<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>> _configurationActions = new List<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>>();

		private readonly ICollection<Action<IComponentRegistry>> _postScanningCallbacks = new List<Action<IComponentRegistry>>();

		public ICollection<Func<Type, bool>> Filters => _filters;

		public ICollection<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>> ConfigurationActions => _configurationActions;

		public ICollection<Action<IComponentRegistry>> PostScanningCallbacks => _postScanningCallbacks;

		public ScanningActivatorData()
			: base(typeof(object))
		{
		}
	}
}
