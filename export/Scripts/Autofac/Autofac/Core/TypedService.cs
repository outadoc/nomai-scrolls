using System;
using Autofac.Util;

namespace Autofac.Core
{
	public sealed class TypedService : Service, IServiceWithType, IEquatable<TypedService>
	{
		public Type ServiceType { get; private set; }

		public override string Description => ServiceType.FullName;

		public TypedService(Type serviceType)
		{
			ServiceType = Enforce.ArgumentNotNull(serviceType, "serviceType");
		}

		public bool Equals(TypedService other)
		{
			if (other == null)
			{
				return false;
			}
			return (object)ServiceType == other.ServiceType;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TypedService);
		}

		public override int GetHashCode()
		{
			return ServiceType.GetHashCode();
		}

		public Service ChangeType(Type newType)
		{
			if ((object)newType == null)
			{
				throw new ArgumentNullException("newType");
			}
			return new TypedService(newType);
		}
	}
}
