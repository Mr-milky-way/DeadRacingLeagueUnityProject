using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogGameRoundStartOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionIdentifier;

		private IntPtr m_LevelName;

		private IntPtr m_ModeName;

		private uint m_RoundTimeSeconds;

		public string SessionIdentifier
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionIdentifier, value);
			}
		}

		public string LevelName
		{
			set
			{
				Helper.TryMarshalSet(ref m_LevelName, value);
			}
		}

		public string ModeName
		{
			set
			{
				Helper.TryMarshalSet(ref m_ModeName, value);
			}
		}

		public uint RoundTimeSeconds
		{
			set
			{
				m_RoundTimeSeconds = value;
			}
		}

		public void Set(LogGameRoundStartOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionIdentifier = other.SessionIdentifier;
				LevelName = other.LevelName;
				ModeName = other.ModeName;
				RoundTimeSeconds = other.RoundTimeSeconds;
			}
		}

		public void Set(object other)
		{
			Set(other as LogGameRoundStartOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionIdentifier);
			Helper.TryMarshalDispose(ref m_LevelName);
			Helper.TryMarshalDispose(ref m_ModeName);
		}
	}
}
