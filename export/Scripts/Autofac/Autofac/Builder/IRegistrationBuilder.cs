using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac.Core;

namespace Autofac.Builder
{
	public interface IRegistrationBuilder<out TLimit, out TActivatorData, out TRegistrationStyle>
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		TActivatorData ActivatorData { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		TRegistrationStyle RegistrationStyle { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		RegistrationData RegistrationData { get; }

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerDependency();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(params object[] lifetimeScopeTag);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(Type serviceType);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(object serviceKey);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(object serviceKey, Type serviceType);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>();

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName, Type serviceType);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named<TService>(string serviceName);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed(object serviceKey, Type serviceType);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed<TService>(object serviceKey);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs> handler);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<IActivatingEventArgs<TLimit>> handler);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<IActivatedEventArgs<TLimit>> handler);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(PropertyWiringOptions options = PropertyWiringOptions.None);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(string key, object value);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(IEnumerable<KeyValuePair<string, object>> properties);

		IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata<TMetadata>(Action<MetadataConfiguration<TMetadata>> configurationAction);
	}
}
