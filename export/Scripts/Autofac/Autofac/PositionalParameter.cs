using System;
using System.Reflection;
using Autofac.Core;

namespace Autofac
{
	public class PositionalParameter : ConstantParameter
	{
		public int Position { get; private set; }

		public PositionalParameter(int position, object value)
			: base(value, (ParameterInfo pi) => pi.Position == position && pi.Member is ConstructorInfo)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			Position = position;
		}
	}
}
