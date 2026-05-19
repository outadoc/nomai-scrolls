using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core
{
	public class ActivatingEventArgs<T> : EventArgs, IActivatingEventArgs<T>
	{
		private readonly IComponentContext _context;

		private readonly IComponentRegistration _component;

		private T _instance;

		private readonly IEnumerable<Parameter> _parameters;

		public IComponentContext Context => _context;

		public IComponentRegistration Component => _component;

		public T Instance
		{
			get
			{
				return _instance;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				_instance = value;
			}
		}

		public IEnumerable<Parameter> Parameters => _parameters;

		public ActivatingEventArgs(IComponentContext context, IComponentRegistration component, IEnumerable<Parameter> parameters, T instance)
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

		public void ReplaceInstance(object instance)
		{
			Instance = (T)instance;
		}
	}
}
