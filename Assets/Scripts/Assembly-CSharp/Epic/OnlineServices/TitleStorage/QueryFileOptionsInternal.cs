using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryFileOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Filename;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string Filename
		{
			set
			{
				Helper.TryMarshalSet(ref m_Filename, value);
			}
		}

		public void Set(QueryFileOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Filename = other.Filename;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryFileOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Filename);
		}
	}
}
