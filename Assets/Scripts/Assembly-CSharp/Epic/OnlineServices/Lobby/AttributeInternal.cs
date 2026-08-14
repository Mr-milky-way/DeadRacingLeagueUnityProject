using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AttributeInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Data;

		private LobbyAttributeVisibility m_Visibility;

		public AttributeData Data
		{
			get
			{
				Helper.TryMarshalGet<AttributeDataInternal, AttributeData>(m_Data, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<AttributeDataInternal, AttributeData>(ref m_Data, value);
			}
		}

		public LobbyAttributeVisibility Visibility
		{
			get
			{
				return m_Visibility;
			}
			set
			{
				m_Visibility = value;
			}
		}

		public void Set(Attribute other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Data = other.Data;
				Visibility = other.Visibility;
			}
		}

		public void Set(object other)
		{
			Set(other as Attribute);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}
