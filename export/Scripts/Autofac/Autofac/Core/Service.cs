using System;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core
{
	public abstract class Service
	{
		public abstract string Description { get; }

		public override string ToString()
		{
			return Description;
		}

		public static bool operator ==(Service left, Service right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(Service left, Service right)
		{
			return !(left == right);
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "This is an attempt to make Equals 'abstract' when it normally isn't.")]
		public override bool Equals(object obj)
		{
			throw new NotImplementedException(ServiceResources.MustOverrideEquals);
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "This is an attempt to make GetHashCode 'abstract' when it normally isn't.")]
		public override int GetHashCode()
		{
			throw new NotImplementedException(ServiceResources.MustOverrideGetHashCode);
		}
	}
}
