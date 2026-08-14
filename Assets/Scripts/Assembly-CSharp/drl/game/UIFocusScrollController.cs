using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	[RequireComponent(typeof(ScrollRect))]
	public class UIFocusScrollController : Controller<DRLApp>
	{
		[Tooltip("Set this 'true' if items in scroll view are showing bottom to top")]
		public bool bottomToTop;

		[Range(1f, 0.6f)]
		public float clampTopRange = 0.9f;

		[Range(0f, 0.4f)]
		public float clampBottomRange = 0.1f;

		private NavigationModeType m_navigationMode = NavigationModeType.Mouse;

		public ScrollRect scroll => AssertLocal<ScrollRect>("scroll");

		private void OnEnable()
		{
			UIScreenManagerController component = base.app.view.ui.screens.GetComponent<UIScreenManagerController>();
			if (component != null)
			{
				m_navigationMode = component.navigationMode;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			bool flag = p_event.Contains("@focus");
			bool flag2 = p_event.Contains("@scroll");
			if (flag || flag2)
			{
				if (m_navigationMode == NavigationModeType.Mouse && flag)
				{
					return;
				}
				UIElementView uIElementView = p_target as UIElementView;
				if ((bool)uIElementView && CheckIfChild(uIElementView.transform, scroll.content.transform))
				{
					if (flag)
					{
						OnFocusScroll(uIElementView.transform);
						return;
					}
					if (!(p_data[0] is PointerEventData data))
					{
						return;
					}
					OnMouseWheelScroll(data);
				}
			}
			if (p_event != null && p_event == "ui.screen.navigation-mode@change")
			{
				m_navigationMode = (NavigationModeType)p_data[0];
				if (m_navigationMode == NavigationModeType.Focus)
				{
					m_navigationMode = NavigationModeType.Controller;
				}
			}
		}

		private void OnFocusScroll(Transform p_targetItem)
		{
			RectTransform rectTransform = scroll.viewport;
			if (rectTransform == null)
			{
				rectTransform = base.transform.GetComponent<RectTransform>();
			}
			if (!(rectTransform == null))
			{
				Vector2 vector = rectTransform.InverseTransformPoint(scroll.content.TransformPoint(Vector2.zero));
				Vector2 vector2 = rectTransform.InverseTransformPoint(p_targetItem.position);
				Vector2 normalizedPosition = (bottomToTop ? (vector2 - vector) : (vector - vector2));
				if (scroll.content.sizeDelta.x > 0f)
				{
					normalizedPosition.x /= scroll.content.sizeDelta.x;
				}
				else
				{
					normalizedPosition.x = 0.5f;
				}
				if (scroll.content.sizeDelta.y > 0f)
				{
					normalizedPosition.y /= scroll.content.sizeDelta.y;
				}
				else
				{
					normalizedPosition.y = 0.5f;
				}
				if (normalizedPosition.y > clampTopRange)
				{
					normalizedPosition.y = 1f;
				}
				if (normalizedPosition.y < clampBottomRange)
				{
					normalizedPosition.y = 0f;
				}
				if (!bottomToTop)
				{
					normalizedPosition.y = 1f - normalizedPosition.y;
				}
				scroll.normalizedPosition = normalizedPosition;
			}
		}

		private void OnMouseWheelScroll(PointerEventData data)
		{
			scroll.OnScroll(data);
		}

		private bool CheckIfChild(Transform p_child, Transform p_parent)
		{
			if (p_child.parent == p_parent)
			{
				return true;
			}
			if (p_child.parent != null)
			{
				return CheckIfChild(p_child.parent, p_parent);
			}
			return false;
		}
	}
}
