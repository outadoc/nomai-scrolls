namespace Autofac.Core.Registration
{
	internal static class ComponentRegistrationExtensions
	{
		public static bool IsAdapting(this IComponentRegistration componentRegistration)
		{
			return componentRegistration.Target != componentRegistration;
		}
	}
}
