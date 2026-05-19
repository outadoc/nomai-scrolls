namespace Autofac.Core
{
	internal class AutoActivateService : Service
	{
		public override string Description => "AutoActivate";

		public override bool Equals(object obj)
		{
			AutoActivateService autoActivateService = obj as AutoActivateService;
			return autoActivateService != null;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}
