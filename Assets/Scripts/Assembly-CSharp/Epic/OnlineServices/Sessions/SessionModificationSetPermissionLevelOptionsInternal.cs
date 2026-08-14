using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationSetPermissionLevelOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private OnlineSessionPermissionLevel m_PermissionLevel;

		public OnlineSessionPermissionLevel PermissionLevel
		{
			set
			{
				m_PermissionLevel = value;
			}
		}

		public void Set(SessionModificationSetPermissionLevelOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PermissionLevel = other.PermissionLevel;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationSetPermissionLevelOptions);
		}

		public void Dispose()
		{
		}
	}
}
