using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public struct ActionEventData
	{
		public Vector4 @event;

		private object[] m_data;

		public object[] data
		{
			get
			{
				return m_data;
			}
			set
			{
				if (value == null)
				{
					m_data = null;
					return;
				}
				int num = value.Length;
				if (m_data == null)
				{
					m_data = new object[num];
				}
				if (m_data.Length != num)
				{
					m_data = new object[num];
				}
				for (int i = 0; i < num; i++)
				{
					m_data[i] = value[i];
				}
			}
		}

		public int actionIndex
		{
			get
			{
				if (data == null)
				{
					return -1;
				}
				if (data.Length == 0)
				{
					return -1;
				}
				if (!(data[0] is int))
				{
					return -1;
				}
				return (int)data[0];
			}
		}
	}
}
