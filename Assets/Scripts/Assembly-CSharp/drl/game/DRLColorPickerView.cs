using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLColorPickerView : NotificationView<DRLApp>
	{
		public ListComponent items;

		[SerializeField]
		private List<Color> m_colors;

		private Color m_current;

		public List<Color> colors
		{
			get
			{
				if (m_colors == null)
				{
					m_colors = new List<Color>();
				}
				return m_colors;
			}
			set
			{
				if (m_colors == null)
				{
					m_colors = new List<Color>();
				}
				value = ((value == null) ? new List<Color>() : value);
				m_colors.Clear();
				items.Clear();
				m_colors.AddRange(value);
				for (int i = 0; i < m_colors.Count; i++)
				{
					Color p_color = m_colors[i];
					UIElementView uIElementView = items.Push<UIElementView>();
					Button component = uIElementView.GetComponent<Button>();
					component.onClick.RemoveAllListeners();
					component.onClick.AddListener(GetColorItemHandler(i));
					SetItemColor(uIElementView, p_color);
					SetItemSelect(uIElementView, p_flag: false);
				}
			}
		}

		public Color current
		{
			get
			{
				return m_current;
			}
			set
			{
				int colorIndex = Colorf.GetColorIndex(value, m_colors);
				OnChange(colorIndex);
			}
		}

		protected void Awake()
		{
			colors = m_colors;
			if (m_colors.Count > 0)
			{
				SetCurrent(0);
			}
		}

		public void Invalidate()
		{
			for (int i = 0; i < items.Count; i++)
			{
				SetItemSelect(i, p_flag: false);
			}
			m_current = Color.black;
		}

		public void SetCurrent(int p_index)
		{
			Invalidate();
			if (p_index >= 0)
			{
				m_current = GetItemColor(p_index);
				SetItemSelect(p_index, p_flag: true, p_fade: true);
			}
		}

		public void SetCurrent(Color p_color)
		{
			int colorIndex = Colorf.GetColorIndex(p_color, colors);
			SetCurrent(colorIndex);
		}

		private UnityAction GetColorItemHandler(int p_index)
		{
			return delegate
			{
				OnColorItemClick(p_index);
			};
		}

		private void OnColorItemClick(int p_index)
		{
			current = GetItemColor(p_index);
		}

		protected virtual void OnChange(int p_index)
		{
			if (base.enabled)
			{
				SetCurrent(p_index);
				Notify(notification + "@change", m_current, p_index);
			}
		}

		private void SetItemColor(UIElementView p_item, Color p_color)
		{
			if ((bool)p_item)
			{
				Hierarchy.GetComponent<Image>(p_item.transform.Find("image").gameObject).color = p_color;
			}
		}

		private void SetItemColor(int p_index, Color p_color)
		{
			UIElementView p_item = items.Get<UIElementView>(p_index);
			SetItemColor(p_item, p_color);
		}

		private Color GetItemColor(UIElementView p_item)
		{
			if (!p_item)
			{
				return Color.black;
			}
			return Hierarchy.GetComponent<Image>(p_item.transform.Find("image").gameObject).color;
		}

		private Color GetItemColor(int p_index)
		{
			UIElementView p_item = items.Get<UIElementView>(p_index);
			return GetItemColor(p_item);
		}

		private void SetItemSelect(UIElementView p_item, bool p_flag, bool p_fade = false)
		{
			if ((bool)p_item)
			{
				FadeComponent component = Hierarchy.GetComponent<FadeComponent>(p_item.transform.Find("select").gameObject);
				if (p_fade)
				{
					component.Fade(p_flag ? 1f : (-0.1f), 0.25f);
				}
				else
				{
					component.alpha = (p_flag ? 1f : 0f);
				}
			}
		}

		private bool GetItemSelect(UIElementView p_item)
		{
			if (!p_item)
			{
				return false;
			}
			return Hierarchy.GetComponent<FadeComponent>(p_item.transform.Find("select").gameObject).alpha >= 1f;
		}

		private void SetItemSelect(int p_index, bool p_flag, bool p_fade)
		{
			UIElementView p_item = items.Get<UIElementView>(p_index);
			SetItemSelect(p_item, p_flag, p_fade);
		}

		private void SetItemSelect(int p_index, bool p_flag)
		{
			SetItemSelect(p_index, p_flag, p_fade: false);
		}

		private bool GetItemSelect(int p_index)
		{
			UIElementView p_item = items.Get<UIElementView>(p_index);
			return GetItemSelect(p_item);
		}
	}
}
