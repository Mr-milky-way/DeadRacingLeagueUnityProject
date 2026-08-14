using System;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIFriendItemView : UIUserItemView
	{
		public RectTransform submenu;

		public Image submenuToggle;

		public Sprite[] submenuToggleIcons;

		public UIElementView messageButton;

		public Button inviteButton;

		public Button joinButton;

		public UINavigation navigation;

		public GameObject[] statusIcons;

		public UINavigation[] submenuNav;

		private UINavigation m_nextDown;

		public GameObject[] userRankBadges;

		public Text statusText;

		public Image background;

		public CanvasGroup canvasGroup;

		public CanvasGroup privateMessageCanvasGroup;

		public RectTransform focusRect;

		public GameObject removeFriendButton;

		public GameFriendStatusType status { get; set; }

		public bool submenuOpened { get; set; }

		public RectTransform rectTransform => AssertLocal<RectTransform>("rectTransform");

		public override void Set(GameFriendData p_data)
		{
			base.Set(p_data);
			SetStatus(p_data.status);
			SetUserRankBadge(p_data.profileRank);
		}

		public void SetStatus(GameFriendStatusType p_status)
		{
			bool ingame = base.gameFriendData.ingame;
			status = p_status;
			canvasGroup.alpha = (ingame ? 1f : 0.3f);
			privateMessageCanvasGroup.alpha = (ingame ? 1f : 0.3f);
			messageButton.interactable = ingame;
			statusText.text = (ingame ? Localization.instance.Get<string>("social.friends.online", "ONLINE") : Localization.instance.Get<string>("social.friends.offline", "OFFLINE"));
			if (!ingame)
			{
				inviteButton.transform.parent.Find("offline-overlay").gameObject.SetActive(value: true);
				inviteButton.interactable = false;
				joinButton.transform.parent.Find("offline-overlay").gameObject.SetActive(value: true);
				joinButton.interactable = false;
			}
			else
			{
				bool flag = base.app.model.network.room != null;
				inviteButton.transform.parent.Find("offline-overlay").gameObject.SetActive(!flag);
				inviteButton.interactable = flag;
				joinButton.transform.parent.Find("offline-overlay").gameObject.SetActive(value: true);
				joinButton.interactable = false;
			}
			GameObject[] array = statusIcons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			statusIcons[(!ingame) ? 6 : 0].SetActive(value: true);
		}

		public void SetNavigation(NavigationDirection p_direction, Component p_target)
		{
			switch (p_direction)
			{
			case NavigationDirection.down:
				navigation.down = p_target;
				break;
			case NavigationDirection.up:
				navigation.up = p_target;
				break;
			case NavigationDirection.left:
				navigation.left = p_target;
				break;
			case NavigationDirection.right:
				navigation.right = p_target;
				break;
			}
		}

		public void UpdateSubmenuNavigation(UINavigation p_NextDown)
		{
			m_nextDown = p_NextDown;
			navigation.down = p_NextDown;
			if (!submenuOpened)
			{
				if ((bool)m_nextDown)
				{
					m_nextDown.up = navigation;
				}
				return;
			}
			if (status != GameFriendStatusType.Offline)
			{
				navigation.down = submenuNav[0];
				if ((bool)m_nextDown)
				{
					m_nextDown.up = submenuNav[0];
				}
			}
			else
			{
				if ((bool)m_nextDown)
				{
					m_nextDown.up = submenuNav[3];
				}
				navigation.down = submenuNav[3];
			}
			submenuNav[2].down = m_nextDown;
			submenuNav[3].down = m_nextDown;
			submenuNav[0].down = submenuNav[2];
			submenuNav[1].down = submenuNav[3];
			submenuNav[2].up = submenuNav[0];
			submenuNav[3].up = submenuNav[1];
			submenuNav[0].up = navigation;
			submenuNav[1].up = navigation;
		}

		public void SubmenuUnfold(float p_duration = 0.3f)
		{
			Tween.Kill(rectTransform, "sizeDelta");
			Tween.Kill(focusRect, "offsetMin");
			submenu.gameObject.SetActive(value: true);
			submenuToggle.sprite = submenuToggleIcons[1];
			Tween tween = Tween.Add(rectTransform, "sizeDelta", new Vector2(submenu.sizeDelta.x, 135f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				submenuOpened = true;
				UpdateSubmenuNavigation(m_nextDown);
			});
			Tween.Add(focusRect, "offsetMin", new Vector2(focusRect.offsetMin.x, -70f), p_duration, Cubic.Out);
		}

		public void SubmenuFold(float p_duration = 0.3f)
		{
			Tween.Kill(rectTransform, "sizeDelta");
			Tween.Kill(focusRect, "offsetMin");
			submenu.transform.Find("stripe").gameObject.SetActive(value: false);
			submenuToggle.sprite = submenuToggleIcons[0];
			Tween tween = Tween.Add(rectTransform, "sizeDelta", new Vector2(submenu.sizeDelta.x, 75f), p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				submenu.gameObject.SetActive(value: false);
				submenuOpened = false;
				UpdateSubmenuNavigation(m_nextDown);
			});
			Tween.Add(focusRect, "offsetMin", new Vector2(focusRect.offsetMin.x, -10f), p_duration, Cubic.Out);
		}

		public void SetUserRankBadge(int p_rank, bool p_active = true)
		{
			userRankBadges[0].SetActive(p_rank == 8);
		}

		private void OnDestroy()
		{
			Tween.Kill(rectTransform, "sizeDelta");
		}
	}
}
