using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Autofac.Core.Registration
{
	internal class ServiceRegistrationInfo
	{
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "The _service field is useful in debugging and diagnostics.")]
		private readonly Service _service;

		private readonly LinkedList<IComponentRegistration> _implementations = new LinkedList<IComponentRegistration>();

		private Queue<IRegistrationSource> _sourcesToQuery;

		public bool IsInitialized { get; private set; }

		public IEnumerable<IComponentRegistration> Implementations
		{
			get
			{
				RequiresInitialization();
				return _implementations;
			}
		}

		public bool IsRegistered
		{
			get
			{
				RequiresInitialization();
				return Any;
			}
		}

		private bool Any => _implementations.First != null;

		public bool IsInitializing
		{
			get
			{
				if (!IsInitialized)
				{
					return _sourcesToQuery != null;
				}
				return false;
			}
		}

		public bool HasSourcesToQuery
		{
			get
			{
				if (IsInitializing)
				{
					return _sourcesToQuery.Count != 0;
				}
				return false;
			}
		}

		public ServiceRegistrationInfo(Service service)
		{
			_service = service;
		}

		private void RequiresInitialization()
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException(ServiceRegistrationInfoResources.NotInitialized);
			}
		}

		public void AddImplementation(IComponentRegistration registration, bool preserveDefaults)
		{
			if (preserveDefaults)
			{
				_implementations.AddLast(registration);
				return;
			}
			_ = Any;
			_implementations.AddFirst(registration);
		}

		public bool TryGetRegistration(out IComponentRegistration registration)
		{
			RequiresInitialization();
			if (Any)
			{
				registration = _implementations.First.Value;
				return true;
			}
			registration = null;
			return false;
		}

		public void Include(IRegistrationSource source)
		{
			if (IsInitialized)
			{
				BeginInitialization(new IRegistrationSource[1] { source });
			}
			else if (IsInitializing)
			{
				_sourcesToQuery.Enqueue(source);
			}
		}

		public void BeginInitialization(IEnumerable<IRegistrationSource> sources)
		{
			IsInitialized = false;
			_sourcesToQuery = new Queue<IRegistrationSource>(sources);
		}

		public void SkipSource(IRegistrationSource source)
		{
			EnforceDuringInitialization();
			_sourcesToQuery = new Queue<IRegistrationSource>(_sourcesToQuery.Where((IRegistrationSource rs) => rs != source));
		}

		private void EnforceDuringInitialization()
		{
			if (!IsInitializing)
			{
				throw new InvalidOperationException(ServiceRegistrationInfoResources.NotDuringInitialization);
			}
		}

		public IRegistrationSource DequeueNextSource()
		{
			EnforceDuringInitialization();
			return _sourcesToQuery.Dequeue();
		}

		public void CompleteInitialization()
		{
			IsInitialized = true;
			_sourcesToQuery = null;
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "registration")]
		public bool ShouldRecalculateAdaptersOn(IComponentRegistration registration)
		{
			return IsInitialized;
		}
	}
}
