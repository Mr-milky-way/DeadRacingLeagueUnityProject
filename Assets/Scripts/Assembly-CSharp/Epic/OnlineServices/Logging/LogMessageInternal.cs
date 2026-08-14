using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Logging
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LogMessageInternal
	{
		private IntPtr m_Category;

		private IntPtr m_Message;

		private LogLevel m_Level;

		public string Category
		{
			get
			{
				Helper.TryMarshalGet(m_Category, out string target);
				return target;
			}
		}

		public string Message
		{
			get
			{
				Helper.TryMarshalGet(m_Message, out string target);
				return target;
			}
		}

		public LogLevel Level => m_Level;
	}
}
