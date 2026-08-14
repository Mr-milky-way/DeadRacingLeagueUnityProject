using System;
using UnityEngine;

namespace thelab.core
{
	public class OrbitTransform : MonoBehaviour
	{
		public enum Transition
		{
			DistanceSnap = 1,
			DistanceLerp = 2,
			DistanceMove = 4,
			DistanceLock = 8,
			AngleSnap = 0x10,
			AngleLerp = 0x20,
			AngleMove = 0x40,
			AngleLock = 0x80,
			AnchorSnap = 0x100,
			AnchorLerp = 0x200,
			AnchorMove = 0x400,
			AnchorLock = 0x800,
			AnchorRotationSnap = 0x1000,
			AnchorRotationLerp = 0x2000,
			AnchorRotationMove = 0x4000,
			AnchorRotationLock = 0x8000
		}

		public enum TransitionMask
		{
			None = 0,
			DistanceMask = 7,
			AngleMask = 112,
			AnchorMask = 1792,
			AnchorRotationMask = 28672,
			Snap = 4369,
			Lerp = 8738,
			Move = 17476,
			SmoothTPV = 8465
		}

		[Serializable]
		public struct Data
		{
			public float distance;

			public float angle;

			public float anchor;

			public float rotation;
		}

		[SerializeField]
		private float m_distance;

		[SerializeField]
		private float m_next_distance;

		[SerializeField]
		private Vector3 m_anchor;

		[SerializeField]
		private Vector3 m_next_anchor;

		[SerializeField]
		private Vector2 m_angle;

		[SerializeField]
		private Vector2 m_next_angle;

		public float anglePrecision = 50f;

		[SerializeField]
		private Quaternion m_anchorRotation;

		[SerializeField]
		private Quaternion m_next_anchorRotation;

		private OrbitConstraint m_constraint;

		public Transition transition;

		public Data speed;

		public bool allowTimeScale;

		[SerializeField]
		[HideInInspector]
		internal bool m_init;

		[SerializeField]
		[HideInInspector]
		internal bool m_dirty;

		internal Rigidbody m_rb;

		public float distance
		{
			get
			{
				return m_distance;
			}
			set
			{
				m_next_distance = value;
				Refresh();
			}
		}

		public Vector3 anchor
		{
			get
			{
				return m_anchor;
			}
			set
			{
				m_next_anchor = value;
				Refresh();
			}
		}

		public Vector3 localAnchor
		{
			get
			{
				if (!base.transform.parent)
				{
					return anchor;
				}
				return base.transform.parent.InverseTransformPoint(anchor);
			}
			set
			{
				Vector3 vector = value;
				vector = (base.transform.parent ? base.transform.parent.TransformPoint(vector) : vector);
				anchor = vector;
			}
		}

		public Vector2 angle
		{
			get
			{
				return m_angle;
			}
			set
			{
				m_next_angle = value;
				Refresh();
			}
		}

		public Quaternion anchorRotation
		{
			get
			{
				return m_anchorRotation;
			}
			set
			{
				m_next_anchorRotation = value;
				Refresh();
			}
		}

		public Vector3 anchorEulerAngles
		{
			get
			{
				return m_anchorRotation.eulerAngles;
			}
			set
			{
				anchorRotation = Quaternion.Euler(value);
				Refresh();
			}
		}

		public OrbitConstraint constraint
		{
			get
			{
				if ((bool)m_constraint)
				{
					return m_constraint;
				}
				m_constraint = GetComponent<OrbitConstraint>();
				if ((bool)m_constraint)
				{
					return m_constraint;
				}
				return m_constraint = base.gameObject.AddComponent<OrbitConstraint>();
			}
		}

		public Rigidbody rigidbody => m_rb;

		public bool hasPhysics
		{
			get
			{
				if ((bool)m_rb)
				{
					return !m_rb.isKinematic;
				}
				return false;
			}
		}

		public float GetDistance()
		{
			return m_next_distance;
		}

		internal void Init()
		{
			if (!m_init)
			{
				m_init = true;
				m_anchor = base.transform.position;
				m_anchorRotation = base.transform.localRotation;
				m_distance = 1f;
				m_angle = Vector2.zero;
				m_dirty = true;
				SetTransitionMask(TransitionMask.Snap);
				speed = default(Data);
				speed.distance = 1f;
				speed.angle = 1f;
				speed.distance = 1f;
				speed.anchor = 1f;
				speed.rotation = 1f;
				Refresh();
			}
		}

		protected virtual void Awake()
		{
			m_constraint = GetComponent<OrbitConstraint>();
			m_rb = GetComponent<Rigidbody>();
			Refresh();
		}

		public void Snap(bool p_position = true, bool p_angle = true)
		{
			Transition transition = this.transition;
			SetTransitionMask(TransitionMask.Snap);
			if (p_angle)
			{
				m_anchorRotation = (m_next_anchorRotation = base.transform.localRotation);
				m_angle = (m_next_angle = Vector2.zero);
			}
			if (p_position)
			{
				m_next_distance = m_distance;
				m_anchor = (m_next_anchor = base.transform.position - base.transform.forward * (0f - m_distance));
			}
			this.transition = transition;
		}

