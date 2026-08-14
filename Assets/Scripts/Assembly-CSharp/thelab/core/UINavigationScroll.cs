using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using drl.sim.rci;

namespace thelab.core
{
	public class UINavigationScroll : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Serializable]
		public class FocusProperty
		{
			public float duration = 0.1f;
		}

		[Serializable]
		public class MouseProperty
		{
			public float wheelSpeed = -50f;

			public float wheelDuration = 0.15f;
		}

		public ScrollAreaType areaType;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private Canvas m_root;

		public RectTransform viewrect;

		public LayoutFitter bounds;

		public RectTransform boundsRT;

		public Vector2 boundsMarginLftBtm = new Vector2(0f, 0f);

		public Vector2 boundsMarginRgtTop = new Vector2(90f, 0f);

		public RectTransform[] boundsTargets;

		public RectTransform container;

		public RectTransform[] containerSizeCalculationTargets;

		public bool containerNeedsSizeUpdate;

		public NavigationModeType mode;

		public FocusProperty focus;

		public MouseProperty mouse;

		private float contentSizeChangeTimer;

		private Vector2? contentSizeDifference;

		[HideInInspector]
		public ScrollRect dragScroller;

		private bool dragScrollAllowed;

		private float dragDisableTimer;

		public bool dragScrollUseElasticity = true;

		public Vector2 dragScrollerOffsetMinLftBtm = new Vector2(0f, 70f);

		public Vector2 dragScrollerOffsetMaxRgtTop = new Vector2(0f, -110f);

		public Vector2 dragScrollerContentSizeAdjustment = new Vector2(0f, 0f);

		public Vector2 scroll;

		public Vector2 scrollFocus;

		public Vector2 offset;

		public bool scrollX = true;

		public bool forceScrollX;

		public bool scrollY = true;

		public bool forceScrollY;

		public bool scrollClickAndDrag = true;

		public bool scrollMouseWheel = true;

		public bool enableJoystickPanning;

		public float panSpeed = 10f;

		private Vector2 m_target_position;

		private Vector3[] m_bounds_corners;

		private Vector3[] m_focus_corners;

		private Vector3[] m_viewrect_corners;

		private Vector3[] m_corners = new Vector3[4];

		[HideInInspector]
		public float fx;

		[HideInInspector]
		public float fy;

		[HideInInspector]
		public float bw;

		[HideInInspector]
		public float bh;

		private bool is_scroll_reset;

		private Activity m_set_enable_timer;

		[HideInInspector]
		public Vector3 localFocusCenter;

		[HideInInspector]
		public Vector3 localViewRectCenter;

		private bool lockFocusing;

		public bool disableFocusing;

		public Canvas canvas
		{
			get
			{
				if (!m_canvas)
				{
					return m_canvas = Hierarchy.FindReverse<Canvas>(base.transform);
				}
				return m_canvas;
			}
		}

		public Canvas root
		{
			get
			{
				if (!m_root)
				{
					return m_root = (canvas ? canvas.rootCanvas : null);
				}
				return m_root;
			}
		}

		public float duration
		{
			get
			{
				return focus.duration;
			}
			set
			{
				focus.duration = value;
			}
		}

		protected void Awake()
		{
			focus = new FocusProperty();
			mouse = new MouseProperty();
			m_bounds_corners = new Vector3[4];
			m_focus_corners = new Vector3[4];
			m_viewrect_corners = new Vector3[4];
		}

		public void ResetScroll(bool p_force)
		{
			if (is_scroll_reset)
			{
				return;
			}
			is_scroll_reset = p_force;
			scroll = Vector2.zero;
			scrollFocus = Vector2.zero;
			m_target_position = Vector2.zero;
			dragDisableTimer = -1f;
			if (p_force && (bool)container)
			{
				Vector2 anchoredPosition = container.anchoredPosition;
				if (scrollX)
				{
					anchoredPosition.x = m_target_position.x;
				}
				if (scrollY)
				{
					anchoredPosition.y = m_target_position.y;
				}
				container.anchoredPosition = anchoredPosition;
				bool current_scroll_x = scrollX;
				bool current_scroll_y = scrollY;
				scrollX = false;
				scrollY = false;
				Activity.RunOnce(delegate
				{
					scrollX = current_scroll_x;
					scrollY = current_scroll_y;
					is_scroll_reset = false;
				}, 1f / 6f);
			}
		}

		public void ResetScroll()
		{
			ResetScroll(p_force: false);
		}

		public void SetOffset(float p_off_x, float p_off_y)
		{
			offset.x = p_off_x;
			offset.y = p_off_y;
		}

		public void SetOffset(Vector2 p_offset)
		{
			offset = p_offset;
		}

		public void SetScroll(bool p_x, bool p_y)
		{
			scrollX = p_x;
			scrollY = p_y;
		}

		public void SetEnabled(bool p_flag, float p_delay = 0f)
		{
			if (m_set_enable_timer != null)
			{
				m_set_enable_timer.Stop();
			}
			m_set_enable_timer = Activity.RunOnce(delegate
			{
				base.enabled = p_flag;
			}, p_delay);
		}

		protected void Start()
		{
			m_target_position = default(Vector2);
		}

		protected void Update()
		{
			bool flag = (bounds ? bounds.transform : boundsRT);
			if (!base.enabled || !flag || !container || m_bounds_corners == null || !viewrect || !canvas || !canvas.enabled)
			{
				return;
			}
			if (enableJoystickPanning)
			{
				NavigationModeType navigationModeType = mode;
				if (navigationModeType == NavigationModeType.Focus || (uint)(navigationModeType - 2) <= 1u)
				{
					UpdateJoystickPan();
					return;
				}
			}
			if (IsContentSizeChanging())
			{
				contentSizeChangeTimer -= Time.deltaTime;
				if (!lockFocusing && !disableFocusing)
				{
					UpdateFocusScroll(contentSizeChangeTimer, contentSizeDifference);
					lockFocusing = true;
				}
				if (contentSizeChangeTimer <= 0f)
				{
					RefreshDragScrollersContentSize(0.1f, p_updateScrollAbility: true);
					lockFocusing = false;
				}
				return;
			}
			switch (mode)
			{
			case NavigationModeType.Focus:
			case NavigationModeType.Controller:
			case NavigationModeType.Keyboard:
				UpdateFocusScroll();
				break;
			case NavigationModeType.Mouse:
				if (scrollMouseWheel)
				{
					if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f)
					{
						StartMouseWheelScrollNavigation();
					}
					else
					{
						if (dragDisableTimer > 0f)
						{
							dragDisableTimer -= Time.deltaTime;
						}
						if (dragDisableTimer <= 0f)
						{
							StopMouseWheelScrollNavigation();
						}
					}
					if (dragDisableTimer > 0f)
					{
						UpdateMouseWheelScroll();
					}
				}
				if (scrollClickAndDrag)
				{
					if ((bool)dragScroller)
					{
						dragScroller.horizontal = CanScrollHorizontally();
						dragScroller.vertical = CanScrollVertically();
					}
				}
				else if ((bool)dragScroller && dragScroller.gameObject.activeInHierarchy)
				{
					StopDragScrollNavigation();
				}
				break;
			}
		}

		protected void UpdateFocusScroll(float p_duration = 0f, Vector2? p_diff = null)
		{
			UINavigation uINavigation = UINavigation.focus;
			if (!uINavigation || !uINavigation.transform.IsChildOf(container.transform))
			{
				return;
			}
			Vector2 vector = Vector2.zero;
			if (p_diff.HasValue)
			{
				vector = p_diff.Value;
			}
			Vector2 sizeDelta = container.parent.GetComponent<RectTransform>().sizeDelta;
			bool flag = !CanScrollVertically();
			bool flag2 = !CanScrollHorizontally();
			if (flag2 && flag)
			{
				return;
			}
			if (p_duration == 0f)
			{
				p_duration = focus.duration;
			}
			Vector3[] bounds_corners = m_bounds_corners;
			Vector3[] focus_corners = m_focus_corners;
			Vector3[] viewrect_corners = m_viewrect_corners;
			RectTransform rectTransform = (boundsRT ? boundsRT : ((RectTransform)bounds.transform));
			RectTransform obj = (RectTransform)uINavigation.transform;
			RectTransform rectTransform2 = viewrect;
			rectTransform2.GetWorldCorners(viewrect_corners);
			obj.GetWorldCorners(focus_corners);
			rectTransform.GetWorldCorners(bounds_corners);
			DebugCorners(viewrect_corners, Color.yellow);
			DebugCorners(focus_corners, Color.magenta);
			Vector3 vector2 = (viewrect_corners[0] + viewrect_corners[1] + viewrect_corners[2] + viewrect_corners[3]) * 0.25f;
			Vector3 vector3 = (focus_corners[0] + focus_corners[1] + focus_corners[2] + focus_corners[3]) * 0.25f;
			Vector3 position = bounds_corners[0];
			Vector2 vector4 = rectTransform2.InverseTransformPoint(vector2);
			Vector2 vector5 = rectTransform2.InverseTransformPoint(vector3);
			_ = (Vector2)rectTransform2.InverseTransformPoint(position);
			_ = (Vector2)rectTransform2.InverseTransformPoint(viewrect_corners[0]);
			localFocusCenter = vector5;
			localViewRectCenter = vector4;
			Debug.DrawLine(vector2, vector3, Color.red);
			Vector3 localPosition = container.localPosition;
			float x = vector4.x - vector5.x;
			float y = vector4.y - vector5.y;
			if (!flag2)
			{
				scrollFocus.x = x;
			}
			if (!flag)
			{
				scrollFocus.y = y;
			}
			if (!flag2)
			{
				localPosition.x += scrollFocus.x + offset.x;
			}
			if (!flag)
			{
				localPosition.y += scrollFocus.y + offset.y;
			}
			Vector3 localPosition2 = container.localPosition;
			container.localPosition = localPosition;
			Vector2 anchoredPosition = container.anchoredPosition;
			container.localPosition = localPosition2;
			Vector2 sizeDelta2 = rectTransform.sizeDelta;
			Vector2 vector6 = new Vector2(Screen.width, Screen.height);
			Rect rect = RectTransformUtility.PixelAdjustRect(viewrect, canvas);
			vector6.x = rect.width;
			vector6.y = rect.height;
			Vector2 vector7 = vector6 - sizeDelta2;
			localPosition = anchoredPosition;
			if (!flag2)
			{
				localPosition.x = Mathf.Max(localPosition.x, vector7.x);
			}
			if (!flag2)
			{
				localPosition.x = Mathf.Min(localPosition.x, 0f);
			}
			if (!flag)
			{
				localPosition.y = 0f - Mathf.Max(localPosition.y, vector7.y);
			}
			if (!flag)
			{
				localPosition.y = 0f - Mathf.Min(localPosition.y, 0f);
			}
			anchoredPosition = localPosition;
			if (!flag)
			{
				if (anchoredPosition.y <= -20f)
				{
					anchoredPosition.y = -20f;
				}
				else
				{
					float num = container.sizeDelta.y + vector.y - sizeDelta.y - 31.035f;
					if (num > 0f && anchoredPosition.y > num)
					{
						anchoredPosition.y = num;
					}
				}
			}
			Tween.Add(container, "anchoredPosition", anchoredPosition, p_duration, Quad.In);
		}

		private Rect CalculateScrollableContentRect()
		{
			Vector3[] array = new Vector3[4];
			(boundsRT ? boundsRT : ((RectTransform)bounds.transform)).GetWorldCorners(array);
			Rect result = Rect.MinMaxRect(array[0].x, array[0].y, array[2].x, array[2].y);
			if (containerSizeCalculationTargets.Length != 0)
			{
				if (containerSizeCalculationTargets.Length == 1)
				{
					if (containerSizeCalculationTargets[0] != null)
					{
						containerSizeCalculationTargets[0].GetWorldCorners(array);
						result = Rect.MinMaxRect(array[0].x, array[0].y, array[2].x, array[2].y);
					}
				}
				else
				{
					float num = 10000f;
					float num2 = 10000f;
					float num3 = -10000f;
					float num4 = -10000f;
					bool flag = false;
					for (int i = 0; i < containerSizeCalculationTargets.Length; i++)
					{
						if (!(containerSizeCalculationTargets[i] == null) && containerSizeCalculationTargets[i].gameObject.activeInHierarchy)
						{
							containerSizeCalculationTargets[i].GetWorldCorners(array);
							if (array[0].x < num)
							{
								num = array[0].x;
							}
							if (array[0].y < num2)
							{
								num2 = array[0].y;
							}
							if (array[2].x > num3)
							{
								num3 = array[2].x;
							}
							if (array[2].y > num4)
							{
								num4 = array[2].y;
							}
							flag = true;
						}
					}
					if (flag)
					{
						result = Rect.MinMaxRect(num, num2, num3, num4);
					}
				}
			}
			return result;
		}

		public void SetContentSizeChanging(float p_time, Vector2? p_contentSizeDiff = null)
		{
			contentSizeChangeTimer = p_time;
			contentSizeDifference = p_contentSizeDiff;
			if (p_time > 0f)
			{
				UINavigation.DisableFocusForTime(p_time);
				StopDragScrollNavigation();
				StopMouseWheelScrollNavigation();
			}
		}

		public void RefreshDragScrollersContentSize(float p_delay = 0f, bool p_updateScrollAbility = false)
		{
			if (!containerNeedsSizeUpdate || dragScroller == null || dragScroller.content == null)
			{
				return;
			}
			if (p_updateScrollAbility)
			{
				dragScroller.horizontal = CanScrollHorizontally();
				dragScroller.vertical = CanScrollVertically();
			}
			Activity.Run(delegate
			{
				RectTransform obj = (boundsRT ? boundsRT : ((RectTransform)bounds.transform));
				float x = obj.sizeDelta.x;
				float y = obj.sizeDelta.y;
				if (scrollX)
				{
					dragScroller.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x + dragScrollerContentSizeAdjustment.x);
				}
				if (scrollY)
				{
					dragScroller.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y + dragScrollerContentSizeAdjustment.y);
				}
				StartDragScrollNavigation();
			}, 0f, p_delay);
		}

		public bool IsContentSizeChanging()
		{
			return contentSizeChangeTimer > 0f;
		}

		public void UpdateJoystickPan()
		{
			dragScroller.normalizedPosition += new Vector2(RCI.GetAssignedAxis(RawAxis.RightStickX), RCI.GetAssignedAxis(RawAxis.RightStickY)) * panSpeed * Time.deltaTime;
		}

		public bool CanDragScroll()
		{
			if (UINavigation.IsFocusDisabled() || !scrollClickAndDrag || IsContentSizeChanging())
			{
				return false;
			}
			if (scrollMouseWheel)
			{
				return dragDisableTimer <= 0f;
			}
			return true;
		}

		private bool CanScroll(bool p_horizontally)
		{
			bool result = (p_horizontally ? scrollX : scrollY);
			if (viewrect != null)
			{
				Rect rect = CalculateScrollableContentRect();
				if (p_horizontally)
				{
					viewrect.GetWorldCorners(m_corners);
					float x = m_corners[0].x;
					float x2 = m_corners[2].x;
					result = scrollX && (rect.xMin < x || rect.xMax > x2);
				}
				else
				{
					viewrect.GetWorldCorners(m_corners);
					float y = m_corners[0].y;
					float y2 = m_corners[2].y;
					result = scrollY && (rect.yMin < y || rect.yMax > y2);
				}
			}
			return result;
		}

		public bool CanScrollHorizontally()
		{
			if (forceScrollX)
			{
				return true;
			}
			if (!scrollX)
			{
				return false;
			}
			return CanScroll(p_horizontally: true);
		}

		public bool CanScrollVertically()
		{
			if (forceScrollY)
			{
				return true;
			}
			if (!scrollY)
			{
				return false;
			}
			return CanScroll(p_horizontally: false);
		}

		public bool StartDragScrollNavigation()
		{
			if (!CanDragScroll() || dragScroller == null)
			{
				return false;
			}
			bool flag = CanScrollHorizontally();
			bool flag2 = CanScrollVertically();
			if (areaType == ScrollAreaType.Screen && (flag2 || flag))
			{
				RectTransform component = GetComponent<RectTransform>();
				float width = component.rect.width;
				float height = component.rect.height;
				RectTransform obj = (boundsRT ? boundsRT : ((RectTransform)bounds.transform));
				float x = obj.sizeDelta.x;
				float y = obj.sizeDelta.y;
				component.anchorMin = new Vector2(0f, 0f);
				component.anchorMax = new Vector2(0f, 1f);
				if (scrollX)
				{
					Vector2 sizeDelta = component.sizeDelta;
					sizeDelta.x = x;
					component.sizeDelta = sizeDelta;
					component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
				}
				if (scrollY)
				{
					Vector2 sizeDelta2 = component.sizeDelta;
					sizeDelta2.y = y;
					component.sizeDelta = sizeDelta2;
					component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
				}
			}
			dragScroller.gameObject.SetActive(flag2 || flag);
			return flag2 || flag;
		}

		public void StopDragScrollNavigation()
		{
			if (!dragScroller || !dragScroller.gameObject.activeInHierarchy)
			{
				return;
			}
			dragScroller.gameObject.SetActive(value: false);
			RectTransform component = GetComponent<RectTransform>();
			float width = component.rect.width;
			float height = component.rect.height;
			if (areaType == ScrollAreaType.Screen)
			{
				component.anchorMin = new Vector2(0f, 0f);
				component.anchorMax = new Vector2(1f, 1f);
				if (dragScroller.horizontal)
				{
					Vector2 sizeDelta = component.sizeDelta;
					sizeDelta.x = 0f;
					component.sizeDelta = sizeDelta;
					component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
				}
				if (dragScroller.vertical)
				{
					Vector2 sizeDelta2 = component.sizeDelta;
					sizeDelta2.y = 0f;
					component.sizeDelta = sizeDelta2;
					component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
				}
			}
		}

		public void OnInitializePotentialDrag(PointerEventData data)
		{
			if (scrollClickAndDrag && (bool)dragScroller)
			{
				Debug.Log("UINavigationScroll> OnInitializePotentialDrag");
				dragScrollAllowed = StartDragScrollNavigation();
				if (dragScrollAllowed)
				{
					data.hovered.Clear();
					data.hovered.Add(dragScroller.gameObject);
					ExecuteEvents.Execute(dragScroller.gameObject, data, ExecuteEvents.initializePotentialDrag);
				}
			}
		}

		public void OnBeginDrag(PointerEventData data)
		{
			if (dragScrollAllowed && scrollClickAndDrag && (bool)dragScroller)
			{
				data.hovered.Clear();
				data.hovered.Add(dragScroller.gameObject);
				ExecuteEvents.Execute(dragScroller.gameObject, data, ExecuteEvents.beginDragHandler);
			}
		}

		public void OnEndDrag(PointerEventData data)
		{
			if (dragScrollAllowed && scrollClickAndDrag && (bool)dragScroller)
			{
				data.hovered.Clear();
				data.hovered.Add(dragScroller.gameObject);
				ExecuteEvents.Execute(dragScroller.gameObject, data, ExecuteEvents.endDragHandler);
			}
		}

		public void OnDrag(PointerEventData data)
		{
			if (dragScrollAllowed && scrollClickAndDrag && (bool)dragScroller)
			{
				data.hovered.Clear();
				data.hovered.Add(dragScroller.gameObject);
				ExecuteEvents.Execute(dragScroller.gameObject, data, ExecuteEvents.dragHandler);
			}
		}

		public bool CanMouseWheelScroll()
		{
			if (UINavigation.IsFocusDisabled() || !scrollMouseWheel || IsContentSizeChanging())
			{
				return false;
			}
			return true;
		}

		public void StartMouseWheelScrollNavigation()
		{
			if (CanMouseWheelScroll())
			{
				if (scrollClickAndDrag && (bool)dragScroller && dragScroller.gameObject.activeInHierarchy)
				{
					StopDragScrollNavigation();
				}
				dragDisableTimer = Mathf.Max(mouse.wheelDuration, 0.025f);
			}
		}

		public void StopMouseWheelScrollNavigation()
		{
			dragDisableTimer = -1f;
		}

		private void UpdateMouseWheelScroll()
		{
			RectTransform obj = (boundsRT ? boundsRT : ((RectTransform)bounds.transform));
			RectTransform rectTransform = viewrect;
			Vector2 sizeDelta = obj.sizeDelta;
			Vector2 vector = new Vector2(Screen.width, Screen.height);
			Rect rect = RectTransformUtility.PixelAdjustRect(container, canvas);
			Rect rect2 = RectTransformUtility.PixelAdjustRect(rectTransform, root);
			vector.x = rect.width;
			vector.y = rect.height;
			_ = vector - sizeDelta;
			float width = rect2.width;
			float height = rect2.height;
			float num = width;
			float num2 = height;
			float num3 = 0f;
			float num4 = 0f;
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (scrollX && !scrollY)
			{
				num3 += axis * mouse.wheelSpeed;
			}
			if (scrollY && !scrollX)
			{
				num4 += axis * mouse.wheelSpeed;
			}
			float num5 = num3 * num;
			float num6 = num4 * num2;
			float max = Mathf.Max(sizeDelta.x - vector.x, 0f);
			float max2 = Mathf.Max(sizeDelta.y - vector.y, 0f);
			scroll.x += (CanScrollHorizontally() ? num5 : 0f) * Time.fixedDeltaTime;
			scroll.y += (CanScrollVertically() ? num6 : 0f) * Time.fixedDeltaTime;
			if (CanScrollHorizontally())
			{
				scroll.x = Mathf.Clamp(scroll.x, 0f, max);
			}
			if (CanScrollVertically())
			{
				scroll.y = Mathf.Clamp(scroll.y, 0f, max2);
			}
			Vector2 anchoredPosition = container.anchoredPosition;
			if (CanScrollHorizontally())
			{
				anchoredPosition.x = 0f - (scroll.x + offset.x);
			}
			if (CanScrollVertically())
			{
				anchoredPosition.y = scroll.y + offset.y;
			}
			if (mouse.wheelDuration <= 0f)
			{
				container.anchoredPosition = anchoredPosition;
			}
			else
			{
				container.anchoredPosition = Vector2.Lerp(container.anchoredPosition, anchoredPosition, Time.fixedDeltaTime / mouse.wheelDuration);
			}
		}

		protected void DebugCorners(Vector3[] l, Color p_color)
		{
			Debug.DrawLine(l[0], l[1], p_color);
			Debug.DrawLine(l[1], l[2], p_color);
			Debug.DrawLine(l[2], l[3], p_color);
			Debug.DrawLine(l[3], l[0], p_color);
		}
	}
}
