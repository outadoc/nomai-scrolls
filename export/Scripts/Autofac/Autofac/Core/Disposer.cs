using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core
{
	internal class Disposer : Disposable, IDisposer, IDisposable
	{
		private Stack<IDisposable> _items = new Stack<IDisposable>();

		private readonly object _synchRoot = new object();

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_synchRoot)
				{
					while (_items.Count > 0)
					{
						IDisposable disposable = _items.Pop();
						disposable.Dispose();
					}
					_items = null;
				}
			}
			base.Dispose(disposing);
		}

		public void AddInstanceForDisposal(IDisposable instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			lock (_synchRoot)
			{
				_items.Push(instance);
			}
		}
	}
}
