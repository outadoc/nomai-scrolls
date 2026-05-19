using System;

namespace Autofac.Core
{
	public sealed class UniqueService : Service
	{
		private Guid _id;

		public override string Description => _id.ToString();

		public UniqueService()
			: this(Guid.NewGuid())
		{
		}

		public UniqueService(Guid id)
		{
			_id = id;
		}

		public override bool Equals(object obj)
		{
			UniqueService uniqueService = obj as UniqueService;
			if (uniqueService != null)
			{
				return _id == uniqueService._id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}
	}
}
