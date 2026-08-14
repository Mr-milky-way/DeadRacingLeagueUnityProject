using System;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIBaseSubmenuView : View<DRLApp>
	{
		public GameObject offlineOverlay;

		public RectTransform submenu;

		public LayoutElement submenuLayout;

		public UINavigation[] entryElements;

		public UINavigation[] exitElements;

		protected UINavigation mParentNav;

		protected Component mParentNextDown;

		protected Image mSubmenuToggleImg;

		protected Sprite mFoldedIcon;

		protected Sprite mUnFoldedIcon;

		public bool IsOpen { get; private set; }

		public bool Opening { get; private set; }

		public virtual void Setup(UISubmenuData data)
		{
			mParentNav = data.parentNav;
			mSubmenuToggleImg = data.submenuToggleImg;
			mFoldedIcon = data.foldedIcon;
			mUnFoldedIcon = data.unFoldedIcon;
		}

		public void SetDissabled(bool dissabled)
		{
			offlineOverlay.SetActive(!dissabled);
		}

		private void OnDisable()
		{
			Opening = false;
			Tween.Kill(submenu, "sizeDelta");
		}

		public virtual void UpdateSubmenuNavigation()
		{
			if (IsOpen)
			{
				UINavigation.focus = entryElements[0];
				UINavigation[] array = exitElements;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].down = mParentNav.down;
				}
				mParentNextDown = mParentNav.down;
				mParentNav.down = entryElements[0];
				array = entryElements;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].up = mParentNav;
				}
			}
			else
			{
				if (mParentNextDown != null)
				{
					mParentNav.down = mParentNextDown;
				}
				if (UINavigation.focus != null && mParentNav != null && mParentNav.transform.Find(UINavigation.focus.transform.name) != null)
				{
					UINavigation.focus = mParentNav;
				}
			}
		}

		public void SubmenuUnfold(float p_duration = 0.3f)
		{
			Notify("chat.ui.submenu.unfold", this);
			Opening = true;
			submenu.gameObject.SetActive(value: true);
			if (mSubmenuToggleImg != null)
			{
				mSubmenuToggleImg.sprite = mUnFoldedIcon;
			}
			Tween.Kill(submenuLayout, "preferredHeight");
			Tween.Kill(submenu, "sizeDelta");
			Tween.Add(submenuLayout, "preferredHeight", 45f, p_duration, Cubic.Out);
			Tween tween = Tween.Add(submenu, "sizeDelta", new Vector2(submenu.sizeDelta.x, 45f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				IsOpen = true;
				if (submenu != null)
				{
					UpdateSubmenuNavigation();
				}
				Opening = false;
			});
		}

		public void SubmenuFold(float p_duration = 0.3f)
		{
			Opening = false;
			submenu.transform.Find("stripe").gameObject.SetActive(value: false);
			if (mSubmenuToggleImg != null)
			{
				mSubmenuToggleImg.sprite = mFoldedIcon;
			}
			Tween.Kill(submenuLayout, "preferredHeight");
			Tween.Kill(submenu, "sizeDelta");
			Tween.Add(submenuLayout, "preferredHeight", 0f, p_duration, Cubic.Out);
			Tween tween = Tween.Add(submenu, "sizeDelta", new Vector2(submenu.sizeDelta.x, 0f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				if (submenu != null)
				{
					submenu.gameObject.SetActive(value: false);
				}
				IsOpen = false;
				UpdateSubmenuNavigation();
			});
		}

		private void OnDestroy()
		{
			Opening = false;
			Tween.Kill(submenu, "sizeDelta");
			Tween.Kill(submenuLayout, "preferredHeight");
		}
	}
}
