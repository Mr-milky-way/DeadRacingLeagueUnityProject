using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class ScaleHandle : TRSHandle
	{
		public Transform xyzAxis;

		public float rate = 1f;

		public bool uniform;

		public bool useDelta;

		public Vector3 mouseProjectionDown;

		public Vector3 mouseProjection;

		public Vector3 scaleDown;

		public Vector3 minScale = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

		public Vector3 maxScale = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

		private Transform m_scale_anchor;

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

		protected override void Awake()
		{
			m_scale_anchor = new GameObject("scale-anchor-" + base.gameObject.GetInstanceID().ToString("x")).transform;
			m_scale_anchor.hideFlags = HideFlags.HideInHierarchy;
			m_scale_anchor.position = base.transform.position;
			m_scale_anchor.rotation = base.transform.rotation;
			base.Awake();
		}

		protected override void OnTargetsAdd(List<Transform> p_list)
		{
			int count = base.targets.Count;
			bool flag = flatHierarchy;
			if (count <= 1)
			{
				flatHierarchy = false;
			}
			base.OnTargetsAdd(p_list);
			flatHierarchy = flag;
		}

		protected override void ApplyHandleValue(Vector3 p_delta)
		{
			int num = base.currentIndex;
			if (num < 0)
			{
				return;
			}
			Vector3 scale = scaleDown + p_delta * rate;
			bool num2 = num == 0 || num == 3 || uniform;
			bool flag = num == 1 || num == 3 || uniform;
			bool flag2 = num == 2 || num == 3 || uniform;
			if (num2)
			{
				scale[0] = ((snap <= 0f) ? scale[0] : (Mathf.Round(scale[0] / snap) * snap));
			}
			else
			{
				scale[0] = 1f;
			}
			if (flag)
			{
				scale[1] = ((snap <= 0f) ? scale[1] : (Mathf.Round(scale[1] / snap) * snap));
			}
			else
			{
				scale[1] = 1f;
			}
			if (flag2)
			{
				scale[2] = ((snap <= 0f) ? scale[2] : (Mathf.Round(scale[2] / snap) * snap));
			}
			else
			{
				scale[2] = 1f;
			}
			List<Transform> list = transforms;
			List<Vector3> list2 = hierarchyPositions;
			List<Vector3> list3 = hierarchyScales;
			scale.x = Mathf.Clamp(scale.x, minScale.x, maxScale.x);
			scale.y = Mathf.Clamp(scale.y, minScale.y, maxScale.y);
			scale.z = Mathf.Clamp(scale.z, minScale.z, maxScale.z);
			for (int i = 0; i < list.Count; i++)
			{
				Transform transform = list[i];
				if ((bool)transform)
				{
					Vector3 localPosition = list2[i];
					Vector3 localScale = list3[i];
					localPosition.Scale(scale);
					localScale.Scale(scale);
					transform.localPosition = localPosition;
					transform.localScale = localScale;
				}
			}
		}

		protected override void InitHandleValue(Vector3 p_delta)
		{
		}

		protected override void UpdateHandle(Vector3 p_delta)
		{
			base.UpdateHandle(p_delta);
		}

		protected override void SetFocus(GizmoHandle p_handle)
		{
			m_anchor = (p_handle ? m_scale_anchor : null);
			base.SetFocus(p_handle);
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
					scaleDown = Vector3.one;
					m_scale_anchor.position = base.transform.position;
					m_scale_anchor.rotation = base.transform.rotation;
					m_scale_anchor.localScale = Vector3.one;
					StoreTransformData(p_use_anchor: true);
					Ray rayDown2 = plane.mouse.rayDown;
					mouseProjection = plane.GetMouseProjection(camera, rayDown2);
					mouseProjectionDown = mouseProjection;
				}
				break;
			case HandleEventType.Drag:
				if ((bool)camera && (bool)plane)
				{
					Vector3 one = Vector3.one;
					Ray rayDown = plane.mouse.rayDown;
					mouseProjection = plane.GetMouseProjection(camera, rayDown);
					one = mouseProjection - mouseProjectionDown;
					float magnitude = one.magnitude;
					float num = ((Vector3.Dot(one, planeHandle.transform.forward) < 0f) ? (-1f) : 1f);
					UpdateHandle(Vector3.one * magnitude * num);
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
				m_delta = Vector3.zero;
				if (base.currentIndex != 3)
				{
					SetFocus(arrowHandle);
					if ((bool)camera && (bool)arrow)
					{
						scaleDown = Vector3.one;
						m_scale_anchor.position = base.transform.position;
						m_scale_anchor.rotation = base.transform.rotation;
						m_scale_anchor.localScale = Vector3.one;
						StoreTransformData(p_use_anchor: true);
						Ray rayDown = arrow.mouse.rayDown;
						mouseProjection = arrow.GetMouseProjection(camera, rayDown);
						mouseProjectionDown = mouseProjection;
					}
				}
				break;
			case HandleEventType.Drag:
				if ((bool)camera && (bool)arrow)
				{
					Vector3 one = Vector3.one;
					float num5;
					if (base.currentIndex == 3)
					{
						one = arrowHandle.mouse.offset;
						one.Scale(arrowHandle.mouse.offsetMask);
						float num4 = ((rate > 0f) ? (1f / rate) : rate);
						one *= 0.01f * num4;
						num5 = ((Mathf.Abs(one.x) > Mathf.Abs(one.y)) ? one.x : one.y);
					}
					else
					{
						Ray rayDown2 = current.mouse.rayDown;
						mouseProjection = arrow.GetMouseProjection(camera, rayDown2);
						one = mouseProjection - mouseProjectionDown;
						float magnitude2 = one.magnitude;
						float num6 = ((Vector3.Dot(one, arrowHandle.transform.forward) < 0f) ? (-1f) : 1f);
						num5 = magnitude2 * num6;
					}
					Vector3 vector = Vector3.one * num5;
					Vector3 p_delta2 = (useDelta ? (vector - m_delta) : vector);
					m_delta = vector;
					UpdateHandle(p_delta2);
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
				if (!current && handleKeyboardActiveCount == 1)
				{
					current = arrowHandle;
					m_delta = Vector3.zero;
					scaleDown = Vector3.one;
					m_scale_anchor.position = base.transform.position;
					m_scale_anchor.rotation = base.transform.rotation;
					m_scale_anchor.localScale = Vector3.one;
					StoreTransformData(p_use_anchor: true);
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
						_ = keyboard.rayDown;
						bool flag = keyboard.keyDownIndex == 0;
						bool flag2 = keyboard.keyDownIndex == 1;
						float step = keyboard.step;
						Vector3 zero2 = Vector3.zero;
						switch (arrowHandle2.transform.GetSiblingIndex())
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
						if (flag)
						{
							zero += zero2 * step;
						}
						if (flag2)
						{
							zero += zero2 * (0f - step);
						}
					}
				}
				m_delta += zero;
				Vector3 p_delta = m_delta;
				if (uniform)
				{
					int num = ((p_delta.x < 0f) ? 1 : 0) + ((p_delta.y < 0f) ? 1 : 0) + ((p_delta.z < 0f) ? 1 : 0);
					int num2 = ((p_delta.x > 0f) ? 1 : 0) + ((p_delta.y > 0f) ? 1 : 0) + ((p_delta.z > 0f) ? 1 : 0);
					float num3 = ((num >= num2) ? (-1f) : 1f);
					float magnitude = p_delta.magnitude;
					p_delta = Vector3.one * magnitude * num3;
				}
				UpdateHandle(p_delta);
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
			SetHandleEnabled(5, p_flag);
			SetHandleEnabled(6, p_flag);
			SetHandleEnabled(3, p_flag);
		}

		public void SetHandleYEnabled(bool p_flag)
		{
			SetHandleEnabled(1, p_flag);
			SetHandleEnabled(4, p_flag);
			SetHandleEnabled(6, p_flag);
			SetHandleEnabled(3, p_flag);
		}

		public void SetHandleZEnabled(bool p_flag)
		{
			SetHandleEnabled(2, p_flag);
			SetHandleEnabled(4, p_flag);
			SetHandleEnabled(5, p_flag);
			SetHandleEnabled(3, p_flag);
		}
	}
}
