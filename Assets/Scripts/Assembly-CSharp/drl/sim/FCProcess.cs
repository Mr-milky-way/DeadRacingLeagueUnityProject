using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class FCProcess : MonoBehaviour
	{
		private string m_name_cache;

		private string m_namelower_cache;

		[SerializeField]
		private DroneFlightController m_fc;

		protected bool m_attached;

		public PID[] pids;

		[HideInInspector]
		public float deltaTime;

		private bool m_enabled;

		[SerializeField]
		private float[] m_signals;

		public new string name
		{
			get
			{
				if (!string.IsNullOrEmpty(m_name_cache))
				{
					return m_name_cache;
				}
				return m_name_cache = base.name;
			}
		}

		public string nameLower
		{
			get
			{
				if (!string.IsNullOrEmpty(m_namelower_cache))
				{
					return m_namelower_cache;
				}
				return m_namelower_cache = base.name.ToLower();
			}
		}

		public DroneFlightController fc
		{
			get
			{
				if (m_attached)
				{
					return m_fc;
				}
				if ((bool)m_fc)
				{
					m_attached = true;
					return m_fc;
				}
				m_fc = Hierarchy.FindReverse<DroneFlightController>(base.transform);
				if ((bool)m_fc)
				{
					m_attached = true;
					return m_fc;
				}
				return null;
			}
			set
			{
				m_fc = value;
				m_attached = m_fc != null;
			}
		}

		public bool attached => m_attached;

		public PID pid
		{
			get
			{
				if (pids.Length != 0)
				{
					return pids[0];
				}
				return null;
			}
		}

		public new bool enabled
		{
			get
			{
				return m_enabled;
			}
			set
			{
				base.enabled = (m_enabled = value);
				base.gameObject.SetActive(value);
			}
		}

		public virtual float[] signals
		{
			get
			{
				if (m_signals == null)
				{
					m_signals = new float[4];
				}
				if (m_signals.Length < 4)
				{
					m_signals = new float[4];
				}
				return m_signals;
			}
		}

		private void Start()
		{
			m_enabled = base.enabled && base.gameObject.activeInHierarchy;
		}

		public virtual void Boot()
		{
		}

		public virtual void Reset()
		{
			for (int i = 0; i < pids.Length; i++)
			{
				pids[i].Reset();
			}
			float[] array = m_signals;
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = 0f;
				}
			}
		}

		public void Loop(float p_dt)
		{
			if (enabled)
			{
				deltaTime = p_dt;
				OnUpdate();
				for (int i = 0; i < pids.Length; i++)
				{
					OnPIDUpdate(pids[i]);
				}
			}
		}

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnPIDUpdate(PID p_pid)
		{
		}

		public virtual void SetLayout(FrameLayoutType p_type)
		{
		}
	}
}
