using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class MEActionData
	{
		public string id;

		public MEActionType type;

		[SerializeField]
		private Dictionary<string, object> m_data;

		public Dictionary<string, object> data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				return m_data = new Dictionary<string, object>();
			}
			set
			{
				m_data = ((value == null) ? new Dictionary<string, object>() : m_data);
			}
		}

		public void Set(string p_key, object p_value)
		{
			data[p_key] = p_value;
		}

		public T Get<T>(string p_key, T p_default)
		{
			if (!data.ContainsKey(p_key))
			{
				return p_default;
			}
			return (T)data[p_key];
		}

		public T Get<T>(string p_key)
		{
			object obj = default(T);
			if (typeof(T) == typeof(string[]))
			{
				obj = new string[0];
			}
			return Get(p_key, (T)obj);
		}
	}
}
