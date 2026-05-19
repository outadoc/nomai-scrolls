using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Autofac.Util
{
	public class Disposable : IDisposable
	{
		private const int DisposedFlag = 1;

		private int _isDisposed;

		protected bool IsDisposed
		{
			get
			{
				Thread.MemoryBarrier();
				return _isDisposed == 1;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Dispose is implemented correctly, FxCop just doesn't see it.")]
		public void Dispose()
		{
			int num = Interlocked.Exchange(ref _isDisposed, 1);
			if (num != 1)
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
