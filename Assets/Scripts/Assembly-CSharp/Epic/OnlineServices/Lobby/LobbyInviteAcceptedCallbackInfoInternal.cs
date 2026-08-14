using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyInviteAcceptedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_InviteId;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		private IntPtr m_LobbyId;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public string InviteId
		{
			get
			{
				Helper.TryMarshalGet(m_InviteId, out string target);
				return target;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out ProductUserId target);
				return target;
			}
		}

		public ProductUserId TargetUserId
		{
			get
			{
				Helper.TryMarshalGet(m_TargetUserId, out ProductUserId target);
				return target;
			}
		}

		public string LobbyId
		{
			get
			{
				Helper.TryMarshalGet(m_LobbyId, out string target);
				return target;
			}
		}
	}
}
