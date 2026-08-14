using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetToggleFriendsKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(GetToggleFriendsKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as GetToggleFriendsKeyOptions);
		}

		public void Dispose()
		{
		}
	}
}
