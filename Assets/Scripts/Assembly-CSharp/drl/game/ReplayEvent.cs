using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class ReplayEvent : SerializedData
	{
		private float m_time = -1f;

		private long m_sample = -1L;

		private int m_type = -1;

		private Vector3 m_position;

		private bool m_has_position;

		private object[] m_data;

		private static object[] m_empty_data = new object[0];

		public float time
		{
			get
			{
				if (m_time >= 0f)
				{
					return m_time;
				}
				return Get("event-time", 0f);
			}
			set
			{
				Set("event-time", value);
			}
		}

		public long sample
		{
			get
			{
				if (m_sample >= 0)
				{
					return m_sample;
				}
				return Get("event-sample", 0L);
			}
			set
			{
				Set("event-sample", value);
			}
		}

		public int type
		{
			get
			{
				if (m_type >= 0)
				{
					return m_type;
				}
				return Get("event-type", 0);
			}
			set
			{
				Set("event-type", value);
			}
		}

		public ReplayEventType typeFlag
		{
			get
			{
				return (ReplayEventType)type;
			}
			set
			{
				type = (int)value;
			}
		}

		public Vector3 position
		{
			get
			{
				if (m_has_position)
				{
					return m_position;
				}
				object obj = Get<object>("event-position", null);
				float[] array = null;
				if (obj is float[])
				{
					array = (float[])obj;
				}
				else if (obj is JArray)
				{
					array = ((JArray)obj).ToObject<float[]>();
				}
				Vector3 result = default(Vector3);
				if (array == null)
				{
					return result;
				}
				result[0] = ((array.Length != 0) ? array[0] : 0f);
				result[1] = ((array.Length > 1) ? array[1] : 0f);
				result[2] = ((array.Length > 2) ? array[2] : 0f);
				return result;
			}
			set
			{
				Vector3 vector = value;
				float[] v = new float[3]
				{
					vector[0],
					vector[1],
					vector[2]
				};
				Set("event-position", v);
			}
		}

		public object[] data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				object obj = Get<object>("event-data", null);
				object[] array = null;
				if (obj is object[])
				{
					array = (object[])obj;
				}
				else if (obj is JArray)
				{
					array = ((JArray)obj).ToObject<object[]>();
				}
				return m_data = ((array == null) ? m_empty_data : array);
			}
			set
			{
				Set("event-data", value);
			}
		}

		public void Init()
		{
			m_time = Get("event-time", 0f);
			m_sample = Get("event-sample", 0L);
			m_type = Get("event-type", 0);
			m_position = position;
			m_has_position = true;
		}
	}
}
