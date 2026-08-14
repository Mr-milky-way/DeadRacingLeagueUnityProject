using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityExt.Core.IO;
using thelab.core;

namespace drl.sim
{
	[Serializable]
	public class BlackboxFrame
	{
		[SerializableField]
		public byte type;

		[SerializableField]
		public object[] data;

		[SerializableField]
		public float time;

		private static float[] m_null_thrust = new float[4];

		private object[] m_object_cache;

		[JsonIgnore]
		public bool valid
		{
			get
			{
				if (data != null)
				{
					return data.Length != 0;
				}
				return false;
			}
		}

		public void Init()
		{
			if (data == null)
			{
				data = new object[24];
			}
		}

		public void Prune()
		{
			if (data != null)
			{
				int num = 0;
				for (num = 0; num < data.Length && data[num] != null; num++)
				{
				}
				if (num >= 0)
				{
					Array.Resize(ref data, num);
				}
			}
		}

		public T GetType<T>()
		{
			return Reflection<object>.GetEnum<T>(type);
		}

		public void Set(float p_time, byte p_type, byte v0)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
		}

		public void Set(float p_time, byte p_type, int v0)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
		}

		public void Set(float p_time, byte p_type, float v0)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
		}

		public void Set(float p_time, byte p_type, float v0, float v1)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6)
		{
			if (valid)
			{
				time = p_time;
				type = p_type;
				data[0] = v0;
				data[1] = v1;
				data[2] = v2;
				data[3] = v3;
				data[4] = v4;
				data[5] = v5;
				data[6] = v6;
			}
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
			data[6] = v6;
			data[7] = v7;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
			data[6] = v6;
			data[7] = v7;
			data[8] = v8;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8, float v9)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
			data[6] = v6;
			data[7] = v7;
			data[8] = v8;
			data[9] = v9;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8, float v9, float v10)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
			data[6] = v6;
			data[7] = v7;
			data[8] = v8;
			data[9] = v9;
			data[10] = v10;
		}

		public void Set(float p_time, byte p_type, float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8, float v9, float v10, float v11)
		{
			time = p_time;
			type = p_type;
			data[0] = v0;
			data[1] = v1;
			data[2] = v2;
			data[3] = v3;
			data[4] = v4;
			data[5] = v5;
			data[6] = v6;
			data[7] = v7;
			data[8] = v8;
			data[9] = v9;
			data[10] = v10;
			data[11] = v11;
		}

		public void Set(float p_time, byte p_type, float[] vl)
		{
			time = p_time;
			type = p_type;
			int num = Mathf.Min(vl.Length, data.Length);
			for (int i = 0; i < num; i++)
			{
				data[i] = vl[i];
			}
		}

		public void Set(float p_time, byte p_type, object[] vl)
		{
			time = p_time;
			type = p_type;
			int num = Mathf.Min(vl.Length, data.Length);
			for (int i = 0; i < num; i++)
			{
				data[i] = vl[i];
			}
		}

		public void Set(float p_time, byte p_type, Vector2 p_value)
		{
			Set(p_time, p_type, p_value.x, p_value.y);
		}

		public void Set(float p_time, byte p_type, Vector3 p_value)
		{
			Set(p_time, p_type, p_value.x, p_value.y, p_value.z);
		}

		public void Set(float p_time, byte p_type, Vector4 p_value)
		{
			Set(p_time, p_type, p_value.x, p_value.y, p_value.z, p_value.w);
		}

		public void Set(float p_time, byte p_type, Quaternion p_value)
		{
			Set(p_time, p_type, p_value.x, p_value.y, p_value.z, p_value.w);
		}

		public void Set(float p_time, byte p_type, Vector3 p_position, Quaternion p_rotation)
		{
			Vector3 vector = p_position;
			Quaternion quaternion = p_rotation;
			Set(p_time, p_type, vector.x, vector.y, vector.z, quaternion.x, quaternion.y, quaternion.z, quaternion.w);
		}

		public void Set(float p_time, byte p_type, Vector3 p_dragK, Vector3 p_dragF, float[] p_thrust, float p_torque)
		{
			Vector3 vector = p_dragK;
			Vector3 vector2 = p_dragF;
			float[] array = ((p_thrust == null) ? m_null_thrust : p_thrust);
			Set(p_time, p_type, array[0], array[1], array[2], array[3], p_torque, vector.x, vector.y, vector.z, vector2.x, vector2.y, vector2.z);
		}

		public void Set(float p_time, byte p_type, int p_index, Vector3 p_position, Quaternion p_rotation)
		{
			Set(p_time, p_type, p_index, p_position.x, p_position.y, p_position.z, p_rotation.x, p_rotation.y, p_rotation.z, p_rotation.w);
		}

		public void Set(float p_time, byte p_type, Transform p_value)
		{
			Vector3 position = p_value.position;
			Quaternion rotation = p_value.rotation;
			Set(p_time, p_type, position, rotation);
		}

		public void Set(float p_time, byte p_type, SignalVector v)
		{
			Set(p_time, p_type, v.yaw, v.pitch, v.roll, v.throttle);
		}

		public void Set(float p_time, byte p_type, DroneFlightController v)
		{
			float v2 = 0f;
			float v3 = 0f;
			float v4 = 0f;
			if ((bool)v && (bool)v.gameObject && v.processes != null)
			{
				for (int i = 0; i < v.processes.Count; i++)
				{
					FCProcess fCProcess = v.processes[i];
					if ((bool)fCProcess && (bool)fCProcess.gameObject)
					{
						switch (fCProcess.nameLower)
						{
						case "yaw":
							v2 = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						case "pitch":
							v3 = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						case "roll":
							v4 = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						}
					}
				}
			}
			Set(p_time, p_type, v2, v3, v4);
		}

		private object[] GetObjectFast(object a0 = null, object a1 = null, object a2 = null, object a3 = null, object a4 = null, object a5 = null, object a6 = null, object a7 = null, object a8 = null, object a9 = null, object a10 = null)
		{
			if (m_object_cache == null)
			{
				m_object_cache = new object[24];
			}
			m_object_cache[0] = a0;
			m_object_cache[1] = a1;
			m_object_cache[2] = a2;
			m_object_cache[3] = a3;
			m_object_cache[4] = a4;
			m_object_cache[5] = a5;
			m_object_cache[6] = a6;
			m_object_cache[7] = a7;
			m_object_cache[8] = a8;
			m_object_cache[9] = a9;
			m_object_cache[10] = a10;
			return m_object_cache;
		}

		private object[] GetFloatObjectFast(float[] p_args)
		{
			if (m_object_cache == null)
			{
				m_object_cache = new object[24];
			}
			for (int i = 0; i < p_args.Length; i++)
			{
				m_object_cache[i] = p_args[i];
			}
			return m_object_cache;
		}

		public void GetTransform(out Vector3 p, out Quaternion r)
		{
			p = Vector3.zero;
			r = Quaternion.identity;
			if (data != null)
			{
				p.x = Reflection<object>.Get<float>(data, 0);
				p.y = Reflection<object>.Get<float>(data, 1);
				p.z = Reflection<object>.Get<float>(data, 2);
				r.x = Reflection<object>.Get<float>(data, 3);
				r.y = Reflection<object>.Get<float>(data, 4);
				r.z = Reflection<object>.Get<float>(data, 5);
				r.w = Reflection<object>.Get<float>(data, 6);
			}
		}

		public void GetTransformPart(out int i, out Vector3 p, out Quaternion r)
		{
			p = Vector3.zero;
			r = Quaternion.identity;
			i = -1;
			if (data != null)
			{
				i = Reflection<object>.Get<int>(data, 0);
				p.x = Reflection<object>.Get<float>(data, 1);
				p.y = Reflection<object>.Get<float>(data, 2);
				p.z = Reflection<object>.Get<float>(data, 3);
				r.x = Reflection<object>.Get<float>(data, 4);
				r.y = Reflection<object>.Get<float>(data, 5);
				r.z = Reflection<object>.Get<float>(data, 6);
				r.w = Reflection<object>.Get<float>(data, 7);
			}
		}

		public Vector3 GetVector3()
		{
			Vector3 zero = Vector3.zero;
			if (data == null)
			{
				return zero;
			}
			zero.x = Reflection<object>.Get<float>(data, 0);
			zero.y = Reflection<object>.Get<float>(data, 1);
			zero.z = Reflection<object>.Get<float>(data, 2);
			return zero;
		}

		public Vector4 GetVector4()
		{
			Vector4 result = Vector3.zero;
			if (data == null)
			{
				return result;
			}
			result.x = Reflection<object>.Get<float>(data, 0);
			result.y = Reflection<object>.Get<float>(data, 1);
			result.z = Reflection<object>.Get<float>(data, 2);
			result.w = Reflection<object>.Get<float>(data, 3);
			return result;
		}

		public float[] GetFloats()
		{
			if (data == null)
			{
				return new float[0];
			}
			float[] array = new float[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] != null)
				{
					array[i] = ((data[i] is double) ? ((float)(double)data[i]) : ((float)data[i]));
				}
			}
			return array;
		}

		public void GetPhysics(out Vector3 dk, out Vector3 df, out float[] t, out float to)
		{
			dk = Vector3.zero;
			df = Vector3.zero;
			t = null;
			to = 0f;
			if (data != null)
			{
				t = new float[data.Length - 7];
				int i;
				for (i = 0; i < t.Length; i++)
				{
					t[i] = Reflection<object>.Get<float>(data, i);
				}
				to = Reflection<object>.Get<float>(data, i++);
				dk.x = Reflection<object>.Get<float>(data, i++);
				dk.y = Reflection<object>.Get<float>(data, i++);
				dk.z = Reflection<object>.Get<float>(data, i++);
				df.x = Reflection<object>.Get<float>(data, i++);
				df.y = Reflection<object>.Get<float>(data, i++);
				df.z = Reflection<object>.Get<float>(data, i++);
			}
		}
	}
}
