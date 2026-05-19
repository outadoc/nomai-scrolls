using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Core
{
	public interface IComponentRegistration : IDisposable
	{
		Guid Id { get; }

		IInstanceActivator Activator { get; }

		IComponentLifetime Lifetime { get; }

		InstanceSharing Sharing { get; }

		InstanceOwnership Ownership { get; }

		IEnumerable<Service> Services { get; }

		IDictionary<string, object> Metadata { get; }

		IComponentRegistration Target { get; }

		event EventHandler<PreparingEventArgs> Preparing;

		event EventHandler<ActivatingEventArgs<object>> Activating;

		event EventHandler<ActivatedEventArgs<object>> Activated;

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#", Justification = "The method may change the backing store of the parameter collection.")]
		void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters);

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#", Justification = "The method may change the object as part of activation.")]
		void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance);

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is the method that would raise the event.")]
		void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance);
	}
}
