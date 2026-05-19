using System.Reflection;
using Autofac.Util;

namespace Autofac.Core
{
	public class NamedPropertyParameter : ConstantParameter
	{
		public string Name { get; private set; }

		public NamedPropertyParameter(string name, object value)
			: base(value, (ParameterInfo pi) => pi.TryGetDeclaringProperty(out var prop) && prop.Name == name)
		{
			Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
		}
	}
}
