using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogGameRoundEndOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_WinningTeamId;

		public uint WinningTeamId
		{
			set
			{
				m_WinningTeamId = value;
			}
		}

		public void Set(LogGameRoundEndOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				WinningTeamId = other.WinningTeamId;
			}
		}

		public void Set(object other)
		{
			Set(other as LogGameRoundEndOptions);
		}

		public void Dispose()
		{
		}
	}
}
