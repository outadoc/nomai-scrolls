using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Resolving
{
	internal class CircularDependencyDetector
	{
		private const int MaxResolveDepth = 50;

		private static string CreateDependencyGraphTo(IComponentRegistration registration, IEnumerable<InstanceLookup> activationStack)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (activationStack == null)
			{
				throw new ArgumentNullException("activationStack");
			}
			string text = Display(registration);
			foreach (IComponentRegistration item in activationStack.Select((InstanceLookup a) => a.ComponentRegistration))
			{
				text = Display(item) + " -> " + text;
			}
			return text;
		}

		private static string Display(IComponentRegistration registration)
		{
			return registration.Activator.LimitType.FullName ?? string.Empty;
		}

		public static void CheckForCircularDependency(IComponentRegistration registration, Stack<InstanceLookup> activationStack, int callDepth)
		{
			if (registration == null)
			{
				throw new ArgumentNullException("registration");
			}
			if (activationStack == null)
			{
				throw new ArgumentNullException("activationStack");
			}
			if (callDepth > 50)
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorResources.MaxDepthExceeded, new object[1] { registration }));
			}
			if (IsCircularDependency(registration, activationStack))
			{
				throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorResources.CircularDependency, new object[1] { CreateDependencyGraphTo(registration, activationStack) }));
			}
		}

		private static bool IsCircularDependency(IComponentRegistration registration, IEnumerable<InstanceLookup> activationStack)
		{
			return activationStack.Any((InstanceLookup a) => a.ComponentRegistration == registration);
		}
	}
}
