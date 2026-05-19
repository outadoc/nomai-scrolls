using System.Collections.Generic;

namespace Autofac.Features.Metadata
{
	public class Meta<T, TMetadata>
	{
		private readonly T _value;

		private readonly TMetadata _metadata;

		public T Value => _value;

		public TMetadata Metadata => _metadata;

		public Meta(T value, TMetadata metadata)
		{
			_value = value;
			_metadata = metadata;
		}
	}
	public class Meta<T>
	{
		private readonly T _value;

		private readonly IDictionary<string, object> _metadata;

		public T Value => _value;

		public IDictionary<string, object> Metadata => _metadata;

		public Meta(T value, IDictionary<string, object> metadata)
		{
			_value = value;
			_metadata = metadata;
		}
	}
}
