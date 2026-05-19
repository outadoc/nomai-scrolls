using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Util;

namespace Autofac.Features.Collections
{
	internal static class CollectionRegistrationExtensions
	{
		private const string MemberOfPropertyKey = "Autofac.CollectionRegistrationExtensions.MemberOf";

		public static IRegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle> RegisterCollection<T>(ContainerBuilder builder, string collectionName, Type elementType)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if ((object)elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");
			Type limitType = elementType.MakeArrayType();
			DelegateActivator activator = new DelegateActivator(limitType, delegate(IComponentContext c, IEnumerable<Parameter> p)
			{
				IEnumerable<IComponentRegistration> elementRegistrations = GetElementRegistrations(collectionName, c.ComponentRegistry);
				object[] array = elementRegistrations.Select((IComponentRegistration e) => c.ResolveComponent(e, p)).ToArray();
				Array array2 = Array.CreateInstance(elementType, array.Length);
				array.CopyTo(array2, 0);
				return array2;
			});
			RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle> rb = new RegistrationBuilder<T[], SimpleActivatorData, SingleRegistrationStyle>(new TypedService(typeof(T[])), new SimpleActivatorData(activator), new SingleRegistrationStyle());
			builder.RegisterCallback(delegate(IComponentRegistry cr)
			{
				RegistrationBuilder.RegisterSingleComponent(cr, rb);
			});
			return rb;
		}

		private static IEnumerable<IComponentRegistration> GetElementRegistrations(string collectionName, IComponentRegistry registry)
		{
			return registry.Registrations.Where((IComponentRegistration cr) => IsElementRegistration(collectionName, cr));
		}

		private static bool IsElementRegistration(string collectionName, IComponentRegistration cr)
		{
			if (cr.Metadata.TryGetValue("Autofac.CollectionRegistrationExtensions.MemberOf", out var value))
			{
				return ((IEnumerable<string>)value).Contains(collectionName);
			}
			return false;
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> MemberOf<TLimit, TActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, string collectionName) where TSingleRegistrationStyle : SingleRegistrationStyle
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			Enforce.ArgumentNotNullOrEmpty(collectionName, "collectionName");
			registration.OnRegistered(delegate(ComponentRegisteredEventArgs e)
			{
				IDictionary<string, object> metadata = e.ComponentRegistration.Metadata;
				if (metadata.ContainsKey("Autofac.CollectionRegistrationExtensions.MemberOf"))
				{
					metadata["Autofac.CollectionRegistrationExtensions.MemberOf"] = ((IEnumerable<string>)metadata["Autofac.CollectionRegistrationExtensions.MemberOf"]).Union(new string[1] { collectionName });
				}
				else
				{
					metadata.Add("Autofac.CollectionRegistrationExtensions.MemberOf", new string[1] { collectionName });
				}
			});
			return registration;
		}
	}
}
