using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct InfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private Status m_Status;

		private IntPtr m_UserId;

		private IntPtr m_ProductId;

		private IntPtr m_ProductVersion;

		private IntPtr m_Platform;

		private IntPtr m_RichText;

		private int m_RecordsCount;

		private IntPtr m_Records;

		private IntPtr m_ProductName;

		public Status Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				m_Status = value;
			}
		}

		public EpicAccountId UserId
		{
			get
			{
				Helper.TryMarshalGet(m_UserId, out EpicAccountId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public string ProductId
		{
			get
			{
				Helper.TryMarshalGet(m_ProductId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ProductId, value);
			}
		}

		public string ProductVersion
		{
			get
			{
				Helper.TryMarshalGet(m_ProductVersion, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ProductVersion, value);
			}
		}

		public string Platform
		{
			get
			{
				Helper.TryMarshalGet(m_Platform, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Platform, value);
			}
		}

		public string RichText
		{
			get
			{
				Helper.TryMarshalGet(m_RichText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_RichText, value);
			}
		}

		public DataRecord[] Records
		{
			get
			{
				Helper.TryMarshalGet<DataRecordInternal, DataRecord>(m_Records, out var target, m_RecordsCount);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<DataRecordInternal, DataRecord>(ref m_Records, value, out m_RecordsCount);
			}
		}

		public string ProductName
		{
			get
			{
				Helper.TryMarshalGet(m_ProductName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ProductName, value);
			}
		}

		public void Set(Info other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				Status = other.Status;
				UserId = other.UserId;
				ProductId = other.ProductId;
				ProductVersion = other.ProductVersion;
				Platform = other.Platform;
				RichText = other.RichText;
				Records = other.Records;
				ProductName = other.ProductName;
			}
		}

		public void Set(object other)
		{
			Set(other as Info);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_UserId);
			Helper.TryMarshalDispose(ref m_ProductId);
			Helper.TryMarshalDispose(ref m_ProductVersion);
			Helper.TryMarshalDispose(ref m_Platform);
			Helper.TryMarshalDispose(ref m_RichText);
			Helper.TryMarshalDispose(ref m_Records);
			Helper.TryMarshalDispose(ref m_ProductName);
		}
	}
}
