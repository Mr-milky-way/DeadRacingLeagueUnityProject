using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UpdateParentEmailOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_ParentEmail;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string ParentEmail
		{
			set
			{
				Helper.TryMarshalSet(ref m_ParentEmail, value);
			}
		}

		public void Set(UpdateParentEmailOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				ParentEmail = other.ParentEmail;
			}
		}

		public void Set(object other)
		{
			Set(other as UpdateParentEmailOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ParentEmail);
		}
	}
}
