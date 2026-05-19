using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;

namespace Autofac
{
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class RegistrationExtensionsResources
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
					ResourceManager resourceManager = new ResourceManager("Autofac.RegistrationExtensionsResources", typeof(RegistrationExtensionsResources).Assembly);
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

		internal static string InstanceRegistrationsAreSingleInstanceOnly => ResourceManager.GetString("InstanceRegistrationsAreSingleInstanceOnly", resourceCulture);

		internal static string MetadataAttributeNotFound => ResourceManager.GetString("MetadataAttributeNotFound", resourceCulture);

		internal static string MultipleMetadataAttributesSameType => ResourceManager.GetString("MultipleMetadataAttributesSameType", resourceCulture);

		internal static string NoMatchingConstructorExists => ResourceManager.GetString("NoMatchingConstructorExists", resourceCulture);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal RegistrationExtensionsResources()
		{
		}
	}
}
