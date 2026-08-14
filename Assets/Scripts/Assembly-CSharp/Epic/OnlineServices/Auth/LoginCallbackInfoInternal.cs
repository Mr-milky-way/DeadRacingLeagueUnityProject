using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LoginCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_PinGrantInfo;

		private IntPtr m_ContinuanceToken;

		private IntPtr m_AccountFeatureRestrictedInfo;

		public Result ResultCode => m_ResultCode;

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

		public PinGrantInfo PinGrantInfo
		{
			get
			{
				Helper.TryMarshalGet<PinGrantInfoInternal, PinGrantInfo>(m_PinGrantInfo, out var target);
				return target;
			}
		}

		public ContinuanceToken ContinuanceToken
		{
			get
			{
				Helper.TryMarshalGet(m_ContinuanceToken, out ContinuanceToken target);
				return target;
			}
		}

		public AccountFeatureRestrictedInfo AccountFeatureRestrictedInfo
		{
			get
			{
				Helper.TryMarshalGet<AccountFeatureRestrictedInfoInternal, AccountFeatureRestrictedInfo>(m_AccountFeatureRestrictedInfo, out var target);
				return target;
			}
		}
	}
}
