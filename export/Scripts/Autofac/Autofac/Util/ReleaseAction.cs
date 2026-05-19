using System;

namespace Autofac.Util
{
	internal class ReleaseAction : Disposable
	{
		private readonly Action _action;

		public ReleaseAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			_action = action;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_action();
			}
			base.Dispose(disposing);
		}
	}
}
