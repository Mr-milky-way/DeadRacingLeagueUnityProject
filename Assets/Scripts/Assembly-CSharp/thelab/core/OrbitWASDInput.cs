using UnityEngine;
using drl.sim.rci;

namespace thelab.core
{
	public class OrbitWASDInput : MonoBehaviour
	{
		public bool userInteracts;

		private OrbitTransform m_orbit;

		public float joystickThreshold;

		public float sensitivity = 1f;

		public float joystickSensitivityMultiplier = 1f;

		public float moveSpeed = 3f;

		[Tooltip("Seconds until Max speed")]
		public float moveAccel = 1f;

		public float moveMultiplier = 3f;

		public float scrollStep = 0.25f;

		public bool snapOnRelease;

		[HideInInspector]
		public Vector3 velocity;

		public float collisionRadius = 0.3f;

		[SerializeField]
		private bool m_usePhysics;

		public bool useJoystick;

		[HideInInspector]
		public bool allowZoom = true;

		public bool useTimeScale;

		public string[] axis = new string[4] { "StickLX", "StickLY", "StickRX", "StickRY" };

		public KeyCode orbitDragKey = KeyCode.Mouse0;

		private Vector2 m_last_mouse;

		private Vector2 m_last_angle;

		private float m_current_speed;

		private Vector3 m_rb_velocity;

		protected Rigidbody m_rb;

		protected SphereCollider m_sc;

		public OrbitTransform orbit
		{
			get
			{
				if (!m_orbit)
				{
					return m_orbit = GetComponent<OrbitTransform>();
				}
				return m_orbit;
			}
		}

		public bool usePhysics
		{
			get
			{
				return orbit.m_rb;
			}
			set
			{
				m_usePhysics = value;
				orbit.m_rb = (m_usePhysics ? m_rb : null);
				if ((bool)m_sc)
				{
					m_sc.enabled = m_usePhysics;
				}
			}
		}

		protected void Awake()
		{
			m_current_speed = 0f;
			m_last_mouse = Input.mousePosition;
			m_rb = GetComponent<Rigidbody>();
			if (m_rb == null)
			{
				m_rb = base.gameObject.AddComponent<Rigidbody>();
			}
			m_rb.useGravity = false;
			m_rb.constraints = RigidbodyConstraints.FreezeRotation;
			m_rb.isKinematic = false;
			m_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			m_rb.hideFlags = HideFlags.HideInInspector;
			m_sc = base.gameObject.AddComponent<SphereCollider>();
			m_sc.radius = collisionRadius;
			m_sc.enabled = false;
			m_sc.hideFlags = HideFlags.HideInInspector;
			usePhysics = m_usePhysics;
		}

		protected virtual float GetAxis(int p_id)
		{
			return Input.GetAxis((p_id < 0) ? "" : ((p_id >= axis.Length) ? "" : axis[p_id]));
		}

		public void ResetInput()
		{
			ClearInput();
			orbit.Snap(p_position: true, p_angle: false);
		}

		public void ClearInput()
		{
			m_last_mouse = Input.mousePosition;
			m_last_angle = orbit.angle;
			velocity = Vector3.zero;
			m_rb_velocity = Vector3.zero;
			if ((bool)m_rb)
			{
				m_rb.velocity = m_rb_velocity;
			}
		}

