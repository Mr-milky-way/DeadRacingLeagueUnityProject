using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class ComponentHandle<T> : MonoBehaviour where T : Component
	{
		[SerializeField]
		private Camera m_camera;

		private Camera m_main;

		public List<GizmoHandle> handles;

		[SerializeField]
		protected List<T> m_targets;

		[SerializeField]
		private GizmoHandleCallback m_callback;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					if (!m_main)
					{
						return m_main = Camera.main;
					}
					return m_main;
				}
				return m_camera;
			}
			set
			{
				m_camera = value;
			}
		}

		public bool moving
		{
			get
			{
				if (!base.gameObject.activeInHierarchy)
				{
					return false;
				}
				for (int i = 0; i < handles.Count; i++)
				{
					if (handles[i].moving)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<T> targets
		{
			get
			{
				return m_targets;
			}
			set
			{
				if (m_targets == null)
				{
					m_targets = new List<T>();
				}
				OnTargetsRemove(m_targets);
				m_targets = new List<T>(value);
				OnTargetsAdd(m_targets);
			}
		}

		public GizmoHandleCallback callback
		{
			get
			{
				if (m_callback != null)
				{
					return m_callback;
				}
				return m_callback = new GizmoHandleCallback();
			}
		}

		protected virtual void Awake()
		{
			if (m_targets == null)
			{
				m_targets = new List<T>();
			}
			if (m_targets.Count > 0)
			{
				OnTargetsAdd(m_targets);
			}
			for (int i = 0; i < handles.Count; i++)
			{
				handles[i].callback.AddListener(HandleEvent);
			}
		}

		protected void HandleEvent(GizmoHandleEvent p_event)
		{
			if (base.enabled)
			{
				OnHandleEvent(p_event);
				callback.Invoke(p_event);
			}
		}

		protected virtual void OnTargetsRemove(List<T> p_list)
		{
		}

		protected virtual void OnTargetsAdd(List<T> p_list)
		{
		}

		protected virtual void OnHandleEvent(GizmoHandleEvent p_event)
		{
		}

		public void SetHandlesMouseEnabled(bool p_flag)
		{
			for (int i = 0; i < handles.Count; i++)
			{
				if ((bool)handles[i])
				{
					handles[i].mouse.enabled = p_flag;
				}
			}
		}

		public void SetHandlesKeyboardEnabled(bool p_flag)
		{
			for (int i = 0; i < handles.Count; i++)
			{
				if ((bool)handles[i])
				{
					handles[i].keyboard.enabled = p_flag;
				}
			}
		}
	}
}
