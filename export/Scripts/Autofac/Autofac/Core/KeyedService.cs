using System;
using Autofac.Util;

namespace Autofac.Core
{
	public sealed class KeyedService : Service, IServiceWithType, IEquatable<KeyedService>
	{
		private readonly object _serviceKey;

		private readonly Type _serviceType;

		public object ServiceKey => _serviceKey;

		public Type ServiceType => _serviceType;

		public override string Description => string.Concat(ServiceKey, " (", ServiceType.FullName, ")");

		public KeyedService(object serviceKey, Type serviceType)
		{
			_serviceKey = Enforce.ArgumentNotNull(serviceKey, "serviceKey");
			_serviceType = Enforce.ArgumentNotNull(serviceType, "serviceType");
		}

		public bool Equals(KeyedService other)
		{
			if (other == null)
			{
				return false;
			}
			if (ServiceKey.Equals(other.ServiceKey))
			{
				return (object)ServiceType == other.ServiceType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as KeyedService);
		}

		public override int GetHashCode()
		{
			return ServiceKey.GetHashCode() ^ ServiceType.GetHashCode();
		}

		public Service ChangeType(Type newType)
		{
			if ((object)newType == null)
			{
				throw new ArgumentNullException("newType");
			}
			return new KeyedService(ServiceKey, newType);
		}
	}
}
