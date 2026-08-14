using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnFriendsUpdateInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		private FriendsStatus m_PreviousStatus;

		private FriendsStatus m_CurrentStatus;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public EpicAccountId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out EpicAccountId target);
				return target;
			}
		}

		public EpicAccountId TargetUserId
		{
			get
			{
				Helper.TryMarshalGet(m_TargetUserId, out EpicAccountId target);
				return target;
			}
		}

		public FriendsStatus PreviousStatus => m_PreviousStatus;

		public FriendsStatus CurrentStatus => m_CurrentStatus;
	}
}
