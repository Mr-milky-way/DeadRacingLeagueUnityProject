using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIScreenView : View<DRLApp>
	{
		public MonoBehaviour caller;

		public UIScreen screen => AssertLocal<UIScreen>("screen");

		public bool current => base.app.view.ui.screens.current == screen;

		public bool visible
		{
			get
			{
				if ((bool)screen && screen.alpha <= 0f)
				{
					return false;
				}
				return base.gameObject.activeSelf;
			}
			set
			{
				base.gameObject.SetActive(value);
			}
		}

		public float alpha
		{
			get
			{
				if (!screen)
				{
					return 0f;
				}
				return screen.alpha;
			}
			set
			{
				if ((bool)screen)
				{
					screen.alpha = value;
				}
			}
		}

		public UINavigation leftNavigation
		{
			get
			{
				UINavigation uINavigation = null;
				uINavigation = Find<UINavigation>("columns.left.nav");
				if (!uINavigation)
				{
					uINavigation = Find<UINavigation>("left.nav");
				}
				return uINavigation;
			}
		}

		public UINavigation rightNavigation
		{
			get
			{
				UINavigation uINavigation = null;
				uINavigation = Find<UINavigation>("columns.right.nav");
				if (!uINavigation)
				{
					uINavigation = Find<UINavigation>("right.nav");
				}
				return uINavigation;
			}
		}

		public RectTransform leftColumn
		{
			get
			{
				RectTransform rectTransform = null;
				rectTransform = Find<RectTransform>("columns.left");
				if (!rectTransform)
				{
					rectTransform = Find<RectTransform>("left");
				}
				return rectTransform;
			}
		}

		public RectTransform rightColumn
		{
			get
			{
				RectTransform rectTransform = null;
				rectTransform = Find<RectTransform>("columns.right");
				if (!rectTransform)
				{
					rectTransform = Find<RectTransform>("right");
				}
				return rectTransform;
			}
		}

		public UINavigationScroll scroll => AssertLocal<UINavigationScroll>("scroll");

		public static implicit operator UIScreen(UIScreenView b)
		{
			if (!b)
			{
				return null;
			}
			return b.screen;
		}

		public List<UINavigation> GetSideNavigations(string p_side, bool p_ignore_disabled = false)
		{
			List<UINavigation> list = new List<UINavigation>();
			Transform transform = base.transform.Find("columns");
			if (!transform)
			{
				transform = base.transform;
			}
			transform = transform.Find(p_side);
			if (!transform)
			{
				return list;
			}
			list.AddRange(Hierarchy.FindAll<UINavigation>(transform));
			if (p_ignore_disabled)
			{
				list.RemoveAll((UINavigation it) => !it.gameObject.activeInHierarchy);
			}
			return list;
		}

		public List<UINavigation> GetLeftSideNavigations(bool p_ignore_disabled = false)
		{
			return GetSideNavigations("left", p_ignore_disabled);
		}

		public List<UINavigation> GetRightSideNavigations(bool p_ignore_disabled = false)
		{
			return GetSideNavigations("right", p_ignore_disabled);
		}

		public virtual string BackButtonPressedEvent()
		{
			return "ui.screen.return@click";
		}
	}
}
