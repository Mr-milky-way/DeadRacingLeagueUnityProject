using UnityEngine;

namespace drl.sim
{
	public class BacktraceCollider : MonoBehaviour
	{
		[SerializeField]
		private Collider m_collider;

		[SerializeField]
		private Rigidbody m_rigidbody;

		public LayerMask collisionLayer;

		public LayerMask triggerLayer;

		public bool allowTrigger = true;

		public bool allowCollision;

		private Vector3 m_lastPosition;

		private Vector3 m_lastFixedPosition;

		private float m_raycastDistanceThreshold = 0.0002f;

		private float m_raycastCollisionBounceStrength = 0.2f;

		private RaycastHit[] hits;

		public Collider collider
		{
			get
			{
				if (!m_collider)
				{
					m_collider = GetComponent<Collider>();
					if (!m_collider)
					{
						m_collider = GetComponentInChildren<Collider>();
					}
				}
				return m_collider;
			}
		}

		public Rigidbody rigidbody
		{
			get
			{
				if (!m_rigidbody)
				{
					return m_rigidbody = GetComponent<Rigidbody>();
				}
				return m_rigidbody;
			}
		}

		public Vector3 position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
				ResetBacktrace();
			}
		}

		private void Start()
		{
			ResetBacktrace();
		}

		public void ResetBacktrace()
		{
			m_lastPosition = position;
			m_lastFixedPosition = position;
		}

		private void FixedUpdate()
		{
			if (base.enabled && allowTrigger)
			{
				BackTraceTriggers();
			}
			if (allowCollision)
			{
				BackTraceCollisions();
			}
		}

		private void Update()
		{
			if (base.enabled && allowTrigger)
			{
				BackTraceTriggers();
			}
		}

		protected void BackTraceTriggers()
		{
			if (m_lastPosition != position)
			{
				Vector3 vector = position - m_lastPosition;
				float magnitude = vector.magnitude;
				if (magnitude > m_raycastDistanceThreshold)
				{
					if (hits == null || hits.Length < 20)
					{
						hits = new RaycastHit[20];
					}
					int num = Physics.RaycastNonAlloc(m_lastPosition, vector.normalized, hits, magnitude, triggerLayer, QueryTriggerInteraction.Collide);
					for (int i = 0; i < hits.Length && i < num; i++)
					{
						RaycastHit raycastHit = hits[i];
						if (!raycastHit.transform.IsChildOf(base.transform) && raycastHit.collider.isTrigger)
						{
							raycastHit.collider.SendMessage("OnTriggerEnter", collider);
							raycastHit.collider.SendMessage("OnTriggerExit", collider);
						}
					}
				}
			}
			m_lastPosition = position;
		}

		protected void BackTraceCollisions()
		{
			if (m_lastFixedPosition != position)
			{
				Vector3 lhs = position - m_lastFixedPosition;
				float magnitude = lhs.magnitude;
				if (magnitude > m_raycastDistanceThreshold)
				{
					if (hits == null || hits.Length < 20)
					{
						hits = new RaycastHit[20];
					}
					int num = Physics.RaycastNonAlloc(m_lastFixedPosition, lhs.normalized, hits, magnitude, collisionLayer, QueryTriggerInteraction.Ignore);
					for (int i = 0; i < hits.Length && i < num; i++)
					{
						RaycastHit raycastHit = hits[i];
						if (!raycastHit.transform.IsChildOf(base.transform) && !raycastHit.collider.isTrigger && Vector3.Dot(lhs, raycastHit.normal) < 0f)
						{
							raycastHit.collider.SendMessage("OnCollisionEnter", new Collision());
							raycastHit.collider.SendMessage("OnCollisionExit", new Collision());
							Vector3 velocity = Vector3.Reflect(rigidbody.velocity, raycastHit.normal) * m_raycastCollisionBounceStrength;
							position = raycastHit.point - lhs.normalized * m_raycastDistanceThreshold * 5f;
							rigidbody.position = position;
							rigidbody.velocity = velocity;
							break;
						}
					}
				}
			}
			m_lastFixedPosition = position;
		}
	}
}
