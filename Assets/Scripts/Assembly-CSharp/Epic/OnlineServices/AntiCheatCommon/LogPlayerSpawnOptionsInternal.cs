using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogPlayerSpawnOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SpawnedPlayerHandle;

		private uint m_TeamId;

		private uint m_CharacterId;

		public IntPtr SpawnedPlayerHandle
		{
			set
			{
				m_SpawnedPlayerHandle = value;
			}
		}

		public uint TeamId
		{
			set
			{
				m_TeamId = value;
			}
		}

		public uint CharacterId
		{
			set
			{
				m_CharacterId = value;
			}
		}

		public void Set(LogPlayerSpawnOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SpawnedPlayerHandle = other.SpawnedPlayerHandle;
				TeamId = other.TeamId;
				CharacterId = other.CharacterId;
			}
		}

		public void Set(object other)
		{
			Set(other as LogPlayerSpawnOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SpawnedPlayerHandle);
		}
	}
}
