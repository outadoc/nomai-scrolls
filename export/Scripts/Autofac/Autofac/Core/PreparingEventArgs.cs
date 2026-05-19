using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core
{
	public class PreparingEventArgs : EventArgs
	{
		private readonly IComponentContext _context;

		private readonly IComponentRegistration _component;

		private IEnumerable<Parameter> _parameters;

		public IComponentContext Context => _context;

		public IComponentRegistration Component => _component;

		public IEnumerable<Parameter> Parameters
		{
			get
			{
				return _parameters;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				_parameters = value;
			}
		}

		public PreparingEventArgs(IComponentContext context, IComponentRegistration component, IEnumerable<Parameter> parameters)
		{
			_context = Enforce.ArgumentNotNull(context, "context");
			_component = Enforce.ArgumentNotNull(component, "component");
			_parameters = Enforce.ArgumentNotNull(parameters, "parameters");
		}
	}
}
