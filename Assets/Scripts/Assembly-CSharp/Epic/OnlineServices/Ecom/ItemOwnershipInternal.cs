using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ItemOwnershipInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Id;

		private OwnershipStatus m_OwnershipStatus;

		public string Id
		{
			get
			{
				Helper.TryMarshalGet(m_Id, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Id, value);
			}
		}

		public OwnershipStatus OwnershipStatus
		{
			get
			{
				return m_OwnershipStatus;
			}
			set
			{
				m_OwnershipStatus = value;
			}
		}

		public void Set(ItemOwnership other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Id = other.Id;
				OwnershipStatus = other.OwnershipStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as ItemOwnership);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Id);
		}
	}
}
