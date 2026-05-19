using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;

namespace Autofac.Features.OpenGenerics
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	internal class OpenGenericServiceBinderResources
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
					ResourceManager resourceManager = new ResourceManager("Autofac.Features.OpenGenerics.OpenGenericServiceBinderResources", typeof(OpenGenericServiceBinderResources).Assembly);
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

		internal static string ImplementorDoesntImplementService => ResourceManager.GetString("ImplementorDoesntImplementService", resourceCulture);

		internal static string ImplementorMustBeOpenGenericTypeDefinition => ResourceManager.GetString("ImplementorMustBeOpenGenericTypeDefinition", resourceCulture);

		internal static string InterfaceIsNotImplemented => ResourceManager.GetString("InterfaceIsNotImplemented", resourceCulture);

		internal static string ServiceTypeMustBeOpenGenericTypeDefinition => ResourceManager.GetString("ServiceTypeMustBeOpenGenericTypeDefinition", resourceCulture);

		internal static string TypesAreNotConvertible => ResourceManager.GetString("TypesAreNotConvertible", resourceCulture);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal OpenGenericServiceBinderResources()
		{
		}
	}
}
