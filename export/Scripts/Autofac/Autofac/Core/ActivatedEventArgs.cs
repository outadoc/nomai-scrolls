using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core
{
	public class ActivatedEventArgs<T> : EventArgs, IActivatedEventArgs<T>
	{
		private readonly IComponentContext _context;

		private readonly IComponentRegistration _component;

		private readonly IEnumerable<Parameter> _parameters;

		private readonly T _instance;

		public IComponentContext Context => _context;

		public IComponentRegistration Component => _component;

		public IEnumerable<Parameter> Parameters => _parameters;

		public T Instance => _instance;

		public ActivatedEventArgs(IComponentContext context, IComponentRegistration component, IEnumerable<Parameter> parameters, T instance)
		{
			_context = Enforce.ArgumentNotNull(context, "context");
			_component = Enforce.ArgumentNotNull(component, "component");
			_parameters = Enforce.ArgumentNotNull(parameters, "parameters");
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			_instance = instance;
		}
	}
}
