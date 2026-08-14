using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct JoinGameAcceptedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_JoinInfo;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		private ulong m_UiEventId;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public string JoinInfo
		{
			get
			{
				Helper.TryMarshalGet(m_JoinInfo, out string target);
				return target;
			}
		}

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

		public ulong UiEventId => m_UiEventId;
	}
}
