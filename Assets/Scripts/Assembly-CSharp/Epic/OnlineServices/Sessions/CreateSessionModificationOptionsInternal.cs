using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CreateSessionModificationOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		private IntPtr m_BucketId;

		private uint m_MaxPlayers;

		private IntPtr m_LocalUserId;

		private int m_PresenceEnabled;

		private IntPtr m_SessionId;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public string BucketId
		{
			set
			{
				Helper.TryMarshalSet(ref m_BucketId, value);
			}
		}

		public uint MaxPlayers
		{
			set
			{
				m_MaxPlayers = value;
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public bool PresenceEnabled
		{
			set
			{
				Helper.TryMarshalSet(ref m_PresenceEnabled, value);
			}
		}

		public string SessionId
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionId, value);
			}
		}

		public void Set(CreateSessionModificationOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				SessionName = other.SessionName;
				BucketId = other.BucketId;
				MaxPlayers = other.MaxPlayers;
				LocalUserId = other.LocalUserId;
				PresenceEnabled = other.PresenceEnabled;
				SessionId = other.SessionId;
			}
		}

		public void Set(object other)
		{
			Set(other as CreateSessionModificationOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
			Helper.TryMarshalDispose(ref m_BucketId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_SessionId);
		}
	}
}
