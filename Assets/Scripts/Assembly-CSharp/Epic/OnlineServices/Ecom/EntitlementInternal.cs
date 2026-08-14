using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct EntitlementInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_EntitlementName;

		private IntPtr m_EntitlementId;

		private IntPtr m_CatalogItemId;

		private int m_ServerIndex;

		private int m_Redeemed;

		private long m_EndTimestamp;

		public string EntitlementName
		{
			get
			{
				Helper.TryMarshalGet(m_EntitlementName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementName, value);
			}
		}

		public string EntitlementId
		{
			get
			{
				Helper.TryMarshalGet(m_EntitlementId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementId, value);
			}
		}

		public string CatalogItemId
		{
			get
			{
				Helper.TryMarshalGet(m_CatalogItemId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CatalogItemId, value);
			}
		}

		public int ServerIndex
		{
			get
			{
				return m_ServerIndex;
			}
			set
			{
				m_ServerIndex = value;
			}
		}

		public bool Redeemed
		{
			get
			{
				Helper.TryMarshalGet(m_Redeemed, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Redeemed, value);
			}
		}

		public long EndTimestamp
		{
			get
			{
				return m_EndTimestamp;
			}
			set
			{
				m_EndTimestamp = value;
			}
		}

		public void Set(Entitlement other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				EntitlementName = other.EntitlementName;
				EntitlementId = other.EntitlementId;
				CatalogItemId = other.CatalogItemId;
				ServerIndex = other.ServerIndex;
				Redeemed = other.Redeemed;
				EndTimestamp = other.EndTimestamp;
			}
		}

		public void Set(object other)
		{
			Set(other as Entitlement);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_EntitlementName);
			Helper.TryMarshalDispose(ref m_EntitlementId);
			Helper.TryMarshalDispose(ref m_CatalogItemId);
		}
	}
}
