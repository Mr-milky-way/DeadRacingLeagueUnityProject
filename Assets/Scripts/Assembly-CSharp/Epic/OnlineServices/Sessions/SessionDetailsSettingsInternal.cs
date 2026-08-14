using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsSettingsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_BucketId;

		private uint m_NumPublicConnections;

		private int m_AllowJoinInProgress;

		private OnlineSessionPermissionLevel m_PermissionLevel;

		private int m_InvitesAllowed;

		public string BucketId
		{
			get
			{
				Helper.TryMarshalGet(m_BucketId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_BucketId, value);
			}
		}

		public uint NumPublicConnections
		{
			get
			{
				return m_NumPublicConnections;
			}
			set
			{
				m_NumPublicConnections = value;
			}
		}

		public bool AllowJoinInProgress
		{
			get
			{
				Helper.TryMarshalGet(m_AllowJoinInProgress, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AllowJoinInProgress, value);
			}
		}

		public OnlineSessionPermissionLevel PermissionLevel
		{
			get
			{
				return m_PermissionLevel;
			}
			set
			{
				m_PermissionLevel = value;
			}
		}

		public bool InvitesAllowed
		{
			get
			{
				Helper.TryMarshalGet(m_InvitesAllowed, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_InvitesAllowed, value);
			}
		}

		public void Set(SessionDetailsSettings other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				BucketId = other.BucketId;
				NumPublicConnections = other.NumPublicConnections;
				AllowJoinInProgress = other.AllowJoinInProgress;
				PermissionLevel = other.PermissionLevel;
				InvitesAllowed = other.InvitesAllowed;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsSettings);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_BucketId);
		}
	}
}
