using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICardView : UIElementView<DRLApp>
	{
		private FadeComponent m_menu_fade;

		private bool m_menu_open;

		private Activity m_menu_watch;

		public virtual UICardType type => UICardType.None;

		public virtual bool selected
		{
			get
			{
				Transform transform = base.transform.Find("outline");
				if (!transform)
				{
					return false;
				}
				return transform.gameObject.activeInHierarchy;
			}
			set
			{
				Transform transform = base.transform.Find("outline");
				if ((bool)transform)
				{
					transform.gameObject.SetActive(value);
				}
			}
		}

		public FadeComponent menu
		{
			get
			{
				if ((bool)m_menu_fade)
				{
					return m_menu_fade;
				}
				Transform transform = base.transform.Find("content");
				if ((bool)transform)
				{
					transform = transform.Find("menu");
				}
				if (!transform)
				{
					return null;
				}
				return m_menu_fade = transform.GetComponent<FadeComponent>();
			}
			set
			{
				m_menu_fade = value;
			}
		}

		public virtual void Build()
		{
			base.name = GetType().Name;
			List<UICardLayoutElement> gc = new List<UICardLayoutElement>();
			Hierarchy.Traverse(base.transform, delegate(UICardLayoutElement it)
			{
				if (!it.Contains(UICardType.All) && !it.Contains(type))
				{
					gc.Add(it);
				}
			});
			for (int num = 0; num < gc.Count; num++)
			{
				if ((bool)gc[num])
				{
					gc[num].Destroy();
				}
			}
			gc.Clear();
			RectTransform obj = (RectTransform)base.transform;
			obj.anchoredPosition = new Vector2(0f, 0f);
			obj.sizeDelta = new Vector2(500f, 500f);
		}

		protected virtual bool CanOpenMenu()
		{
			if (menu != null)
			{
				return !m_menu_open;
			}
			return false;
		}

		public void OpenMenu()
		{
			if (!CanOpenMenu())
			{
				return;
			}
			if (m_menu_watch != null)
			{
				m_menu_watch.Stop();
			}
			m_menu_open = true;
			menu.gameObject.SetActive(value: true);
			menu.FadeIn(0.1f, 0f, Cubic.Out);
			UINavigation.Focus(menu);
			m_menu_watch = Activity.Run(delegate(float t)
			{
				if (t <= 0.01f)
				{
					return true;
				}
				if (!m_menu_open)
				{
					return false;
				}
				UINavigation focus = UINavigation.focus;
				if (!focus)
				{
					return false;
				}
				if (!focus.transform.IsChildOf(menu.transform))
				{
					CloseMenu();
					return false;
				}
				return true;
			});
		}

		public void CloseMenu()
		{
			if (m_menu_watch != null)
			{
				m_menu_watch.Stop();
			}
			if ((bool)menu && m_menu_open)
			{
				m_menu_open = false;
				menu.FadeOut(0.1f, 0f, Cubic.Out);
			}
		}

		public override void OnFocus()
		{
			base.OnFocus();
			CloseMenu();
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			if ((bool)menu)
			{
				UINavigation focus = UINavigation.focus;
				if (!focus)
				{
					CloseMenu();
				}
				else if (!focus.transform.IsChildOf(menu.transform))
				{
					CloseMenu();
				}
			}
		}
	}
}
