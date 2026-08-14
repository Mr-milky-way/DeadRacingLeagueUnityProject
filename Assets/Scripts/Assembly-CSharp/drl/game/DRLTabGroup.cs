using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTabGroup : UIElementView
	{
		public List<FadeComponent> tabs;

		public List<RectTransform> contents;

		public float fade = 0.1f;

		public float duration = 0.1f;

		public Color color;

		public bool useBackgroundChange;

		public FadeComponent[] selectionBackgrounds;

		public FadeComponent[] unselectedBackgrounds;

		private string m_selection;

		public string selection
		{
			get
			{
				return m_selection;
			}
			set
			{
				m_selection = value;
				int num = -1;
				for (int i = 0; i < tabs.Count; i++)
				{
					if (tabs[i].name == m_selection)
					{
						num = i;
						tabs[i].Fade(1f, duration);
						if (useBackgroundChange)
						{
							selectionBackgrounds[i].FadeIn(0f);
							unselectedBackgrounds[i].FadeOut(0f);
						}
					}
					else
					{
						tabs[i].Fade(fade, duration);
						if (useBackgroundChange && selectionBackgrounds[i].alpha >= 1f)
						{
							unselectedBackgrounds[i].FadeIn(0f);
							selectionBackgrounds[i].FadeOut(0f);
						}
					}
				}
				for (int j = 0; j < contents.Count; j++)
				{
					RectTransform rectTransform = contents[j];
					if ((bool)rectTransform)
					{
						FadeComponent component = rectTransform.GetComponent<FadeComponent>();
						if ((bool)component)
						{
							component.Fade((j == num) ? 1f : (-0.1f), duration);
						}
						else
						{
							rectTransform.gameObject.SetActive(j == num);
						}
					}
				}
				Notify(notification + "@change");
			}
		}

		public int index
		{
			get
			{
				for (int i = 0; i < tabs.Count; i++)
				{
					if (tabs[i].name == m_selection)
					{
						return i;
					}
				}
				return -1;
			}
			set
			{
				string text = "";
				if (value >= 0 && value < tabs.Count)
				{
					text = tabs[value].name;
				}
				selection = text;
			}
		}

		protected void Awake()
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				Button component = tabs[i].GetComponent<Button>();
				if ((bool)component)
				{
					component.onClick.AddListener(GetTabClickHandler(component.name));
				}
			}
		}

		protected virtual UnityAction GetTabClickHandler(string p_id)
		{
			return delegate
			{
				OnTabClick(p_id);
			};
		}

		protected virtual void OnTabClick(string p_id)
		{
			selection = p_id;
		}
	}
}
