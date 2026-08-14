using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UpdateModCallbackInfoInternal : ICallbackInfoInternal
	{
		private Result m_ResultCode;

		private IntPtr m_LocalUserId;

		private IntPtr m_ClientData;

		private IntPtr m_Mod;

		public Result ResultCode => m_ResultCode;

		public EpicAccountId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out EpicAccountId target);
				return target;
			}
		}

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public ModIdentifier Mod
		{
			get
			{
				Helper.TryMarshalGet<ModIdentifierInternal, ModIdentifier>(m_Mod, out var target);
				return target;
			}
		}
	}
}