		protected void Update()
		{
			velocity = Vector3.zero;
			m_rb_velocity = Vector3.zero;
			if (!base.enabled)
			{
				return;
			}
			bool flag = false;
			bool flag2 = !orbit.IsTransitionEnabled(OrbitTransform.Transition.AngleLock);
			bool flag3 = !orbit.IsTransitionEnabled(OrbitTransform.Transition.DistanceLock);
			bool flag4 = !orbit.IsTransitionEnabled(OrbitTransform.Transition.AnchorLock);
			userInteracts = false;
			if (m_usePhysics && Mathf.Abs(m_sc.radius - collisionRadius) > 0f)
			{
				m_sc.radius = collisionRadius;
			}
			if (usePhysics != m_usePhysics)
			{
				usePhysics = m_usePhysics;
			}
			float num = 1f;
			float num2 = (useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime);
			if (Input.GetKey(KeyCode.LeftShift))
			{
				num = moveMultiplier;
			}
			if (useJoystick)
			{
				for (int i = 0; i < axis.Length; i++)
				{
					float num3 = GetAxis(i);
					if (!(Mathf.Abs(num3) <= joystickThreshold))
					{
						userInteracts = true;
						Vector3 vector = Vector3.zero;
						Vector2 angle = orbit.angle;
						bool flag5 = false;
						float num4 = 12.5f;
						float num5 = 2f;
						switch (i)
						{
						case 0:
							vector = orbit.transform.right;
							flag5 = true;
							break;
						case 1:
							vector = -orbit.transform.forward;
							flag5 = true;
							break;
						case 2:
							angle.x += num3 * sensitivity * joystickSensitivityMultiplier * 360f * num4 * num2;
							break;
						case 3:
							angle.y += num3 * sensitivity * joystickSensitivityMultiplier * 360f * num4 * num2;
							break;
						}
						if (flag5)
						{
							velocity += vector * m_current_speed * num3 * num5;
							flag = true;
						}
						else
						{
							orbit.angle = angle;
						}
					}
				}
				if (Mathf.Abs(GetAxis(2)) <= joystickThreshold && Mathf.Abs(GetAxis(3)) <= joystickThreshold)
				{
					orbit.StopTransition(OrbitTransform.TransitionMask.AngleMask);
				}
			}
			if (Input.GetKey(KeyCode.W))
			{
				velocity += orbit.transform.forward * m_current_speed;
				flag = true;
			}
			if (Input.GetKey(KeyCode.S))
			{
				velocity += orbit.transform.forward * (0f - m_current_speed);
				flag = true;
			}
			if (Input.GetKey(KeyCode.A))
			{
				velocity += orbit.transform.right * (0f - m_current_speed);
				flag = true;
			}
			if (Input.GetKey(KeyCode.D))
			{
				velocity += orbit.transform.right * m_current_speed;
				flag = true;
			}
			if (Input.GetKey(KeyCode.E))
			{
				velocity += orbit.transform.up * m_current_speed;
				flag = true;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				velocity += orbit.transform.up * (0f - m_current_speed);
				flag = true;
			}
			if (!flag4)
			{
				velocity = Vector3.zero;
			}
			if (flag)
			{
				float num6 = ((moveAccel <= 0f) ? 0f : (1f / moveAccel));
				m_current_speed = Mathf.Lerp(m_current_speed, moveSpeed * num, num2 * num6);
			}
			else
			{
				m_current_speed = 0f;
			}
			m_rb_velocity = velocity;
			if (!usePhysics && velocity.sqrMagnitude > Mathf.Epsilon)
			{
				orbit.anchor += velocity * num2;
			}
			if (Input.GetKeyDown(orbitDragKey))
			{
				ResetInput();
			}
			float num7 = Input.mousePosition.x - m_last_mouse.x;
			float num8 = Input.mousePosition.y - m_last_mouse.y;
			if (Input.GetKey(orbitDragKey))
			{
				if (!flag2)
				{
					num7 = (num8 = 0f);
				}
				userInteracts = true;
				Vector2 angle2 = orbit.angle;
				angle2.x = m_last_angle.x + num7 * sensitivity;
				angle2.y = m_last_angle.y + (0f - num8) * sensitivity;
				orbit.angle = angle2;
			}
			if (Input.GetKeyUp(orbitDragKey) && snapOnRelease)
			{
				orbit.StopTransition(OrbitTransform.TransitionMask.AngleMask);
			}
			num8 = Input.mouseScrollDelta.y;
			if (useJoystick && allowZoom && !RCI.IsRCController())
			{
				if (RCI.GetButton(ConsoleButtons.LeftShoulder2))
				{
					num8 = -1f;
				}
				if (RCI.GetButton(ConsoleButtons.RightShoulder2))
				{
					num8 = 1f;
				}
			}
			if (!flag3)
			{
				num8 = 0f;
			}
			if (Mathf.Abs(num8) > 0f)
			{
				userInteracts = true;
				orbit.distance += ((num8 < 0f) ? scrollStep : (0f - scrollStep));
			}
		}

		protected void FixedUpdate()
		{
			if (usePhysics && (bool)m_rb)
			{
				m_rb.velocity = m_rb_velocity;
			}
		}

		protected void OnDisable()
		{
			ClearInput();
		}
	}
}
