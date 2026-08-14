using UnityEngine;

namespace thelab.core
{
	public class RotateHandle : TRSHandle
	{
		public float rate = 360f;

		public Quaternion rotationDown;

		private Vector2 m_delta_mask;

		private Quaternion m_handle_rotation;

		public new RadialHandle current
		{
			get
			{
				return base.current as RadialHandle;
			}
			set
			{
				base.current = value;
			}
		}

		protected override void ApplyHandleValue(Vector3 p_delta)
		{
			RadialHandle radialHandle = current;
			Camera obj = base.camera;
			Vector2 vector = radialHandle.mouse.offsetMask;
			if (vector.sqrMagnitude <= 0f)
			{
				vector = new Vector2(1f, 0f);
			}
			float num = ((Vector3.Dot(obj.transform.forward, radialHandle.TopAxisDown) < 0f) ? (-1f) : 1f);
			float num2 = p_delta.x * num * vector.x;
			float num3 = p_delta.y * num * vector.y;
			float num4 = 0f;
			num4 = ((!(Mathf.Abs(num2) > Mathf.Abs(num3))) ? num3 : num2);
			num4 /= 400f;
			ApplyHandleValue(radialHandle, num4);
		}

		protected void ApplyHandleValue(RadialHandle p_handle, float p_delta)
		{
			if ((bool)p_handle)
			{
				float num = p_delta * rate;
				num = ((snap <= 0f) ? num : (Mathf.Round(num / snap) * snap));
				p_handle.gizmo.angle = num;
				Vector3 yAxisDown = p_handle.YAxisDown;
				base.transform.localRotation = rotationDown * Quaternion.AngleAxis(num, yAxisDown);
				p_handle.transform.rotation = p_handle.rotationPivot;
			}
		}

		protected override void SetFocus(GizmoHandle p_handle)
		{
			for (int i = 0; i < handles.Count; i++)
			{
				RadialHandle radialHandle = handles[i] as RadialHandle;
				radialHandle.transform.localRotation = radialHandle.rotationAnchor;
			}
			base.SetFocus(p_handle);
			for (int j = 0; j < handles.Count; j++)
			{
				RadialHandle radialHandle2 = handles[j] as RadialHandle;
				SetHandleStateActive(radialHandle2, radialHandle2 == current);
			}
		}

		protected void SetHandleStateActive(RadialHandle p_handle, bool p_flag)
		{
			if ((bool)p_handle)
			{
				p_handle.gizmo.angle = (p_flag ? 0f : 360f);
				p_handle.gizmo.culling = (p_flag ? 0f : 0.98f);
				p_handle.gizmo.closed = p_flag;
				p_handle.gizmo.fill = p_flag;
				p_handle.gizmo.fillColor = (p_flag ? p_handle.style.active : p_handle.style.normal);
				p_handle.gizmo.fillColor.a = 0.1f;
				p_handle.gizmo.color = (p_flag ? p_handle.style.active : p_handle.style.normal);
			}
		}

		protected override void OnHandleEvent(GizmoHandleEvent p_event)
		{
			if (p_event.target is RadialHandle)
			{
				OnRadialHandleEvent(p_event);
			}
		}

		protected void OnRadialHandleEvent(GizmoHandleEvent p_event)
		{
			RadialHandle radialHandle = p_event.target as RadialHandle;
			Camera camera = base.camera;
			if (!radialHandle || transforms.Count <= 0)
			{
				SetFocus(null);
				return;
			}
			switch (p_event.type)
			{
			case HandleEventType.Down:
			case HandleEventType.DragStart:
				SetFocus(radialHandle);
				if ((bool)camera && (bool)current)
				{
					radialHandle.LockRotationAtPoint(radialHandle.mouse.hit.point);
					radialHandle.YAxisDown = base.transform.InverseTransformVector(radialHandle.transform.up);
					switch (radialHandle.name)
					{
					case "x":
						radialHandle.TopAxisDown = -radialHandle.transform.up;
						break;
					case "y":
						radialHandle.TopAxisDown = radialHandle.transform.forward;
						break;
					case "z":
						radialHandle.TopAxisDown = radialHandle.transform.right;
						break;
					}
					rotationDown = base.transform.localRotation;
				}
				break;
			case HandleEventType.Drag:
				if ((bool)camera && (bool)current)
				{
					Vector3 offset = radialHandle.mouse.offset;
					UpdateHandle(offset);
				}
				break;
			case HandleEventType.DragEnd:
				SetFocus(null);
				break;
			case HandleEventType.Up:
				SetFocus(null);
				break;
			case HandleEventType.KeyDown:
			{
				int handleKeyboardActiveCount = GetHandleKeyboardActiveCount();
				if (!current && handleKeyboardActiveCount == 1)
				{
					current = radialHandle;
					m_delta = Vector3.zero;
					rotationDown = base.transform.localRotation;
				}
				radialHandle.LockRotationAtPoint(camera.transform.position - camera.transform.right);
				radialHandle.YAxisDown = base.transform.InverseTransformVector(radialHandle.transform.up);
				switch (radialHandle.name)
				{
				case "x":
					radialHandle.TopAxisDown = radialHandle.transform.up;
					break;
				case "y":
					radialHandle.TopAxisDown = radialHandle.transform.forward;
					break;
				case "z":
					radialHandle.TopAxisDown = radialHandle.transform.right;
					break;
				}
				SetHandleStateActive(radialHandle, p_flag: true);
				break;
			}
			case HandleEventType.KeyHold:
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < handles.Count; i++)
				{
					RadialHandle radialHandle2 = handles[i] as RadialHandle;
					if (!radialHandle2)
					{
						continue;
					}
					GizmoHandle.Keyboard keyboard = radialHandle2.keyboard;
					if (keyboard.active)
					{
						int siblingIndex = radialHandle2.transform.GetSiblingIndex();
						Vector3 zero2 = Vector3.zero;
						switch (siblingIndex)
						{
						case 0:
							zero2.x = 1f;
							break;
						case 1:
							zero2.y = 1f;
							break;
						case 2:
							zero2.z = 1f;
							break;
						}
						bool num = keyboard.keyDownIndex == 0;
						bool flag = keyboard.keyDownIndex == 1;
						if (num)
						{
							zero += zero2 * keyboard.step;
						}
						if (flag)
						{
							zero += zero2 * (0f - keyboard.step);
						}
					}
				}
				m_delta += zero;
				radialHandle = current;
				for (int j = 0; j < handles.Count; j++)
				{
					RadialHandle radialHandle3 = (RadialHandle)handles[j];
					SetHandleStateActive(radialHandle3, radialHandle3.keyboard.active);
					if (radialHandle3.keyboard.active)
					{
						zero.Set(0f - m_delta[j], 0f - m_delta[j], 0f - m_delta[j]);
						current = radialHandle3;
						UpdateHandle(zero);
					}
				}
				current = radialHandle;
				break;
			}
			case HandleEventType.KeyUp:
				SetHandleStateActive(radialHandle, p_flag: false);
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
		}

		public void SetHandleYEnabled(bool p_flag)
		{
			SetHandleEnabled(1, p_flag);
		}

		public void SetHandleZEnabled(bool p_flag)
		{
			SetHandleEnabled(2, p_flag);
		}
	}
}
