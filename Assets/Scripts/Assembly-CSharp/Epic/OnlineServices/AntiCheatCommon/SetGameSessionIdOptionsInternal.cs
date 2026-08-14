using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SetGameSessionIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_GameSessionId;

		public string GameSessionId
		{
			set
			{
				Helper.TryMarshalSet(ref m_GameSessionId, value);
			}
		}

		public void Set(SetGameSessionIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				GameSessionId = other.GameSessionId;
			}
		}

		public void Set(object other)
		{
			Set(other as SetGameSessionIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_GameSessionId);
		}
	}
}