		public void SnapPhysics()
		{
			if ((bool)m_rb)
			{
				m_rb.position = m_anchor;
				m_rb.velocity = Vector3.zero;
			}
		}

		public void SetDistanceSnap(float p_distance)
		{
			Vector3 position = base.transform.position;
			m_distance = p_distance;
			m_next_distance = p_distance;
			m_next_anchor = position + base.transform.forward * p_distance;
			m_anchor = m_next_anchor;
		}

		public void ClampCurrentAngle()
		{
			float y = m_angle.y;
			y %= 360f;
			float x = m_angle.x;
			x %= 360f;
			m_angle = new Vector2(x, y);
			UpdateRotation();
		}

		public void StopTransition(TransitionMask p_mask)
		{
			if ((p_mask & TransitionMask.AnchorMask) != TransitionMask.None)
			{
				m_next_anchor = m_anchor;
			}
			if ((p_mask & TransitionMask.AnchorRotationMask) != TransitionMask.None)
			{
				m_next_anchorRotation = m_anchorRotation;
			}
			if ((p_mask & TransitionMask.AngleMask) != TransitionMask.None)
			{
				m_next_angle = m_angle;
			}
			if ((p_mask & TransitionMask.DistanceMask) != TransitionMask.None)
			{
				m_next_distance = m_distance;
			}
		}

		public bool IsTransitionEnabled(Transition p_flag)
		{
			return (p_flag & transition) != 0;
		}

		public void SetTransitionMask(TransitionMask p_mask)
		{
			transition = (Transition)p_mask;
		}

		public void SetTransition(Transition p_transition)
		{
			transition = p_transition;
		}

		public string ToTransitionString()
		{
			int num = 1;
			string text = "Transition Flags\n";
			for (int i = 0; i < 16; i++)
			{
				Transition p_flag = (Transition)num;
				text = text + "[" + num + "] " + p_flag.ToString() + " [" + IsTransitionEnabled(p_flag) + "]";
				text += "\n";
				num <<= 1;
			}
			return text;
		}

		public void Refresh()
		{
			m_dirty = true;
			ApplyRefresh(p_force: true);
		}

		protected void ApplyRefresh(bool p_force = false)
		{
			if (!m_dirty)
			{
				return;
			}
			Transition transition = this.transition;
			if (!Application.isPlaying)
			{
				transition = (Transition)4369;
			}
			m_dirty = false;
			Transition transition2 = (Transition)0;
			bool flag = p_force;
			bool flag2 = p_force;
			float num = (allowTimeScale ? Time.deltaTime : Time.unscaledDeltaTime);
			if ((bool)m_constraint && m_constraint.enabled)
			{
				m_next_distance = Mathf.Clamp(m_next_distance, m_constraint.distanceMin, m_constraint.distanceMax);
			}
			Transition transition3 = (Transition)7;
			transition2 = transition & transition3;
			if (IsDirty(m_distance, m_next_distance, 0.0005f))
			{
				flag = true;
			}
			else
			{
				m_distance = m_next_distance;
			}
			switch (transition2)
			{
			case Transition.DistanceSnap:
				m_distance = m_next_distance;
				break;
			case Transition.DistanceLerp:
				m_distance = Mathf.Lerp(m_distance, m_next_distance, num * speed.distance);
				break;
			case Transition.DistanceMove:
				m_distance = Mathf.MoveTowards(m_distance, m_next_distance, num * speed.distance);
				break;
			}
			if ((bool)m_constraint && m_constraint.enabled)
			{
				if (m_constraint.useAngleXConstraint)
				{
					m_next_angle.x = Mathf.Clamp(m_next_angle.x, m_constraint.angleMin.x, m_constraint.angleMax.x);
				}
				if (m_constraint.useAngleYConstraint)
				{
					m_next_angle.y = Mathf.Clamp(m_next_angle.y, m_constraint.angleMin.y, m_constraint.angleMax.y);
				}
			}
			transition3 = (Transition)112;
			transition2 = transition & transition3;
			if (IsDirty(m_angle, m_next_angle, 0.1f))
			{
				flag2 = true;
			}
			else
			{
				m_angle = m_next_angle;
			}
			switch (transition2)
			{
			case Transition.AngleSnap:
				m_angle = m_next_angle;
				break;
			case Transition.AngleLerp:
				m_angle = Vector2.Lerp(m_angle, m_next_angle, num * speed.angle);
				break;
			case Transition.AngleMove:
				m_angle = Vector2.MoveTowards(m_angle, m_next_angle, num * speed.angle);
				break;
			}
			transition3 = (Transition)1792;
			transition2 = transition & transition3;
			if (IsDirty(m_anchor, m_next_anchor, 0.0005f))
			{
				flag = true;
			}
			else
			{
				m_anchor = m_next_anchor;
			}
			switch (transition2)
			{
			case Transition.AnchorSnap:
				m_anchor = m_next_anchor;
				break;
			case Transition.AnchorLerp:
				m_anchor = Vector3.Lerp(m_anchor, m_next_anchor, num * speed.anchor);
				break;
			case Transition.AnchorMove:
				m_anchor = Vector3.MoveTowards(m_anchor, m_next_anchor, num * speed.anchor);
				break;
			}
			if (Mathf.Abs(m_next_anchorRotation.w) <= Mathf.Epsilon)
			{
				m_next_anchorRotation.w = 1E-05f;
			}
			transition3 = (Transition)28672;
			transition2 = transition & transition3;
			if (IsDirty(m_anchorRotation, m_next_anchorRotation, 0.0001f))
			{
				flag2 = true;
			}
			else
			{
				m_anchorRotation = m_next_anchorRotation;
			}
			switch (transition2)
			{
			case Transition.AnchorRotationSnap:
				m_anchorRotation = m_next_anchorRotation;
				break;
			case Transition.AnchorRotationLerp:
				m_anchorRotation = Quaternion.Lerp(m_anchorRotation, m_next_anchorRotation, num * speed.angle);
				break;
			case Transition.AnchorRotationMove:
				m_anchorRotation = Quaternion.RotateTowards(m_anchorRotation, m_next_anchorRotation, num * speed.angle);
				break;
			}
			m_dirty = flag || flag2;
			if (m_dirty)
			{
				if (flag2)
				{
					UpdateRotation();
				}
				if (flag2 || flag)
				{
					UpdatePosition();
				}
			}
		}

