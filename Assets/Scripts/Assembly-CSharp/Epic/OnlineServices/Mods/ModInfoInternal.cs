using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Mods
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ModInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_ModsCount;

		private IntPtr m_Mods;

		private ModEnumerationType m_Type;

		public ModIdentifier[] Mods
		{
			get
			{
				Helper.TryMarshalGet<ModIdentifierInternal, ModIdentifier>(m_Mods, out var target, m_ModsCount);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<ModIdentifierInternal, ModIdentifier>(ref m_Mods, value, out m_ModsCount);
			}
		}

		public ModEnumerationType Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public void Set(ModInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Mods = other.Mods;
				Type = other.Type;
			}
		}

		public void Set(object other)
		{
			Set(other as ModInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Mods);
		}
	}
}
