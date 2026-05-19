using System;
using System.Collections.Generic;

namespace Autofac.Core.Resolving
{
	internal class ResolveOperation : IComponentContext, IResolveOperation
	{
		private readonly Stack<InstanceLookup> _activationStack = new Stack<InstanceLookup>();

		private ICollection<InstanceLookup> _successfulActivations;

		private readonly ISharingLifetimeScope _mostNestedLifetimeScope;

		private int _callDepth;

		private bool _ended;

		public IComponentRegistry ComponentRegistry => _mostNestedLifetimeScope.ComponentRegistry;

		public event EventHandler<ResolveOperationEndingEventArgs> CurrentOperationEnding;

		public event EventHandler<InstanceLookupBeginningEventArgs> InstanceLookupBeginning;

		public ResolveOperation(ISharingLifetimeScope mostNestedLifetimeScope)
		{
			_mostNestedLifetimeScope = mostNestedLifetimeScope;
			ResetSuccessfulActivations();
		}

		public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			return GetOrCreateInstance(_mostNestedLifetimeScope, registration, parameters);
		}

		public object Execute(IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			object result;
			try
			{
				result = ResolveComponent(registration, parameters);
			}
			catch (DependencyResolutionException exception)
			{
				End(exception);
				throw;
			}
			catch (Exception ex)
			{
				End(ex);
				throw new DependencyResolutionException(ResolveOperationResources.ExceptionDuringResolve, ex);
			}
			End();
			return result;
		}

		public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, IComponentRegistration registration, IEnumerable<Parameter> parameters)
		{
			if (_ended)
			{
				throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, (Exception)null);
			}
			CircularDependencyDetector.CheckForCircularDependency(registration, _activationStack, ++_callDepth);
			InstanceLookup instanceLookup = new InstanceLookup(registration, this, currentOperationScope, parameters);
			_activationStack.Push(instanceLookup);
			this.InstanceLookupBeginning?.Invoke(this, new InstanceLookupBeginningEventArgs(instanceLookup));
			object result = instanceLookup.Execute();
			_successfulActivations.Add(instanceLookup);
			_activationStack.Pop();
			if (_activationStack.Count == 0)
			{
				CompleteActivations();
			}
			_callDepth--;
			return result;
		}

		private void CompleteActivations()
		{
			ICollection<InstanceLookup> successfulActivations = _successfulActivations;
			ResetSuccessfulActivations();
			foreach (InstanceLookup item in successfulActivations)
			{
				item.Complete();
			}
		}

		private void ResetSuccessfulActivations()
		{
			_successfulActivations = new List<InstanceLookup>();
		}

		private void End(Exception exception = null)
		{
			if (!_ended)
			{
				_ended = true;
				this.CurrentOperationEnding?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
			}
		}
	}
}
