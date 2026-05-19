using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac.Util;

namespace Autofac.Builder
{
	public class MetadataConfiguration<TMetadata>
	{
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

		internal IEnumerable<KeyValuePair<string, object>> Properties => _properties;

		public MetadataConfiguration<TMetadata> For<TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
		{
			if (propertyAccessor == null)
			{
				throw new ArgumentNullException("propertyAccessor");
			}
			string name = ReflectionExtensions.GetProperty(propertyAccessor).Name;
			_properties.Add(name, value);
			return this;
		}
	}
}
