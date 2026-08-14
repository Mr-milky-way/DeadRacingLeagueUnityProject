using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyMemberStatusReceivedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LobbyId;

		private IntPtr m_TargetUserId;

		private LobbyMemberStatus m_CurrentStatus;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public string LobbyId
		{
			get
			{
				Helper.TryMarshalGet(m_LobbyId, out string target);
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

		public LobbyMemberStatus CurrentStatus => m_CurrentStatus;
	}
}
