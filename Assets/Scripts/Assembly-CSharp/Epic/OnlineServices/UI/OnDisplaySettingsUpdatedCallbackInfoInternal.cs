using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnDisplaySettingsUpdatedCallbackInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private int m_IsVisible;

		private int m_IsExclusiveInput;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public bool IsVisible
		{
			get
			{
				Helper.TryMarshalGet(m_IsVisible, out var target);
				return target;
			}
		}

		public bool IsExclusiveInput
		{
			get
			{
				Helper.TryMarshalGet(m_IsExclusiveInput, out var target);
				return target;
			}
		}
	}
}
