using System;
using System.Collections.Generic;

namespace Autofac.Core.Resolving
{
	internal class InstanceLookup : IComponentContext, IInstanceLookup
	{
		private readonly IEnumerable<Parameter> _parameters;

		private readonly IComponentRegistration _componentRegistration;

		private readonly IResolveOperation _context;

		private readonly ISharingLifetimeScope _activationScope;

		private object _newInstance;

		private bool _executed;

		private bool NewInstanceActivated => _newInstance != null;

		public IComponentRegistry ComponentRegistry => _activationScope.ComponentRegistry;

		public IComponentRegistration ComponentRegistration => _componentRegistration;

		public ILifetimeScope ActivationScope => _activationScope;

		public IEnumerable<Parameter> Parameters => _parameters;

		public event EventHandler<InstanceLookupEndingEventArgs> InstanceLookupEnding;

		public event EventHandler<InstanceLookupCompletionBeginningEventArgs> CompletionBeginning;

		public event EventHandler<InstanceLookupCompletionEndingEventArgs> CompletionEnding;

		public InstanceLookup(IComponentRegistration registration, IResolveOperation context, ISharingLifetimeScope mostNestedVisibleScope, IEnumerable<Parameter> parameters)
		{
			_parameters = parameters;
			_componentRegistration = registration;
			_context = context;
			_activationScope = _componentRegistration.Lifetime.FindScope(mostNestedVisibleScope);
		}

		public object Execute()
		{
			if (_executed)
			{
				throw new InvalidOperationException(ComponentActivationResources.ActivationAlreadyExecuted);
			}
			_executed = true;
			object result = ((_componentRegistration.Sharing != InstanceSharing.None) ? _activationScope.GetOrCreateAndShare(_componentRegistration.Id, () => Activate(Parameters)) : Activate(Parameters));
			this.InstanceLookupEnding?.Invoke(this, new InstanceLookupEndingEventArgs(this, NewInstanceActivated));
			return result;
		}

		private object Activate(IEnumerable<Parameter> parameters)
		{
			_componentRegistration.RaisePreparing(this, ref parameters);
			_newInstance = _componentRegistration.Activator.ActivateInstance(this, parameters);
			if (_componentRegistration.Ownership == InstanceOwnership.OwnedByLifetimeScope && _newInstance is IDisposable instance)
			{
				_activationScope.Disposer.AddInstanceForDisposal(instance);
			}
			_componentRegistration.RaiseActivating(this, parameters, ref _newInstance);
			return _newInstance;
		}

		public void Complete()
		{
			if (NewInstanceActivated)
			{
				this.CompletionBeginning?.Invoke(this, new InstanceLookupCompletionBeginningEventArgs(this));
				_componentRegistration.RaiseActivated(this, Parameters, _newInstance);
				this.CompletionEnding?.Invoke(this, new InstanceLookupCompletionEndingEventArgs(this));
			}
		}

		public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			return _context.GetOrCreateInstance(_activationScope, registration, parameters);
		}
	}
}
