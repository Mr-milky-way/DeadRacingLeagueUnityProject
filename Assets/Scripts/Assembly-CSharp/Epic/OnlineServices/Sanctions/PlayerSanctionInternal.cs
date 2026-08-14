using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sanctions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PlayerSanctionInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private long m_TimePlaced;

		private IntPtr m_Action;

		public long TimePlaced
		{
			get
			{
				return m_TimePlaced;
			}
			set
			{
				m_TimePlaced = value;
			}
		}

		public string Action
		{
			get
			{
				Helper.TryMarshalGet(m_Action, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Action, value);
			}
		}

		public void Set(PlayerSanction other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TimePlaced = other.TimePlaced;
				Action = other.Action;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerSanction);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Action);
		}
	}
}
