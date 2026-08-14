using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsCopyMemberAttributeByKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private IntPtr m_AttrKey;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public string AttrKey
		{
			set
			{
				Helper.TryMarshalSet(ref m_AttrKey, value);
			}
		}

		public void Set(LobbyDetailsCopyMemberAttributeByKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				AttrKey = other.AttrKey;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsCopyMemberAttributeByKeyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
			Helper.TryMarshalDispose(ref m_AttrKey);
		}
	}
}
