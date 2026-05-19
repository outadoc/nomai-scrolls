using System.Reflection;
using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
	public class NamedParameter : ConstantParameter
	{
		public string Name { get; private set; }

		public NamedParameter(string name, object value)
			: base(value, (ParameterInfo pi) => pi.Name == name)
		{
			Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
		}
	}
}
