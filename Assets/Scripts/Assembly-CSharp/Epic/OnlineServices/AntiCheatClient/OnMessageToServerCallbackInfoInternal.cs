using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatClient
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnMessageToServerCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_MessageData;

		private uint m_MessageDataSizeBytes;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public byte[] MessageData
		{
			get
			{
				Helper.TryMarshalGet(m_MessageData, out byte[] target, m_MessageDataSizeBytes);
				return target;
			}
		}
	}
}
