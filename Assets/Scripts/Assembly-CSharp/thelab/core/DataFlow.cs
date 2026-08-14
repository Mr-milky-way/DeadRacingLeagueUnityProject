using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class DataFlow : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		internal byte[] m_data_d;

		internal Dictionary<string, object> m_data;

		public Dictionary<string, object> data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				byte[] array = ((m_data_d == null) ? new byte[0] : m_data_d);
				m_data = ((array.Length == 0) ? new Dictionary<string, object>() : Serialize.FromBytes<Dictionary<string, object>>(array));
				if (m_data == null)
				{
					m_data = new Dictionary<string, object>();
				}
				return m_data;
			}
			set
			{
				m_data = value;
				if (m_data == null)
				{
					m_data = new Dictionary<string, object>();
				}
				m_data_d = Serialize.ToBytes(m_data);
			}
		}

		public Dictionary<string, string> hash
		{
			get
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				Dictionary<string, object> dictionary2 = data;
				if (dictionary2 == null)
				{
					return dictionary;
				}
				foreach (KeyValuePair<string, object> item in dictionary2)
				{
					string key = item.Key;
					object value = item.Value;
					if (value != null)
					{
						dictionary[key] = value.ToString();
					}
				}
				return dictionary;
			}
			set
			{
				Dictionary<string, string> addHash = ((value == null) ? new Dictionary<string, string>() : value);
				Dictionary<string, object> dictionary = data;
				dictionary.Clear();
				IDictionaryMerge(dictionary, addHash);
				data = dictionary;
			}
		}

		internal void LogData()
		{
			Debug.Log("DataFlow> Set Dictionary [" + m_data?.ToString() + "]");
			if (m_data == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> datum in m_data)
			{
				Debug.Log("data-flow kv = [" + datum.Key + "," + datum.Value?.ToString() + "]");
			}
		}

		internal string ToHashString()
		{
			string text = "";
			if (m_data != null)
			{
				foreach (KeyValuePair<string, object> datum in m_data)
				{
					text = text + "data-flow kv = [" + datum.Key + "," + datum.Value?.ToString() + "]\n";
				}
			}
			return text;
		}

		public Dictionary<string, string> GetHash(IList<string> p_keys)
		{
			if (p_keys == null)
			{
				return hash;
			}
			if (p_keys.Count <= 0)
			{
				return hash;
			}
			Dictionary<string, object> dictionary = data;
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			for (int i = 0; i < p_keys.Count; i++)
			{
				string key = p_keys[i];
				if (dictionary.ContainsKey(key))
				{
					object obj = dictionary[key];
					if (obj != null)
					{
						dictionary2[key] = obj.ToString();
					}
				}
			}
			return dictionary2;
		}

		public void Merge(IDictionary p_data)
		{
			if (p_data != null)
			{
				Dictionary<string, object> target = data;
				IDictionaryMerge(target, p_data);
				data = target;
			}
		}

		public void IDictionaryMerge(IDictionary target, IDictionary addHash, bool p_force_string = false)
		{
			if (addHash == null || target.Equals(addHash))
			{
				return;
			}
			foreach (object key in addHash.Keys)
			{
				object obj = addHash[key];
				target[key] = (p_force_string ? ((obj == null) ? "" : obj.ToString()) : obj);
			}
		}

		public bool Contains(string k)
		{
			if (data != null)
			{
				return data.ContainsKey(k);
			}
			return false;
		}

		public T Get<T>(string k)
		{
			return Get(k, default(T));
		}

		public T Get<T>(string k, T d)
		{
			if (data == null)
			{
				return d;
			}
			if (data.ContainsKey(k))
			{
				return Reflection<object>.AssertCast<T>(data[k]);
			}
			return d;
		}

		public object Get(string k)
		{
			return Get<object>(k);
		}

		public object Set(string k, object v)
		{
			data[k] = v;
			return v;
		}

		public void SetInt(string k, int v)
		{
			Set(k, v);
		}

		public void SetLong(string k, long v)
		{
			Set(k, v);
		}

		public void SetFloat(string k, float v)
		{
			Set(k, v);
		}

		public void SetVector2(string k, Vector2 v)
		{
			Set(k, v);
		}

		public void SetVector3(string k, Vector3 v)
		{
			Set(k, v);
		}

		public void SetVector4(string k, Vector4 v)
		{
			Set(k, v);
		}

		public void SetQuaternion(string k, Quaternion v)
		{
			Set(k, v);
		}

		public void SetBool(string k, bool v)
		{
			Set(k, v);
		}

		public void SetColor(string k, Color v)
		{
			Set(k, v);
		}

		public void SetRect(string k, Rect v)
		{
			Set(k, v);
		}

		public void SetBounds(string k, Bounds v)
		{
			Set(k, v);
		}

		public void SetAnimationCurve(string k, AnimationCurve v)
		{
			Set(k, v);
		}

		public void SetObject(string k, Object v)
		{
			Set(k, v);
		}

		public void SetString(string k, string v)
		{
			Set(k, v);
		}

		public void SetField(string k, Object t, string f)
		{
			f = f.Replace("[", ".[");
			f = f.Replace("@", ".@");
			object v = Reflection<object>.Traverse<object>(t, f);
			Set(k, v);
		}

		public void SetField(string k, string f)
		{
			SetField(k, null, f);
		}

		public void Add(string k, float dt)
		{
			float num = Get<float>(k);
			Set(k, num + dt);
		}

		public void Add(string k, int dt)
		{
			int num = Get<int>(k);
			Set(k, num + dt);
		}

		public bool Remove(string k)
		{
			if (data == null)
			{
				return false;
			}
			return data.Remove(k);
		}
	}
}