		private void UpdateRotation()
		{
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			Quaternion quaternion = Quaternion.AngleAxis(m_angle.x, up);
			quaternion = m_anchorRotation * quaternion * Quaternion.AngleAxis(m_angle.y, right);
			float num = anglePrecision;
			Vector3 eulerAngles = quaternion.eulerAngles;
			eulerAngles.x = ((num <= Mathf.Epsilon) ? eulerAngles.x : (Mathf.Floor(eulerAngles.x * num) / num));
			eulerAngles.y = ((num <= Mathf.Epsilon) ? eulerAngles.y : (Mathf.Floor(eulerAngles.y * num) / num));
			eulerAngles.z = ((num <= Mathf.Epsilon) ? eulerAngles.z : (Mathf.Floor(eulerAngles.z * num) / num));
			base.transform.localEulerAngles = eulerAngles;
		}

		private void UpdatePosition()
		{
			if (!m_rb || m_rb.isKinematic)
			{
				Vector3 position = m_anchor + base.transform.forward * (0f - m_distance);
				base.transform.position = position;
			}
		}

		protected void Update()
		{
			if (m_dirty)
			{
				ApplyRefresh();
			}
		}

		protected void OnDrawGizmos()
		{
			float num = 2f;
			Camera current = Camera.current;
			Vector4 vector = anchor;
			vector.w = 1f;
			vector = current.worldToCameraMatrix * vector;
			vector = current.projectionMatrix * vector;
			float num2 = 2f * num / (float)Screen.width;
			vector.w = Mathf.Max(num * 0.5f, vector.w);
			num2 *= vector.w;
			Gizmos.color = new Color(0.5f, 0.5f, 1f);
			Gizmos.DrawSphere(anchor, num2 * num);
			Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
			Gizmos.DrawLine(anchor, base.transform.position);
		}

		private bool IsDirty(float a, float b, float bias)
		{
			return Mathf.Abs(a - b) >= bias;
		}

		private bool IsDirty(Vector3 a, Vector3 b, float bias)
		{
			if (IsDirty(a.x, b.x, bias))
			{
				return true;
			}
			if (IsDirty(a.y, b.y, bias))
			{
				return true;
			}
			if (IsDirty(a.z, b.z, bias))
			{
				return true;
			}
			return false;
		}

		private bool IsDirty(Quaternion a, Quaternion b, float bias)
		{
			if (IsDirty(a.x, b.x, bias))
			{
				return true;
			}
			if (IsDirty(a.y, b.y, bias))
			{
				return true;
			}
			if (IsDirty(a.z, b.z, bias))
			{
				return true;
			}
			if (IsDirty(a.w, b.w, bias))
			{
				return true;
			}
			return false;
		}

		public void SetTransitionSpeed(float p_distance, float p_angle, float p_anchor, float p_rotation)
		{
			speed.distance = p_distance;
			speed.angle = p_angle;
			speed.anchor = p_anchor;
			speed.rotation = p_rotation;
		}

		public void SetTransitionSpeed(Vector4 p_speed)
		{
			speed.distance = p_speed.x;
			speed.angle = p_speed.y;
			speed.anchor = p_speed.z;
			speed.rotation = p_speed.w;
		}
	}
}
