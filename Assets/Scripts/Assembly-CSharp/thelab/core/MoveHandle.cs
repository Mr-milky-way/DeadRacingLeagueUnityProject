using UnityEngine;

namespace thelab.core
{
	public class MoveHandle : TRSHandle
	{
		public Vector3 mouseProjectionDown;

		public Vector3 mouseProjection;

		public Vector3 positionDown;

		public ArrowHandle arrow
		{
			get
			{
				return current as ArrowHandle;
			}
			set
			{
				current = value;
			}
		}

		public PlaneHandle plane
		{
			get
			{
				return current as PlaneHandle;
			}
			set
			{
				current = value;
			}
		}

		protected override void ApplyHandleValue(Vector3 p_delta)
		{
			if (base.currentIndex >= 0)
			{
				Vector3 vector = p_delta;
				vector.x = ((snap <= 0f) ? vector.x : (Mathf.Round(vector.x / snap) * snap));
				vector.y = ((snap <= 0f) ? vector.y : (Mathf.Round(vector.y / snap) * snap));
				vector.z = ((snap <= 0f) ? vector.z : (Mathf.Round(vector.z / snap) * snap));
				Vector3 position = positionDown + vector;
				base.transform.position = position;
			}
		}

		protected override void OnHandleEvent(GizmoHandleEvent p_event)
		{
			if (p_event.target is ArrowHandle)
			{
				OnArrowHandleEvent(p_event);
			}
			else if (p_event.target is PlaneHandle)
			{
				OnPlaneEvent(p_event);
			}
		}

		protected void OnPlaneEvent(GizmoHandleEvent p_event)
		{
			PlaneHandle planeHandle = p_event.target as PlaneHandle;
			if (!planeHandle || transforms.Count <= 0)
			{
				SetFocus(null);
				return;
			}
			Camera camera = base.camera;
			switch (p_event.type)
			{
			case HandleEventType.Down:
			case HandleEventType.DragStart:
				SetFocus(planeHandle);
				if ((bool)camera && (bool)plane)
				{
					RefreshTransform();
					Ray rayDown2 = current.mouse.rayDown;
					positionDown = base.transform.position;
					mouseProjection = plane.GetMouseProjection(camera, rayDown2);
					mouseProjectionDown = mouseProjection;
				}
				break;
			case HandleEventType.Drag:
				if ((bool)camera && (bool)plane)
				{
					Ray rayDown = current.mouse.rayDown;
					mouseProjection = plane.GetMouseProjection(camera, rayDown);
					Vector3 p_delta = mouseProjection - mouseProjectionDown;
					UpdateHandle(p_delta);
				}
				break;
			case HandleEventType.Up:
				SetFocus(null);
				break;
			case HandleEventType.DragEnd:
				SetFocus(null);
				break;
			case HandleEventType.Stay:
				break;
			}
		}

		protected void OnArrowHandleEvent(GizmoHandleEvent p_event)
		{
			ArrowHandle arrowHandle = p_event.target as ArrowHandle;
			if (!arrowHandle || transforms.Count <= 0)
			{
				SetFocus(null);
				return;
			}
			Camera camera = base.camera;
			switch (p_event.type)
			{
			case HandleEventType.Down:
			case HandleEventType.DragStart:
				SetFocus(arrowHandle);
				if ((bool)camera && (bool)arrow)
				{
					RefreshTransform();
					Ray rayDown = current.mouse.rayDown;
					positionDown = base.transform.position;
					mouseProjection = arrow.GetMouseProjection(camera, rayDown);
					mouseProjectionDown = mouseProjection;
				}
				break;
			case HandleEventType.Drag:
				if ((bool)camera && (bool)arrow)
				{
					Ray rayDown3 = current.mouse.rayDown;
					mouseProjection = arrow.GetMouseProjection(camera, rayDown3);
					Vector3 p_delta = mouseProjection - mouseProjectionDown;
					UpdateHandle(p_delta);
				}
				break;
			case HandleEventType.Up:
				SetFocus(null);
				break;
			case HandleEventType.DragEnd:
				SetFocus(null);
				break;
			case HandleEventType.KeyDown:
			{
				int handleKeyboardActiveCount = GetHandleKeyboardActiveCount();
				if (!arrow && handleKeyboardActiveCount == 1)
				{
					current = arrowHandle;
					RefreshTransform();
					m_delta = Vector3.zero;
					positionDown = base.transform.position;
				}
				break;
			}
			case HandleEventType.KeyHold:
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < handles.Count; i++)
				{
					ArrowHandle arrowHandle2 = handles[i] as ArrowHandle;
					if (!arrowHandle2)
					{
						continue;
					}
					GizmoHandle.Keyboard keyboard = arrowHandle2.keyboard;
					if (keyboard.active)
					{
						Ray rayDown2 = keyboard.rayDown;
						bool flag = keyboard.keyDownIndex == 0;
						bool flag2 = keyboard.keyDownIndex == 1;
						float step = keyboard.step;
						Vector3 zero2 = Vector3.zero;
						if (flag)
						{
							zero2 += rayDown2.direction;
						}
						if (flag2)
						{
							zero2 -= rayDown2.direction;
						}
						zero2.Normalize();
						if (flag || flag2)
						{
							zero += zero2 * step;
						}
					}
				}
				m_delta += zero;
				UpdateHandle(m_delta);
				break;
			}
			case HandleEventType.KeyUp:
				if (GetHandleKeyboardActiveCount() <= 0)
				{
					SetFocus(null);
				}
				break;
			case HandleEventType.Stay:
			case HandleEventType.Click:
			case HandleEventType.Activate:
			case HandleEventType.Deactivate:
			case HandleEventType.Disabled:
			case HandleEventType.Enabled:
				break;
			}
		}

		public void SetHandleXEnabled(bool p_flag)
		{
			SetHandleEnabled(0, p_flag);
			SetHandleEnabled(4, p_flag);
			SetHandleEnabled(5, p_flag);
		}

		public void SetHandleYEnabled(bool p_flag)
		{
			SetHandleEnabled(1, p_flag);
			SetHandleEnabled(3, p_flag);
			SetHandleEnabled(5, p_flag);
		}

		public void SetHandleZEnabled(bool p_flag)
		{
			SetHandleEnabled(2, p_flag);
			SetHandleEnabled(3, p_flag);
			SetHandleEnabled(4, p_flag);
		}
	}
}
