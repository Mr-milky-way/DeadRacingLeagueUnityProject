using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class FNTriggerElement
	{
		public FNTriggerType type;

		public bool completed;

		[SerializeField]
		internal bool m_on;

		public float elapsed;

		public float timeout;

		private bool m_initialized;

		public bool on
		{
			get
			{
				return m_on;
			}
			set
			{
				bool flag = m_on;
				m_on = value;
				if (flag != m_on)
				{
					if (flag && !m_on && type == FNTriggerType.SwitchOff)
					{
						completed = true;
					}
					if (!flag && m_on && type == FNTriggerType.SwitchOn)
					{
						completed = true;
					}
				}
			}
		}

		internal virtual void Initialize()
		{
			if (!m_initialized)
			{
				m_initialized = true;
				elapsed = 0f;
				m_on = IsOn();
				completed = false;
			}
		}

		protected virtual bool IsOn()
		{
			return true;
		}

		public virtual void Reset()
		{
			m_on = IsOn();
			completed = false;
			elapsed = 0f;
			m_initialized = false;
		}

		internal virtual void Update()
		{
			if (completed)
			{
				return;
			}
			if (!m_initialized)
			{
				Initialize();
			}
			on = IsOn();
			float num = ((type == FNTriggerType.On || type == FNTriggerType.Off) ? timeout : 0f);
			bool flag = ((type != FNTriggerType.Off && type != FNTriggerType.SwitchOff) ? true : false);
			if (on != flag)
			{
				elapsed = 0f;
			}
			switch (type)
			{
			case FNTriggerType.On:
				elapsed += (on ? Time.deltaTime : 0f);
				if (elapsed >= num && flag == on)
				{
					completed = true;
				}
				break;
			case FNTriggerType.Off:
				elapsed += (on ? 0f : Time.deltaTime);
				if (elapsed >= num && flag == on)
				{
					completed = true;
				}
				break;
			}
		}
	}
}
