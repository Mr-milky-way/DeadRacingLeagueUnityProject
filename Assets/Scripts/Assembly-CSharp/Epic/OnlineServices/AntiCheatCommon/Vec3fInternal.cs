using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatCommon
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct Vec3fInternal : ISettable, IDisposable
	{
		private float m_x;

		private float m_y;

		private float m_z;

		public float x
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
			}
		}

		public float y
		{
			get
			{
				return m_y;
			}
			set
			{
				m_y = value;
			}
		}

		public float z
		{
			get
			{
				return m_z;
			}
			set
			{
				m_z = value;
			}
		}

		public void Set(Vec3f other)
		{
			if (other != null)
			{
				x = other.x;
				y = other.y;
				z = other.z;
			}
		}

		public void Set(object other)
		{
			Set(other as Vec3f);
		}

		public void Dispose()
		{
		}
	}
}
