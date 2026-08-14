using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationSetJoinInProgressAllowedOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_AllowJoinInProgress;

		public bool AllowJoinInProgress
		{
			set
			{
				Helper.TryMarshalSet(ref m_AllowJoinInProgress, value);
			}
		}

		public void Set(SessionModificationSetJoinInProgressAllowedOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AllowJoinInProgress = other.AllowJoinInProgress;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationSetJoinInProgressAllowedOptions);
		}

		public void Dispose()
		{
		}
	}
}
