using System;
using System.Globalization;
using System.Security;

namespace Autofac.Core
{
	public class DependencyResolutionException : Exception
	{
		public override string Message
		{
			[SecuritySafeCritical]
			get
			{
				string text = base.Message;
				if (base.InnerException != null)
				{
					string message = base.InnerException.Message;
					text = string.Format(CultureInfo.CurrentCulture, DependencyResolutionExceptionResources.MessageNestingFormat, new object[2] { text, message });
				}
				return text;
			}
		}

		public DependencyResolutionException(string message)
			: base(message)
		{
		}

		public DependencyResolutionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
