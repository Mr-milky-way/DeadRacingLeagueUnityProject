using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Reports
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SendPlayerBehaviorReportOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ReporterUserId;

		private IntPtr m_ReportedUserId;

		private PlayerReportsCategory m_Category;

		private IntPtr m_Message;

		private IntPtr m_Context;

		public ProductUserId ReporterUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ReporterUserId, value);
			}
		}

		public ProductUserId ReportedUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ReportedUserId, value);
			}
		}

		public PlayerReportsCategory Category
		{
			set
			{
				m_Category = value;
			}
		}

		public string Message
		{
			set
			{
				Helper.TryMarshalSet(ref m_Message, value);
			}
		}

		public string Context
		{
			set
			{
				Helper.TryMarshalSet(ref m_Context, value);
			}
		}

		public void Set(SendPlayerBehaviorReportOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				ReporterUserId = other.ReporterUserId;
				ReportedUserId = other.ReportedUserId;
				Category = other.Category;
				Message = other.Message;
				Context = other.Context;
			}
		}

		public void Set(object other)
		{
			Set(other as SendPlayerBehaviorReportOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ReporterUserId);
			Helper.TryMarshalDispose(ref m_ReportedUserId);
			Helper.TryMarshalDispose(ref m_Message);
			Helper.TryMarshalDispose(ref m_Context);
		}
	}
}
