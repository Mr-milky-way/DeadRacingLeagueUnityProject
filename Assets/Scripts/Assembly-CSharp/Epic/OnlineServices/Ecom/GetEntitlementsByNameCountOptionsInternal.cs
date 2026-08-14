using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetEntitlementsByNameCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_EntitlementName;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string EntitlementName
		{
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementName, value);
			}
		}

		public void Set(GetEntitlementsByNameCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				EntitlementName = other.EntitlementName;
			}
		}

		public void Set(object other)
		{
			Set(other as GetEntitlementsByNameCountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_EntitlementName);
		}
	}
}
