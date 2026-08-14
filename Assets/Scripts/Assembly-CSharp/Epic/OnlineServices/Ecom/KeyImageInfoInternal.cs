using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct KeyImageInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Type;

		private IntPtr m_Url;

		private uint m_Width;

		private uint m_Height;

		public string Type
		{
			get
			{
				Helper.TryMarshalGet(m_Type, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Type, value);
			}
		}

		public string Url
		{
			get
			{
				Helper.TryMarshalGet(m_Url, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Url, value);
			}
		}

		public uint Width
		{
			get
			{
				return m_Width;
			}
			set
			{
				m_Width = value;
			}
		}

		public uint Height
		{
			get
			{
				return m_Height;
			}
			set
			{
				m_Height = value;
			}
		}

		public void Set(KeyImageInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Type = other.Type;
				Url = other.Url;
				Width = other.Width;
				Height = other.Height;
			}
		}

		public void Set(object other)
		{
			Set(other as KeyImageInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Type);
			Helper.TryMarshalDispose(ref m_Url);
		}
	}
}
