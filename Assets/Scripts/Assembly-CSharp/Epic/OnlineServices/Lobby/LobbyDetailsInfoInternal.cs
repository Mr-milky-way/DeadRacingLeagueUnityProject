using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LobbyId;

		private IntPtr m_LobbyOwnerUserId;

		private LobbyPermissionLevel m_PermissionLevel;

		private uint m_AvailableSlots;

		private uint m_MaxMembers;

		private int m_AllowInvites;

		private IntPtr m_BucketId;

		private int m_AllowHostMigration;

		public string LobbyId
		{
			get
			{
				Helper.TryMarshalGet(m_LobbyId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LobbyId, value);
			}
		}

		public ProductUserId LobbyOwnerUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LobbyOwnerUserId, out ProductUserId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LobbyOwnerUserId, value);
			}
		}

		public LobbyPermissionLevel PermissionLevel
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

		public uint AvailableSlots
		{
			get
			{
				return m_AvailableSlots;
			}
			set
			{
				m_AvailableSlots = value;
			}
		}

		public uint MaxMembers
		{
			get
			{
				return m_MaxMembers;
			}
			set
			{
				m_MaxMembers = value;
			}
		}

		public bool AllowInvites
		{
			get
			{
				Helper.TryMarshalGet(m_AllowInvites, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AllowInvites, value);
			}
		}

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

		public bool AllowHostMigration
		{
			get
			{
				Helper.TryMarshalGet(m_AllowHostMigration, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AllowHostMigration, value);
			}
		}

		public void Set(LobbyDetailsInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LobbyId = other.LobbyId;
				LobbyOwnerUserId = other.LobbyOwnerUserId;
				PermissionLevel = other.PermissionLevel;
				AvailableSlots = other.AvailableSlots;
				MaxMembers = other.MaxMembers;
				AllowInvites = other.AllowInvites;
				BucketId = other.BucketId;
				AllowHostMigration = other.AllowHostMigration;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LobbyId);
			Helper.TryMarshalDispose(ref m_LobbyOwnerUserId);
			Helper.TryMarshalDispose(ref m_BucketId);
		}
	}
}
