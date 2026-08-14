using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceModificationSetJoinInfoOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_JoinInfo;

		public string JoinInfo
		{
			set
			{
				Helper.TryMarshalSet(ref m_JoinInfo, value);
			}
		}

		public void Set(PresenceModificationSetJoinInfoOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				JoinInfo = other.JoinInfo;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationSetJoinInfoOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_JoinInfo);
		}
	}
}
