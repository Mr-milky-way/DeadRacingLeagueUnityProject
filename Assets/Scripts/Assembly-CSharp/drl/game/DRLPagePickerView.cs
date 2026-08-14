using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLPagePickerView : UIElementView
	{
		public ListComponent listField;

		public HorizontalLayoutGroup layout;

		public UIElementView leftArrow;

		public UIElementView rightArrow;

		public DRLPagePickerItemView selection;

		public int visibleCount = 8;

		public float itemOffset = 55f;

		public int pageOffset = 10;

		public float duration = 0.5f;

		private Activity m_unfocus_timer;

		private float m_left_offset;

		private Vector2 m_anchor_pos;

		private int m_index;

		private bool m_focused;

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public UINavigation nav => AssertLocal<UINavigation>("nav");

		protected float leftOffset
		{
			get
			{
				return m_left_offset;
			}
			set
			{
				m_left_offset = value;
				RectTransform obj = (RectTransform)layout.transform;
				Vector2 anchor_pos = m_anchor_pos;
				anchor_pos.x = m_left_offset;
				obj.anchoredPosition = anchor_pos;
				layout.enabled = false;
				layout.enabled = true;
			}
		}

		public int index
		{
			get
			{
				if (!selection)
				{
					Set(m_index);
					DRLPagePickerItemView dRLPagePickerItemView = listField.Get<DRLPagePickerItemView>(m_index);
					selection = dRLPagePickerItemView;
				}
				return m_index;
			}
			set
			{
				m_index = value;
				if (m_index >= 0)
				{
					DRLPagePickerItemView dRLPagePickerItemView = listField.Get<DRLPagePickerItemView>(m_index);
					if ((bool)selection)
					{
						selection.selected = false;
					}
					selection = dRLPagePickerItemView;
					if ((bool)selection)
					{
						selection.selected = true;
					}
					RefreshCenter(m_index);
				}
			}
		}

		public int total => listField.Count;

		protected void Awake()
		{
			m_anchor_pos = ((RectTransform)layout.transform).anchoredPosition;
		}

		public void Set(int p_page_count)
		{
			listField.Clear();
			int num = ((p_page_count > 0) ? (m_index + 31) : 0);
			if (m_index + 31 >= p_page_count)
			{
				num = p_page_count;
			}
			List<UINavigation> list = new List<UINavigation>();
			HorizontalLayoutGroup component = listField.GetComponent<HorizontalLayoutGroup>();
			for (int i = 0; i < num; i++)
			{
				DRLPagePickerItemView dRLPagePickerItemView = listField.Push<DRLPagePickerItemView>();
				dRLPagePickerItemView.label = (i + 1).ToString();
				dRLPagePickerItemView.selected = i == m_index;
				dRLPagePickerItemView.width = itemOffset - (component ? component.spacing : 0f);
				if (dRLPagePickerItemView.selected)
				{
					selection = dRLPagePickerItemView;
				}
				UINavigation component2 = dRLPagePickerItemView.GetComponent<UINavigation>();
				list.Add(component2);
			}
			UINavigation component3 = GetComponent<UINavigation>();
			UINavigation.Link(layout, null, null, component3.GetUp(), component3.GetDown());
			float num2 = duration;
			duration = 0f;
			RefreshCenter(m_index);
			duration = num2;
			if (num == 0)
			{
				if ((bool)leftArrow)
				{
					leftArrow.interactable = false;
				}
				if ((bool)rightArrow)
				{
					rightArrow.interactable = false;
				}
			}
		}

		protected void RefreshCenter(int p_index)
		{
			float num = itemOffset;
			float num2 = listField.Count;
			int num3 = visibleCount / 2;
			int num4 = -p_index + num3;
			if ((bool)leftArrow)
			{
				leftArrow.interactable = p_index > 0;
			}
			if ((bool)rightArrow)
			{
				rightArrow.interactable = (float)(p_index + 1) < num2;
			}
			if (num2 <= 0f)
			{
				leftOffset = 0f;
				return;
			}
			if (num2 <= (float)visibleCount)
			{
				leftOffset = 0f;
				return;
			}
			float a = num * (float)num4;
			float b = 0f;
			float b2 = (0f - (num2 - (float)num3 - (float)num3 - 1f)) * num;
			a = Mathf.Min(a, b);
			a = Mathf.Max(a, b2);
			if (duration <= 0f)
			{
				leftOffset = a;
			}
			else
			{
				Tween.Add(this, "leftOffset", a, duration, 0f, Cubic.Out);
			}
		}

		internal void OnItemClick(DRLPagePickerItemView p_item)
		{
			int num = (index = p_item.transform.GetSiblingIndex());
			Notify(notification + "@select", num);
		}

		internal void OnItemFocus(DRLPagePickerItemView p_item)
		{
			int siblingIndex = p_item.transform.GetSiblingIndex();
			if (!Cursor.visible)
			{
				RefreshCenter(siblingIndex);
			}
			if (m_unfocus_timer != null)
			{
				m_unfocus_timer.Stop();
			}
			m_focused = true;
		}

		internal void OnItemUnfocus(DRLPagePickerItemView p_item)
		{
			int siblingIndex = p_item.transform.GetSiblingIndex();
			if (!Cursor.visible)
			{
				RefreshCenter(siblingIndex);
			}
			if (m_unfocus_timer != null)
			{
				m_unfocus_timer.Stop();
			}
			m_focused = false;
		}

		public override void OnFocus()
		{
			base.OnFocus();
			if (!selection && total > 0)
			{
				selection = listField.Get<DRLPagePickerItemView>(0);
			}
			if (!Cursor.visible && (bool)selection)
			{
				UINavigation.focus = selection.GetComponent<UINavigation>();
			}
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			RefreshCenter(index);
		}

		protected void Update()
		{
			if ((bool)nav)
			{
				nav.enabled = !Cursor.visible;
			}
			if (!m_focused)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			int a = pageOffset;
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				flag = true;
			}
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				flag2 = true;
			}
			if (RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true))
			{
				flag = true;
			}
			if (RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: false))
			{
				flag2 = true;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis < 0f)
			{
				a = 4;
				flag = true;
			}
			if (axis > 0f)
			{
				a = 4;
				flag2 = true;
			}
			a = Mathf.Max(a, 1);
			if (!flag && !flag2)
			{
				return;
			}
			if (flag)
			{
				flag2 = false;
			}
			UINavigation uINavigation = selection.GetComponent<UINavigation>();
			UINavigation uINavigation2 = uINavigation;
			for (int i = 0; i < a; i++)
			{
				Component component = (flag ? uINavigation.right : (flag2 ? uINavigation.left : null));
				if (!component || !(component is UINavigation))
				{
					break;
				}
				uINavigation = (UINavigation)component;
			}
			if ((bool)uINavigation && uINavigation != uINavigation2)
			{
				index = uINavigation.transform.GetSiblingIndex();
				UINavigation.focus = uINavigation;
			}
		}
	}
}
