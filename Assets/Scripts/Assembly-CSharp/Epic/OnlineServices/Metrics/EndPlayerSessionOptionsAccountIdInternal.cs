using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics
{
	[StructLayout(LayoutKind.Explicit, Pack = 4)]
	internal struct EndPlayerSessionOptionsAccountIdInternal : ISettable, IDisposable
	{
		[FieldOffset(0)]
		private MetricsAccountIdType m_AccountIdType;

		[FieldOffset(4)]
		private IntPtr m_Epic;

		[FieldOffset(4)]
		private IntPtr m_External;

		public EpicAccountId Epic
		{
			get
			{
				Helper.TryMarshalGet(m_Epic, out EpicAccountId target, m_AccountIdType, MetricsAccountIdType.Epic);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Epic, value, ref m_AccountIdType, MetricsAccountIdType.Epic, this);
			}
		}

		public string External
		{
			get
			{
				Helper.TryMarshalGet(m_External, out string target, m_AccountIdType, MetricsAccountIdType.External);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_External, value, ref m_AccountIdType, MetricsAccountIdType.External, this);
			}
		}

		public void Set(EndPlayerSessionOptionsAccountId other)
		{
			if (other != null)
			{
				Epic = other.Epic;
				External = other.External;
			}
		}

		public void Set(object other)
		{
			Set(other as EndPlayerSessionOptionsAccountId);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Epic);
			Helper.TryMarshalDispose(ref m_External, m_AccountIdType, MetricsAccountIdType.External);
		}
	}
}
