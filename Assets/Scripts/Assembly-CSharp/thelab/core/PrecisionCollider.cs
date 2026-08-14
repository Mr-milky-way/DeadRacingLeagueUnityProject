using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class PrecisionCollider : MonoBehaviour
	{
		public LayerMask layers;

		[SerializeField]
		private List<PrecisionHit> m_hits;

		public bool dynamicColliders;

		[SerializeField]
		private PrecisionCollisionCallback m_callback;

		private PrecisionCollisionEvent m_event;

		public new bool enabled
		{
			get
			{
				if (base.enabled)
				{
					return base.gameObject.activeInHierarchy;
				}
				return false;
			}
			set
			{
				base.enabled = value;
			}
		}

		public List<PrecisionHit> hits
		{
			get
			{
				if (m_hits != null)
				{
					return m_hits;
				}
				return m_hits = new List<PrecisionHit>();
			}
		}

		public bool hasHits => hits.Count > 0;

		public PrecisionCollisionCallback callback
		{
			get
			{
				if (m_callback != null)
				{
					return m_callback;
				}
				return m_callback = new PrecisionCollisionCallback();
			}
		}

		protected void Start()
		{
		}

		public void Clear()
		{
			hits.Clear();
		}

		public void Dispatch(PrecisionCollisionEventType p_type, params object[] p_args)
		{
			if (callback != null && enabled)
			{
				PrecisionCollisionEvent precisionCollisionEvent = ((m_event == null) ? (m_event = new PrecisionCollisionEvent()) : m_event);
				precisionCollisionEvent.type = p_type;
				precisionCollisionEvent.target = this;
				precisionCollisionEvent.args = p_args;
				callback.Invoke(precisionCollisionEvent);
			}
		}

		protected void OnTriggerEnter(Collider c)
		{
			UpdateCollision(c, p_is_enter: true, p_is_exit: false);
		}

		protected void OnTriggerStay(Collider c)
		{
			UpdateCollision(c, p_is_enter: false, p_is_exit: false);
		}

		protected void OnTriggerExit(Collider c)
		{
			UpdateCollision(c, p_is_enter: false, p_is_exit: true);
		}

		protected void UpdateCollision(Collider p_collider, bool p_is_enter, bool p_is_exit)
		{
			if (!enabled)
			{
				return;
			}
			if (((1 << p_collider.gameObject.layer) & (int)layers) == 0)
			{
				return;
			}
			PrecisionHit precisionHit = null;
			for (int i = 0; i < hits.Count; i++)
			{
				if (((1 << p_collider.gameObject.layer) & (int)layers) == 0)
				{
					hits.RemoveAt(i--);
					continue;
				}
				if (!hits[i].target)
				{
					hits.RemoveAt(i--);
					continue;
				}
				if (!hits[i].rigidbody)
				{
					hits.RemoveAt(i--);
					continue;
				}
				if (hits[i].from == p_collider)
				{
					precisionHit = hits[i];
					break;
				}
				if (hits[i].to == p_collider)
				{
					precisionHit = hits[i];
					break;
				}
			}
			if (p_is_enter && precisionHit == null)
			{
				precisionHit = new PrecisionHit();
				precisionHit.target = this;
				precisionHit.from = p_collider;
				precisionHit.to = base.gameObject.GetComponent<Collider>();
				precisionHit.rigidbody = p_collider.attachedRigidbody;
				if (!precisionHit.rigidbody)
				{
					Collider component = base.gameObject.GetComponent<Collider>();
					if ((bool)component)
					{
						precisionHit.rigidbody = component.attachedRigidbody;
						precisionHit.from = component;
						precisionHit.to = p_collider;
					}
				}
				hits.Add(precisionHit);
			}
			bool num = dynamicColliders || p_is_enter || p_is_exit;
			Collider collider = precisionHit.from;
			Collider to = precisionHit.to;
			Vector3 vector = (p_is_enter ? collider.transform.position : precisionHit.enter);
			Vector3 vector2 = (p_is_exit ? collider.transform.position : precisionHit.exit);
			Vector3 vector3 = (num ? GetColliderWorldCenter(to) : precisionHit.center);
			Vector3 size = (num ? GetColliderWorldSize(to) : precisionHit.size);
			precisionHit.center = vector3;
			precisionHit.size = size;
			if (p_is_enter)
			{
				precisionHit.enter = vector;
			}
			if (p_is_exit)
			{
				precisionHit.exit = vector2;
			}
			if (num)
			{
				precisionHit.center = GetColliderWorldCenter(to);
			}
			Vector3 vector4 = (p_is_exit ? vector : collider.transform.position);
			Vector3 vector5 = (p_is_exit ? vector2 : collider.transform.position);
			Vector3 vector6 = (vector4 + vector5) * 0.5f;
			precisionHit.precision = to.transform.InverseTransformVector(vector6 - vector3);
			precisionHit.normalized = precisionHit.GetNormalized(precisionHit.precision);
			precisionHit.ratio = precisionHit.GetRatio(precisionHit.normalized, p_normalized: true);
			precisionHit.distance = Vector3.Distance(vector6, vector3);
			PrecisionCollisionEventType p_type = PrecisionCollisionEventType.Stay;
			if (p_is_enter)
			{
				p_type = PrecisionCollisionEventType.Enter;
			}
			if (p_is_exit)
			{
				p_type = PrecisionCollisionEventType.Exit;
			}
			if (p_is_exit)
			{
				precisionHit.orientation = precisionHit.GetOrientation();
			}
			Dispatch(p_type, precisionHit);
			if (p_is_exit)
			{
				Debug.DrawLine(vector4, vector5, Color.red, 20f);
				Debug.DrawLine(vector6, vector3, Color.magenta, 20f);
			}
		}

		protected Vector3 GetColliderWorldSize(Collider c)
		{
			Vector3 result = c.transform.localScale;
			Vector3 vector = Vector3.one;
			if (c is BoxCollider)
			{
				vector = ((BoxCollider)c).size;
				result = Vector3.one;
			}
			if (c is SphereCollider)
			{
				SphereCollider sphereCollider = (SphereCollider)c;
				vector *= sphereCollider.radius;
				result = Vector3.one;
			}
			if (c is MeshCollider)
			{
				MeshCollider meshCollider = (MeshCollider)c;
				vector = (meshCollider.sharedMesh ? meshCollider.sharedMesh.bounds.extents : Vector3.zero);
			}
			if (c is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)c;
				vector = new Vector3(capsuleCollider.radius, capsuleCollider.height * 0.5f, capsuleCollider.radius);
				result = Vector3.one;
			}
			result.x *= vector.x;
			result.y *= vector.y;
			result.z *= vector.z;
			return result;
		}

		protected Vector3 GetColliderWorldCenter(Collider c)
		{
			if (c is BoxCollider)
			{
				BoxCollider boxCollider = (BoxCollider)c;
				return boxCollider.transform.TransformPoint(boxCollider.center);
			}
			if (c is SphereCollider)
			{
				SphereCollider sphereCollider = (SphereCollider)c;
				return sphereCollider.transform.TransformPoint(sphereCollider.center);
			}
			if (c is MeshCollider)
			{
				MeshCollider meshCollider = (MeshCollider)c;
				return meshCollider.transform.TransformPoint(meshCollider.sharedMesh ? meshCollider.sharedMesh.bounds.center : Vector3.zero);
			}
			if (c is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)c;
				return capsuleCollider.transform.TransformPoint(capsuleCollider.center);
			}
			return c.transform.position;
		}
	}
}
