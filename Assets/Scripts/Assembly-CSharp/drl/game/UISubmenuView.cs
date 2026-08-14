using System;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISubmenuView : View
	{
		public RectTransform submenu;

		public Image submenuToggle;

		public Sprite[] submenuToggleIcons;

		public UIElementView[] submenuOfflineDisabledElements;

		public UINavigation[] submenuNav;

		private Component m_nextDown;

		private UINavigation mParentNav;

		private RectTransform mParentRect;

		public bool submenuOpened { get; set; }

		public void Setup(UINavigation parentNavigation, RectTransform parentRect)
		{
			mParentNav = parentNavigation;
			mParentRect = parentRect;
		}

		public void SetDissabled(bool dissabled)
		{
			UIElementView[] array = submenuOfflineDisabledElements;
			foreach (UIElementView obj in array)
			{
				obj.transform.parent.Find("offline-overlay").gameObject.SetActive(!dissabled);
				obj.enabled = dissabled;
			}
		}

		public void UpdateSubmenuNavigation(Component p_NextDown)
		{
			m_nextDown = p_NextDown;
			mParentNav.down = p_NextDown;
			if (submenuOpened)
			{
				mParentNav.down = submenuNav[0];
				submenuNav[2].down = m_nextDown;
				submenuNav[3].down = m_nextDown;
			}
		}

		public void SubmenuUnfold(float p_duration = 0.3f)
		{
			submenu.gameObject.SetActive(value: true);
			submenuToggle.sprite = submenuToggleIcons[1];
			submenu.transform.Find("stripe").gameObject.SetActive(value: true);
			Tween tween = Tween.Add(submenu, "sizeDelta", new Vector2(submenu.sizeDelta.x, 160f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				submenuOpened = true;
				UpdateSubmenuNavigation(m_nextDown);
			});
		}

		public void SubmenuFold(float p_duration = 0.3f)
		{
			submenu.transform.Find("stripe").gameObject.SetActive(value: false);
			submenuToggle.sprite = submenuToggleIcons[0];
			Tween tween = Tween.Add(submenu, "sizeDelta", new Vector2(submenu.sizeDelta.x, 70f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				submenu.gameObject.SetActive(value: false);
				submenuOpened = false;
				UpdateSubmenuNavigation(m_nextDown);
			});
		}
	}
}
