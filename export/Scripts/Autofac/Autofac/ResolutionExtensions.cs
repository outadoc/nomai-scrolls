using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;

namespace Autofac
{
	public static class ResolutionExtensions
	{
		private static readonly IEnumerable<Parameter> NoParameters = Enumerable.Empty<Parameter>();

		public static TService InjectProperties<TService>(this IComponentContext context, TService instance)
		{
			AutowiringPropertyInjector.InjectProperties(context, instance, overrideSetValues: true);
			return instance;
		}

		public static TService InjectUnsetProperties<TService>(this IComponentContext context, TService instance)
		{
			AutowiringPropertyInjector.InjectProperties(context, instance, overrideSetValues: false);
			return instance;
		}

		public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName)
		{
			return context.ResolveNamed<TService>(serviceName, NoParameters);
		}

		public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters)
		{
			return (TService)context.ResolveService(new KeyedService(serviceName, typeof(TService)), parameters);
		}

		public static TService ResolveNamed<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters)
		{
			return context.ResolveNamed<TService>(serviceName, (IEnumerable<Parameter>)parameters);
		}

		public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey)
		{
			return context.ResolveKeyed<TService>(serviceKey, NoParameters);
		}

		public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters)
		{
			return (TService)context.ResolveService(new KeyedService(serviceKey, typeof(TService)), parameters);
		}

		public static TService ResolveKeyed<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters)
		{
			return context.ResolveKeyed<TService>(serviceKey, (IEnumerable<Parameter>)parameters);
		}

		public static TService Resolve<TService>(this IComponentContext context)
		{
			return context.Resolve<TService>(NoParameters);
		}

		public static TService Resolve<TService>(this IComponentContext context, IEnumerable<Parameter> parameters)
		{
			return (TService)context.Resolve(typeof(TService), parameters);
		}

		public static TService Resolve<TService>(this IComponentContext context, params Parameter[] parameters)
		{
			return context.Resolve<TService>((IEnumerable<Parameter>)parameters);
		}

		public static object Resolve(this IComponentContext context, Type serviceType)
		{
			return context.Resolve(serviceType, NoParameters);
		}

		public static object Resolve(this IComponentContext context, Type serviceType, IEnumerable<Parameter> parameters)
		{
			return context.ResolveService(new TypedService(serviceType), parameters);
		}

		public static object Resolve(this IComponentContext context, Type serviceType, params Parameter[] parameters)
		{
			return context.Resolve(serviceType, (IEnumerable<Parameter>)parameters);
		}

		public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType)
		{
			return context.ResolveNamed(serviceName, serviceType, NoParameters);
		}

		public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType, IEnumerable<Parameter> parameters)
		{
			return context.ResolveService(new KeyedService(serviceName, serviceType), parameters);
		}

		public static object ResolveNamed(this IComponentContext context, string serviceName, Type serviceType, params Parameter[] parameters)
		{
			return context.ResolveNamed(serviceName, serviceType, (IEnumerable<Parameter>)parameters);
		}

		public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType)
		{
			return context.ResolveKeyed(serviceKey, serviceType, NoParameters);
		}

		public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, IEnumerable<Parameter> parameters)
		{
			return context.ResolveService(new KeyedService(serviceKey, serviceType), parameters);
		}

		public static object ResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, params Parameter[] parameters)
		{
			return context.ResolveKeyed(serviceKey, serviceType, (IEnumerable<Parameter>)parameters);
		}

		public static object ResolveService(this IComponentContext context, Service service)
		{
			return context.ResolveService(service, NoParameters);
		}

		public static object ResolveService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (!context.TryResolveService(service, parameters, out var instance))
			{
				throw new ComponentNotRegisteredException(service);
			}
			return instance;
		}

		public static object ResolveService(this IComponentContext context, Service service, params Parameter[] parameters)
		{
			return context.ResolveService(service, (IEnumerable<Parameter>)parameters);
		}

		public static TService ResolveOptional<TService>(this IComponentContext context) where TService : class
		{
			return context.ResolveOptional<TService>(NoParameters);
		}

		public static TService ResolveOptional<TService>(this IComponentContext context, IEnumerable<Parameter> parameters) where TService : class
		{
			return (TService)context.ResolveOptionalService(new TypedService(typeof(TService)), parameters);
		}

		public static TService ResolveOptional<TService>(this IComponentContext context, params Parameter[] parameters) where TService : class
		{
			return context.ResolveOptional<TService>((IEnumerable<Parameter>)parameters);
		}

		public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName) where TService : class
		{
			return context.ResolveOptionalKeyed<TService>(serviceName);
		}

		public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, IEnumerable<Parameter> parameters) where TService : class
		{
			return context.ResolveOptionalKeyed<TService>(serviceName, parameters);
		}

		public static TService ResolveOptionalNamed<TService>(this IComponentContext context, string serviceName, params Parameter[] parameters) where TService : class
		{
			return context.ResolveOptionalKeyed<TService>(serviceName, parameters);
		}

		public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey) where TService : class
		{
			return context.ResolveOptionalKeyed<TService>(serviceKey, NoParameters);
		}

		public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, IEnumerable<Parameter> parameters) where TService : class
		{
			return (TService)context.ResolveOptionalService(new KeyedService(serviceKey, typeof(TService)), parameters);
		}

		public static TService ResolveOptionalKeyed<TService>(this IComponentContext context, object serviceKey, params Parameter[] parameters) where TService : class
		{
			return context.ResolveOptionalKeyed<TService>(serviceKey, (IEnumerable<Parameter>)parameters);
		}

		public static object ResolveOptional(this IComponentContext context, Type serviceType)
		{
			return context.ResolveOptional(serviceType, NoParameters);
		}

		public static object ResolveOptional(this IComponentContext context, Type serviceType, IEnumerable<Parameter> parameters)
		{
			return context.ResolveOptionalService(new TypedService(serviceType), parameters);
		}

		public static object ResolveOptional(this IComponentContext context, Type serviceType, params Parameter[] parameters)
		{
			return context.ResolveOptional(serviceType, (IEnumerable<Parameter>)parameters);
		}

		public static object ResolveOptionalService(this IComponentContext context, Service service)
		{
			return context.ResolveOptionalService(service, NoParameters);
		}

		public static object ResolveOptionalService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			context.TryResolveService(service, parameters, out var instance);
			return instance;
		}

		public static object ResolveOptionalService(this IComponentContext context, Service service, params Parameter[] parameters)
		{
			return context.ResolveOptionalService(service, (IEnumerable<Parameter>)parameters);
		}

		public static bool IsRegistered<TService>(this IComponentContext context)
		{
			return context.IsRegistered(typeof(TService));
		}

		public static bool IsRegistered(this IComponentContext context, Type serviceType)
		{
			return context.IsRegisteredService(new TypedService(serviceType));
		}

		public static bool IsRegisteredWithName(this IComponentContext context, string serviceName, Type serviceType)
		{
			return context.IsRegisteredWithKey(serviceName, serviceType);
		}

		public static bool IsRegisteredWithName<TService>(this IComponentContext context, string serviceName)
		{
			return context.IsRegisteredWithKey<TService>(serviceName);
		}

		public static bool IsRegisteredWithKey(this IComponentContext context, object serviceKey, Type serviceType)
		{
			return context.IsRegisteredService(new KeyedService(serviceKey, serviceType));
		}

		public static bool IsRegisteredWithKey<TService>(this IComponentContext context, object serviceKey)
		{
			return context.IsRegisteredWithKey(serviceKey, typeof(TService));
		}

		public static bool IsRegisteredService(this IComponentContext context, Service service)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			return context.ComponentRegistry.IsRegistered(service);
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryResolveService(this IComponentContext context, Service service, IEnumerable<Parameter> parameters, out object instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (!context.ComponentRegistry.TryGetRegistration(service, out var registration))
			{
				instance = null;
				return false;
			}
			instance = context.ResolveComponent(registration, parameters);
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryResolveService(this IComponentContext context, Service service, out object instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return context.TryResolveService(service, NoParameters, out instance);
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryResolve(this IComponentContext context, Type serviceType, out object instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return context.TryResolveService(new TypedService(serviceType), NoParameters, out instance);
		}

		public static bool TryResolve<T>(this IComponentContext context, out T instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			object instance2;
			bool flag = context.TryResolve(typeof(T), out instance2);
			instance = (flag ? ((T)instance2) : default(T));
			return flag;
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryResolveNamed(this IComponentContext context, string serviceName, Type serviceType, out object instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return context.TryResolveService(new KeyedService(serviceName, serviceType), NoParameters, out instance);
		}

		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
		public static bool TryResolveKeyed(this IComponentContext context, object serviceKey, Type serviceType, out object instance)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return context.TryResolveService(new KeyedService(serviceKey, serviceType), NoParameters, out instance);
		}
	}
}
