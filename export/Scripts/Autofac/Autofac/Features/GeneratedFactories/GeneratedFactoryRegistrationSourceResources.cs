using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;

namespace Autofac.Features.GeneratedFactories
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	internal class GeneratedFactoryRegistrationSourceResources
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
					ResourceManager resourceManager = new ResourceManager("Autofac.Features.GeneratedFactories.GeneratedFactoryRegistrationSourceResources", typeof(GeneratedFactoryRegistrationSourceResources).Assembly);
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

		internal static string DuplicateTypesInTypeMappedFuncParameterList => ResourceManager.GetString("DuplicateTypesInTypeMappedFuncParameterList", resourceCulture);

		internal static string GeneratedFactoryRegistrationSourceDescription => ResourceManager.GetString("GeneratedFactoryRegistrationSourceDescription", resourceCulture);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal GeneratedFactoryRegistrationSourceResources()
		{
		}
	}
}
