using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PresenceChangedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_PresenceUserId;

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

		public EpicAccountId PresenceUserId
		{
			get
			{
				Helper.TryMarshalGet(m_PresenceUserId, out EpicAccountId target);
				return target;
			}
		}
	}
}
