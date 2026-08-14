using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationAddAttributeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Attribute;

		private LobbyAttributeVisibility m_Visibility;

		public AttributeData Attribute
		{
			set
			{
				Helper.TryMarshalSet<AttributeDataInternal, AttributeData>(ref m_Attribute, value);
			}
		}

		public LobbyAttributeVisibility Visibility
		{
			set
			{
				m_Visibility = value;
			}
		}

		public void Set(LobbyModificationAddAttributeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Attribute = other.Attribute;
				Visibility = other.Visibility;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationAddAttributeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Attribute);
		}
	}
}
