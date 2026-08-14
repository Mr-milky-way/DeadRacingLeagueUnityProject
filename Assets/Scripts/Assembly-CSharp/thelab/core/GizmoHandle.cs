using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class GizmoHandle : MonoBehaviour
	{
		[Serializable]
		public class Style
		{
			public Color normal = Color.white;

			public Color over = Color.white;

			public Color down = Color.white;

			public Color active = Color.white;

			public Color disabled = Color.gray;
		}

		[Serializable]
		public class Mouse
		{
			public bool enabled;

			public bool active;

			public Vector3 colliderDown;

			public Vector3 collider;

			public Vector3 colliderUp;

			public Vector3 down;

			public Vector3 position;

			public Vector3 up;

			public Vector3 offsetMask;

			public RaycastHit hit;

			public Ray ray;

			public Ray rayDown;

			public Ray rayUp;

			public Vector3 colliderOffset => collider - colliderDown;

			public Vector3 offset => position - down;
		}

		[Serializable]
		public class Keyboard
		{
			public bool enabled;

			public bool active;

			public List<KeyCode> keys;

			private List<KeyCode> m_keys_down;

			public bool down;

			public float hold;

			public float rate = 1f;

			public float multiplier = 1f;

			public List<KeyCode> multiplierKeys;

			public List<KeyCode> keyLocks;

			public Ray ray;

			public Ray rayDown;

			public Ray rayUp;

			public List<KeyCode> keysDown
			{
				get
				{
					if (m_keys_down == null)
					{
						m_keys_down = new List<KeyCode>();
					}
					m_keys_down.Clear();
					for (int i = 0; i < keys.Count; i++)
					{
						if (Input.GetKey(keys[i]))
						{
							m_keys_down.Add(keys[i]);
						}
					}
					return m_keys_down;
				}
			}

			public KeyCode keyDown
			{
				get
				{
					if (keysDown.Count > 0)
					{
						return keysDown[0];
					}
					return KeyCode.None;
				}
			}

			public int keyDownIndex => keys.IndexOf(keyDown);

			public bool multiplierDown
			{
				get
				{
					if (multiplierKeys.Count <= 0)
					{
						return false;
					}
					for (int i = 0; i < multiplierKeys.Count; i++)
					{
						if (Input.GetKey(multiplierKeys[i]))
						{
							return true;
						}
					}
					return false;
				}
			}

			public float value => hold * rate;

			public float step => rate * (multiplierDown ? multiplier : 1f) * Time.unscaledDeltaTime;
		}

		public Collider collider;

		public GizmoRenderer gizmo;

		[SerializeField]
		private Camera m_camera;

		private Camera m_main;

		protected Ray m_ray;

		public Style style;

		public bool down;

		public bool over;

		[SerializeField]
		private bool m_active;

		[SerializeField]
		private bool m_enabled = true;

		public bool lockCollider = true;

		[SerializeField]
		protected Mouse m_mouse;

		[SerializeField]
		protected Keyboard m_keyboard;

		[SerializeField]
		private GizmoHandleCallback m_callback;

		private HandleEventType m_key_state;

		private bool m_down_over;

		private Activity m_drag_loop;

		private bool m_collider_down;

		private bool m_collider_over;

		private Vector2 m_collider_mpos;

		protected RaycastHit m_collider_hit;

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

		public virtual Ray ray
		{
			get
			{
				if (!base.transform.hasChanged)
				{
					return m_ray;
				}
				return m_ray = new Ray(base.transform.position, base.transform.forward);
			}
		}

		public bool active
		{
			get
			{
				return m_active;
			}
			set
			{
				bool num = m_active;
				m_active = value;
				if (num != m_active)
				{
					Dispatch(m_active ? HandleEventType.Activate : HandleEventType.Deactivate);
				}
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
				bool num = m_enabled;
				m_enabled = value;
				if (num != m_enabled)
				{
					Dispatch(m_enabled ? HandleEventType.Enabled : HandleEventType.Disabled);
				}
			}
		}

		public Mouse mouse
		{
			get
			{
				if (m_mouse != null && m_mouse != null)
				{
					return m_mouse;
				}
				return m_mouse = new Mouse();
			}
		}

		public Keyboard keyboard
		{
			get
			{
				if (m_keyboard != null && m_keyboard != null)
				{
					return m_keyboard;
				}
				return m_keyboard = new Keyboard();
			}
		}

		public bool moving
		{
			get
			{
				if (mouse.active || keyboard.active || over)
				{
					return base.gameObject.activeInHierarchy;
				}
				return false;
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
			Refresh(p_force: true);
		}

		protected virtual void OnEnable()
		{
			Refresh(p_force: true);
		}

		public void Refresh(bool p_force = false)
		{
			if (!collider)
			{
				collider = AssertCollider();
			}
			if (!gizmo)
			{
				gizmo = GetComponent<GizmoRenderer>();
			}
			if ((bool)collider && (bool)gizmo && lockCollider)
			{
				UpdateCollider();
			}
			OnRefresh();
		}

		protected virtual void OnRefresh()
		{
		}

		protected virtual Collider AssertCollider()
		{
			return null;
		}

		public void Dispatch(HandleEventType p_type)
		{
			OnEvent(p_type);
			if (callback != null)
			{
				callback.Invoke(new GizmoHandleEvent(p_type, this));
			}
		}

		protected virtual void OnEvent(HandleEventType p_type)
		{
		}

		protected virtual void ColliderMouseDown()
		{
			if (m_enabled && (bool)collider)
			{
				down = true;
				over = true;
				m_down_over = true;
				UpdateMouse(HandleEventType.Down);
				Dispatch(HandleEventType.Down);
			}
		}

		protected virtual void ColliderMouseDrag()
		{
			if (!m_enabled || !collider || m_drag_loop != null)
			{
				return;
			}
			if (m_drag_loop == null)
			{
				UpdateMouse(HandleEventType.DragStart);
				Dispatch(HandleEventType.DragStart);
			}
			m_drag_loop = Activity.Run((Func<bool>)delegate
			{
				if (!this)
				{
					return false;
				}
				if (!base.gameObject)
				{
					return false;
				}
				bool flag = !m_enabled || !base.gameObject.activeInHierarchy;
				if (flag || !down)
				{
					m_drag_loop = null;
					if (flag)
					{
						m_collider_down = (m_collider_over = false);
					}
					if (!down)
					{
						ColliderMouseExit();
					}
					UpdateMouse(HandleEventType.DragEnd);
					Dispatch(HandleEventType.DragEnd);
					return false;
				}
				UpdateMouseDrag();
				return true;
			}, 0f, false);
			UpdateMouseDrag();
		}

		private void UpdateMouseDrag()
		{
			UpdateMouse(HandleEventType.Drag);
			Dispatch(HandleEventType.Drag);
		}

		protected virtual void ColliderMouseEnter()
		{
			if (m_enabled && (bool)collider)
			{
				over = true;
				UpdateMouse(HandleEventType.Enter);
				Dispatch(HandleEventType.Enter);
			}
		}

		protected virtual void ColliderMouseExit()
		{
			if (m_enabled && (bool)collider)
			{
				bool flag = m_drag_loop != null;
				bool num = down && !flag;
				down = flag;
				over = false;
				m_down_over = false;
				if (num)
				{
					UpdateMouse(HandleEventType.Up);
					Dispatch(HandleEventType.Up);
				}
				UpdateMouse(HandleEventType.Exit);
				Dispatch(HandleEventType.Exit);
			}
		}

		protected virtual void ColliderMouseOver()
		{
			if (m_enabled && (bool)collider)
			{
				over = true;
				UpdateMouse(HandleEventType.Stay);
				Dispatch(HandleEventType.Stay);
			}
		}

		protected virtual void ColliderMouseUp()
		{
			if (m_enabled && (bool)collider)
			{
				down = false;
				if (m_drag_loop != null)
				{
					m_drag_loop.Stop();
					m_drag_loop = null;
					UpdateMouse(HandleEventType.DragEnd);
					Dispatch(HandleEventType.DragEnd);
				}
				UpdateMouse(HandleEventType.Up);
				Dispatch(HandleEventType.Up);
				if (m_down_over)
				{
					m_down_over = false;
					Dispatch(HandleEventType.Click);
				}
			}
		}

		protected void UpdateMouse(HandleEventType p_type)
		{
			if (!keyboard.active && mouse.enabled)
			{
				OnMouseUpdate(p_type);
			}
		}

		protected virtual void OnMouseUpdate(HandleEventType p_type)
		{
			bool flag = m_drag_loop != null;
			switch (p_type)
			{
			case HandleEventType.Enter:
				mouse.position = Input.mousePosition;
				mouse.collider = GetScreenHit();
				mouse.ray = ray;
				break;
			case HandleEventType.Stay:
			{
				mouse.collider = GetScreenHit();
				mouse.position = Input.mousePosition;
				Vector3 offset2 = mouse.offset;
				if (offset2.magnitude > 8f && mouse.offsetMask.magnitude <= 0f)
				{
					mouse.offsetMask = ((Mathf.Abs(offset2.x) > Mathf.Abs(offset2.y)) ? new Vector3(1f, 0f) : new Vector3(0f, 1f));
				}
				break;
			}
			case HandleEventType.Down:
				mouse.colliderDown = GetScreenHit();
				mouse.down = Input.mousePosition;
				mouse.position = mouse.down;
				mouse.offsetMask = Vector3.zero;
				mouse.ray = ray;
				mouse.rayDown = ray;
				mouse.active = true;
				break;
			case HandleEventType.Up:
				mouse.collider = GetScreenHit();
				mouse.position = Input.mousePosition;
				mouse.up = mouse.position;
				mouse.colliderUp = mouse.collider;
				mouse.ray = ray;
				mouse.rayUp = ray;
				mouse.active = false;
				break;
			case HandleEventType.Drag:
			{
				mouse.position = Input.mousePosition;
				mouse.collider = GetScreenHit();
				mouse.ray = ray;
				Vector3 offset = mouse.offset;
				if (offset.magnitude > 8f && mouse.offsetMask.magnitude <= 0f)
				{
					mouse.offsetMask = ((Mathf.Abs(offset.x) > Mathf.Abs(offset.y)) ? new Vector3(1f, 0f) : new Vector3(0f, 1f));
				}
				break;
			}
			case HandleEventType.DragStart:
				mouse.active = true;
				mouse.ray = ray;
				break;
			case HandleEventType.Exit:
				mouse.active = flag;
				break;
			case HandleEventType.DragEnd:
				mouse.active = false;
				mouse.ray = ray;
				break;
			}
		}

		protected virtual bool IsColliderMouseOver()
		{
			Camera camera = this.camera;
			if (!camera)
			{
				return false;
			}
			Vector2 vector = Input.mousePosition;
			Ray ray = camera.ScreenPointToRay(vector);
			RaycastHit hitInfo = default(RaycastHit);
			int num;
			if (!collider)
			{
				num = 0;
			}
			else
			{
				num = (collider.Raycast(ray, out hitInfo, 1000f) ? 1 : 0);
				if (num != 0)
				{
					mouse.hit = hitInfo;
				}
			}
			return (byte)num != 0;
		}

		protected void MouseLateUpdate()
		{
			if (keyboard.active || !mouse.enabled)
			{
				return;
			}
			Vector2 vector = Input.mousePosition;
			if (!collider)
			{
				m_collider_down = false;
				m_collider_over = false;
				m_collider_mpos = vector;
				return;
			}
			Vector2 vector2 = vector - m_collider_mpos;
			m_collider_mpos = vector;
			bool flag = m_collider_down;
			bool flag2 = m_collider_over;
			bool flag3 = IsColliderMouseOver();
			bool flag4 = Input.GetKey(KeyCode.Mouse0);
			if (!flag2 && flag3)
			{
				ColliderMouseEnter();
				m_collider_over = true;
				flag2 = true;
			}
			if (flag2 && !flag3)
			{
				ColliderMouseExit();
				m_collider_over = false;
				flag2 = (flag3 = false);
			}
			if (flag3 && !flag && flag4)
			{
				ColliderMouseDown();
				m_collider_down = true;
				flag = true;
			}
			if (flag3 && flag && !flag4)
			{
				ColliderMouseUp();
				m_collider_down = false;
				flag = (flag4 = false);
			}
			if (flag && !flag4)
			{
				ColliderMouseUp();
				m_collider_down = false;
				flag = false;
			}
			if (flag && flag3 && vector2.sqrMagnitude > 0f)
			{
				ColliderMouseDrag();
			}
			if (flag2 && flag3)
			{
				ColliderMouseOver();
			}
		}

		protected void UpdateKeyboard(HandleEventType p_type)
		{
			if (!mouse.active && keyboard.enabled)
			{
				OnKeyboardUpdate(p_type);
			}
		}

		protected virtual void OnKeyboardUpdate(HandleEventType p_type)
		{
			Keyboard keyboard = this.keyboard;
			switch (p_type)
			{
			case HandleEventType.KeyDown:
				keyboard.active = true;
				keyboard.down = true;
				keyboard.hold = 0f;
				keyboard.ray = ray;
				keyboard.rayDown = ray;
				break;
			case HandleEventType.KeyHold:
			{
				keyboard.active = true;
				float deltaTime = Time.deltaTime;
				deltaTime *= (keyboard.multiplierDown ? keyboard.multiplier : 1f);
				keyboard.hold += deltaTime;
				keyboard.ray = ray;
				break;
			}
			case HandleEventType.KeyUp:
				keyboard.active = false;
				keyboard.down = false;
				keyboard.ray = ray;
				keyboard.rayUp = ray;
				break;
			}
		}

		protected void KeyboardLateUpdate()
		{
			if (mouse.active || !this.keyboard.enabled)
			{
				return;
			}
			for (int i = 0; i < this.keyboard.keyLocks.Count; i++)
			{
				if (Input.GetKey(this.keyboard.keyLocks[i]))
				{
					return;
				}
			}
			Keyboard keyboard = this.keyboard;
			HandleEventType handleEventType = m_key_state;
			switch (handleEventType)
			{
			case HandleEventType.None:
				if (keyboard.keyDown != KeyCode.None)
				{
					handleEventType = HandleEventType.KeyDown;
					UpdateKeyboard(HandleEventType.KeyDown);
					Dispatch(HandleEventType.KeyDown);
				}
				break;
			case HandleEventType.KeyDown:
			case HandleEventType.KeyHold:
				if (keyboard.keyDown == KeyCode.None)
				{
					UpdateKeyboard(HandleEventType.KeyUp);
					Dispatch(HandleEventType.KeyUp);
					handleEventType = HandleEventType.None;
				}
				else
				{
					handleEventType = HandleEventType.KeyHold;
					UpdateKeyboard(HandleEventType.KeyHold);
					Dispatch(HandleEventType.KeyHold);
				}
				break;
			}
			m_key_state = handleEventType;
		}

		protected virtual void UpdateCollider()
		{
		}

		protected virtual void OnRenderObject()
		{
			if ((bool)collider && (bool)gizmo && lockCollider)
			{
				UpdateCollider();
			}
		}

		public Vector3 GetScreenHit()
		{
			if (!collider)
			{
				return Input.mousePosition;
			}
			Camera camera = this.camera;
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = camera.nearClipPlane;
			Ray ray = camera.ScreenPointToRay(mousePosition);
			if (!collider.Raycast(ray, out var hitInfo, 30000f))
			{
				return mouse.collider;
			}
			return hitInfo.point;
		}

		public Vector3 ProjectScreenPointToRay(Camera p_camera, Vector2 p_screen_pos, Ray p_ray)
		{
			if (!p_camera)
			{
				return p_ray.origin;
			}
			Ray ray = p_ray;
			Vector3 position = p_camera.transform.position;
			Vector3 vector = p_screen_pos;
			vector.z = p_camera.nearClipPlane;
			Vector3 position2 = p_camera.ScreenToWorldPoint(vector);
			p_camera.transform.InverseTransformPoint(position2);
			Vector3 origin = ray.origin;
			Vector3 direction = ray.direction;
			Vector3 position3 = ray.origin + direction;
			Vector3 vector2 = p_camera.WorldToScreenPoint(origin);
			Vector3 vector3 = p_camera.WorldToScreenPoint(position3);
			vector2.z = p_camera.nearClipPlane;
			vector3.z = p_camera.nearClipPlane;
			Vector3 vector4 = vector3 - vector2;
			vector4.Normalize();
			Ray ray2 = new Ray(vector2, vector4);
			Vector3 rhs = vector - vector2;
			Vector3.Dot(vector4, rhs);
			Vector3 point = ray2.GetPoint(Vector3.Dot(vector - vector2, ray2.direction));
			float num = ((Vector3.Dot(point - vector2, vector3 - vector2) < 0f) ? (-1f) : 1f);
			Vector3 vector5 = p_camera.ScreenToWorldPoint(point);
			Vector3 vector6 = vector5 - position;
			vector6.Normalize();
			float num2 = Vector3.Distance(vector5, origin);
			Vector3 vector7 = origin - vector5;
			vector7.Normalize();
			float num3 = 0f;
			float num4 = Mathf.Acos(Vector3.Dot(vector6, vector7));
			Mathf.Sin(num4);
			float f = Vector3.Dot(direction * num, -vector7);
			float num5 = Mathf.Acos(f);
			f = Mathf.Sin(num5);
			num3 = Mathf.Sin((float)Math.PI - (num4 + num5));
			float num6 = ((Mathf.Abs(num3) <= 0.0001f) ? 0f : (f * num2 / num3));
			return vector5 + vector6 * num6;
		}

		public Vector3 ProjectScreenPointToPlane(Camera p_camera, Vector2 p_screen_pos, Ray p_plane)
		{
			if (!p_camera)
			{
				return p_plane.origin;
			}
			Ray ray = p_plane;
			_ = p_camera.transform.position;
			Vector3 pos = p_screen_pos;
			pos.z = p_camera.nearClipPlane;
			Ray ray2 = p_camera.ScreenPointToRay(pos);
			float num = Vector3.Dot(ray.origin - ray2.origin, ray.direction);
			float num2 = Vector3.Dot(ray2.direction, ray.direction);
			if (Mathf.Abs(num2) <= 0.001f)
			{
				return ray2.origin;
			}
			float num3 = num / num2;
			return ray2.origin + ray2.direction * num3;
		}

		protected virtual void LateUpdate()
		{
			if (enabled && base.gameObject.activeInHierarchy)
			{
				MouseLateUpdate();
			}
		}

		protected virtual void Update()
		{
			if (enabled && base.gameObject.activeInHierarchy)
			{
				KeyboardLateUpdate();
			}
		}
	}
}
