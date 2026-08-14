using System;
using System.Text;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	[Serializable]
	public class NotificationData
	{
		public string id;

		public NotificationTypeFlag type;

		public float ttl;

		public float ttlElapsed;

		public DateTime timestamp;

		private SerializedData m_data;

		public string message;

		public bool error;

		public float ttlRatio
		{
			get
			{
				if (!(ttl <= 0f))
				{
					return Mathf.Clamp01(ttlElapsed / ttl);
				}
				return 0f;
			}
		}

		public bool ttlComplete
		{
			get
			{
				if (!(ttl <= 0f))
				{
					return ttlElapsed >= ttl;
				}
				return false;
			}
		}

		public SerializedData data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				return m_data = new SerializedData();
			}
		}

		public NotificationData(NotificationTypeFlag p_flag, float p_ttl)
		{
			id = GUID.Create(16, "", 200, 0, 15, "x1");
			type = p_flag;
			timestamp = DateTime.Now;
			ttlElapsed = 0f;
			ttl = p_ttl;
		}

		public NotificationData()
			: this(NotificationTypeFlag.None, 0f)
		{
		}

		public NotificationData(NotificationData p_data)
		{
			if (p_data != null)
			{
				type = p_data.type;
				data.Merge(p_data.data);
			}
		}

		public T Get<T>(string p_key, T p_default)
		{
			return data.Get(p_key, p_default);
		}

		public T Get<T>(string p_key)
		{
			return data.Get<T>(p_key);
		}

		public void Set(string p_key, object p_value)
		{
			data.Set(p_key, p_value);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("id:   " + id);
			stringBuilder.AppendLine("type: " + type);
			stringBuilder.AppendLine(data.ToJson(p_indented: true));
			return stringBuilder.ToString();
		}

		public virtual void OnUpdate()
		{
		}

		internal void Update()
		{
			OnUpdate();
			UpdateTTL();
		}

		internal bool UpdateTTL()
		{
			if (ttl <= 0f)
			{
				return true;
			}
			ttlElapsed += Time.unscaledDeltaTime;
			if (ttlElapsed >= ttl)
			{
				ttlElapsed = ttl;
			}
			return ttlElapsed < ttl;
		}
	}
}
