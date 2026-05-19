using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;

namespace Autofac.Core.Activators.Reflection
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	internal class ConstructorParameterBindingResources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Autofac.Core.Activators.Reflection.ConstructorParameterBindingResources", typeof(ConstructorParameterBindingResources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string BoundConstructor => ResourceManager.GetString("BoundConstructor", resourceCulture);

		internal static string CannotInstantitate => ResourceManager.GetString("CannotInstantitate", resourceCulture);

		internal static string ExceptionDuringInstantiation => ResourceManager.GetString("ExceptionDuringInstantiation", resourceCulture);

		internal static string NonBindableConstructor => ResourceManager.GetString("NonBindableConstructor", resourceCulture);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal ConstructorParameterBindingResources()
		{
		}
	}
}
