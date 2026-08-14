using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AcknowledgeEventIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private ulong m_UiEventId;

		private Result m_Result;

		public ulong UiEventId
		{
			set
			{
				m_UiEventId = value;
			}
		}

		public Result Result
		{
			set
			{
				m_Result = value;
			}
		}

		public void Set(AcknowledgeEventIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				UiEventId = other.UiEventId;
				Result = other.Result;
			}
		}

		public void Set(object other)
		{
			Set(other as AcknowledgeEventIdOptions);
		}

		public void Dispose()
		{
		}
	}
}
