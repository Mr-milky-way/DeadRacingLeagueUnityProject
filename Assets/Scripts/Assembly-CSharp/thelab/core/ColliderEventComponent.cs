using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class ColliderEventComponent : MonoBehaviour
	{
		public class Data
		{
			public Collision enter;

			public Collision stay;

			public Collision exit;
		}

		public bool hit;

		public float elapsed;

		public Data data;

		public ColliderEvent.Type mask;

		[SerializeField]
		private List<Collider> m_colliders;

		[SerializeField]
		private ColliderEventCallback m_callback;

		private ColliderEvent m_evcache;

		public List<Collider> colliders
		{
			get
			{
				if (m_colliders != null)
				{
					return m_colliders;
				}
				return m_colliders = new List<Collider>();
			}
		}

		public ColliderEventCallback callback
		{
			get
			{
				if (m_callback != null)
				{
					return m_callback;
				}
				return m_callback = new ColliderEventCallback();
			}
		}

		protected void Awake()
		{
			if (colliders.Count <= 0)
			{
				colliders.AddRange(GetComponents<Collider>());
			}
			m_evcache = new ColliderEvent();
			data = new Data();
		}

		public void Trigger(Collider p_target)
		{
			if (!p_target || !this || !base.gameObject)
			{
				return;
			}
			for (int i = 0; i < colliders.Count; i++)
			{
				if ((bool)colliders[i] && colliders[i].isTrigger)
				{
					OnTriggerEnter(p_target);
					OnTriggerExit(p_target);
					break;
				}
			}
		}

		protected void Start()
		{
		}

		protected virtual void OnEnter(Collider p_target, Collision p_data)
		{
			if (mask == (ColliderEvent.Type)0 || (mask & ColliderEvent.Type.Enter) != 0)
			{
				elapsed = 0f;
				hit = true;
				data.enter = p_data;
				Invoke(null, ColliderEvent.Type.Enter, p_target, p_data);
			}
		}

		protected virtual void OnExit(Collider p_target, Collision p_data)
		{
			if (mask == (ColliderEvent.Type)0 || (mask & ColliderEvent.Type.Exit) != 0)
			{
				data.exit = p_data;
				Invoke(null, ColliderEvent.Type.Exit, p_target, p_data);
				hit = false;
				elapsed = 0f;
			}
		}

		protected virtual void OnStay(Collider p_target, Collision p_data)
		{
			if ((mask == (ColliderEvent.Type)0 || (mask & ColliderEvent.Type.Stay) != 0) && m_evcache != null)
			{
				data.stay = p_data;
				Invoke(m_evcache, ColliderEvent.Type.Stay, p_target, p_data);
				elapsed += Time.deltaTime;
			}
		}

		private void OnTriggerEnter(Collider p_target)
		{
			OnEnter(p_target, null);
		}

		private void OnTriggerExit(Collider p_target)
		{
			OnExit(p_target, null);
		}

		private void OnTriggerStay(Collider p_target)
		{
			OnStay(p_target, null);
		}

		private void OnCollisionEnter(Collision p_target)
		{
			OnEnter(p_target.collider, p_target);
		}

		private void OnCollisionExit(Collision p_target)
		{
			OnExit(p_target.collider, p_target);
		}

		private void OnCollisionStay(Collision p_target)
		{
			OnStay(p_target.collider, p_target);
		}

		private void Invoke(ColliderEvent p_event, ColliderEvent.Type p_type, Collider p_hit, Collision p_data)
		{
			if (base.enabled)
			{
				ColliderEvent colliderEvent = ((p_event == null) ? new ColliderEvent() : p_event);
				colliderEvent.type = p_type;
				colliderEvent.target = this;
				colliderEvent.collider = p_hit;
				colliderEvent.data = p_data;
				colliderEvent.trigger = p_data == null;
				if (p_type == ColliderEvent.Type.Enter)
				{
					colliderEvent.hitEnter = p_hit.transform.position;
				}
				if (p_type == ColliderEvent.Type.Exit)
				{
					colliderEvent.hitExit = p_hit.transform.position;
				}
				if (callback != null)
				{
					callback.Invoke(colliderEvent);
				}
			}
		}
	}
}
