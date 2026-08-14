using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINotificationCardView : View<DRLApp>
	{
		public NotificationTypeFlag type;

		public GameObject inactiveOverlay;

		[Space]
		[Header("Invite:")]
		public GameObject inviteContainer;

		public GameObject roomInfoContainer;

		public GameObject tournamentInfoContainer;

		public Text inviteTitle;

		public Text roomName;

		public Text tournamentTitle;

		public Text userName;

		public Image timerBar;

		public RawImage userAvatar;

		public Image userColorBar;

		public GameObject timerBarPlaceholder;

		public GameObject timerSpace;

		public GameObject feedback;

		public GameObject connectingLabel;

		public GameObject expiredLabel;

		public Text errorText;

		public GameObject qmStatesContainer;

		public Text qmStatesText;

		public GameObject leaveQMQueueButton;

		public float inviteMaxHeight = 132f;

		[Header("Invite buttons:")]
		public UIElementView acceptButton;

		public UIElementView declineButton;

		public UIElementView registerButton;

		public UIElementView joinButton;

		public UIElementView dismissButton;

		public List<Button> confirmButtons;

		public List<Button> declineButtons;

		public List<GameObject> xboxIcons;

		public List<GameObject> psIcons;

		public FadeComponent acceptFade;

		public FadeComponent declineFade;

		public FadeComponent registerFade;

		public FadeComponent joinFade;

		public FadeComponent dismissFade;

		[Space]
		[Header("Info:")]
		public GameObject infoContainer;

		public Text infoTitle;

		public RawImage infoImage;

		public Text infoMessage;

		public UIElementView detailsButton;

		public UIElementView closeButton;

		public float infoMaxHeight = 232f;

		public Image psAcceptIcon;

		public Image psDeclineIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		private bool m_connected;

		private Dictionary<string, Texture> cache = new Dictionary<string, Texture>();

		private Activity m_inviteExpTimer;

		public bool inactive { get; set; }

		public LayoutElement layout => AssertLocal<LayoutElement>("layout");

		public RectTransform rect => AssertLocal<RectTransform>("rect");

		public bool connecting { get; set; }

		public string id
		{
			get
			{
				if (data != null)
				{
					return data.id;
				}
				return "";
			}
		}

		public NotificationData data { get; set; }

		public DRLTournamentData tournamentData { get; set; }

		public UINavigation navigation => AssertLocal<UINavigation>("navigation");

		public Button activeButton { get; set; }

		public Button rejectButton { get; set; }

		public bool selected
		{
			get
			{
				if (UINavigation.focus == null)
				{
					return false;
				}
				if (navigation == UINavigation.focus)
				{
					return true;
				}
				return UINavigation.focus.transform.IsChild(base.transform);
			}
		}

		public void Set(NotificationData p_data, bool p_popUp = false, float p_lifeSpan = 10f)
		{
			inviteContainer.SetActive(p_data.type != NotificationTypeFlag.Information);
			infoContainer.SetActive(p_data.type == NotificationTypeFlag.Information);
			type = p_data.type;
			data = p_data;
			ResetUI();
			inactive = false;
			AnimateTimer(p_lifeSpan, p_popUp);
			SetupGamepadIcons();
			switch (type)
			{
			case NotificationTypeFlag.RoomInvite:
			case NotificationTypeFlag.QuickMatchInvite:
				if (data is InviteNotificationData inviteNotificationData)
				{
					Localization locale = base.app.model.storage.locale;
					inviteTitle.text = (inviteNotificationData.inviteIsRace ? locale.Get<string>("notifications.ui.join.race", "JOIN MY RACE") : locale.Get<string>("notifications.ui.join.freestyle", "JOIN MY FREESTYLE"));
					PlatformService platform = base.app.model.service.platform;
					platform.TextValidate(inviteNotificationData.inviteRoomName, delegate(bool p_result, string p_input)
					{
						roomName.text = (p_result ? p_input : "ROOM");
					});
					platform.TextValidate(inviteNotificationData.profileName, delegate(bool p_result, string p_input)
					{
						userName.text = (p_result ? p_input : "PLAYER");
					});
					LoadPhoto(inviteNotificationData.playerId);
				}
				break;
			case NotificationTypeFlag.Tournament:
				if (data is TournamentNotificationData tournamentNotificationData)
				{
					LoadPhoto(tournamentNotificationData.tournamentThumbnailURL);
					tournamentTitle.text = tournamentNotificationData.tournamentTitle;
					userName.text = "DRL SIM";
					roomInfoContainer.SetActive(value: false);
					tournamentInfoContainer.SetActive(value: true);
					acceptButton.gameObject.SetActive(value: false);
					declineButton.gameObject.SetActive(value: false);
					dismissButton.gameObject.SetActive(value: true);
					rejectButton = dismissButton.GetComponent<Button>();
					switch (tournamentNotificationData.status)
					{
					case TournamentNotificationType.Opened:
						inviteTitle.text = "REGISTER NOW!";
						registerButton.gameObject.SetActive(value: true);
						activeButton = registerButton.GetComponent<Button>();
						break;
					case TournamentNotificationType.SoonToStart:
						inviteTitle.text = "TOURNAMENT STARTING SOON!";
						rejectButton = dismissButton.GetComponent<Button>();
						activeButton = null;
						break;
					case TournamentNotificationType.Started:
						inviteTitle.text = "TOURNAMENT STARTED!";
						joinButton.gameObject.SetActive(value: true);
						activeButton = joinButton.GetComponent<Button>();
						break;
					}
				}
				break;
			case NotificationTypeFlag.Message:
				SetFeedback(p_data.message, p_data.error);
				break;
			case NotificationTypeFlag.Information:
				break;
			}
		}

		private void SetupGamepadIcons()
		{
			if (RCI.IsRCController() || RCI.GetActiveJoystick() == null)
			{
				foreach (GameObject xboxIcon in xboxIcons)
				{
					xboxIcon.SetActive(value: false);
				}
				{
					foreach (GameObject psIcon in psIcons)
					{
						psIcon.SetActive(value: false);
					}
					return;
				}
			}
			bool flag = RCI.GetDefaultControllerType(DefaultControllerType.XBox) == DefaultControllerType.PS;
			foreach (GameObject xboxIcon2 in xboxIcons)
			{
				xboxIcon2.SetActive(!flag);
			}
			foreach (GameObject psIcon2 in psIcons)
			{
				psIcon2.SetActive(flag);
			}
		}

		private void ResetUI()
		{
			SetAcceptActive(p_enabled: true);
			inactiveOverlay.SetActive(value: false);
			feedback.SetActive(value: false);
			connectingLabel.SetActive(value: true);
			expiredLabel.SetActive(value: false);
			qmStatesText.text = "CONNECTING...";
			qmStatesContainer.SetActive(value: false);
			leaveQMQueueButton.SetActive(value: false);
			acceptButton.gameObject.SetActive(value: true);
			activeButton = acceptButton.GetComponent<Button>();
			declineButton.gameObject.SetActive(value: true);
			rejectButton = declineButton.GetComponent<Button>();
			joinButton.gameObject.SetActive(value: false);
			dismissButton.gameObject.SetActive(value: false);
			registerButton.gameObject.SetActive(value: false);
			tournamentInfoContainer.SetActive(value: false);
			roomInfoContainer.SetActive(value: true);
			joinButton.interactable = true;
			registerButton.interactable = true;
			acceptButton.interactable = true;
		}

		public void LoadPhoto(string p_playerId)
		{
			if (cache.ContainsKey(p_playerId))
			{
				userAvatar.texture = cache[p_playerId];
				return;
			}
			userAvatar.texture = base.app.model.notifications.defaultAvatar;
			if (string.IsNullOrEmpty(p_playerId))
			{
				return;
			}
			base.app.model.service.GetPlayerAvatar(p_playerId, delegate(Texture2D p_result)
			{
				if ((bool)p_result && base.validContext)
				{
					userAvatar.texture = p_result;
					if (!cache.ContainsKey(p_playerId))
					{
						cache.Add(p_playerId, p_result);
					}
				}
			});
		}

		private Texture2D Resize(Texture2D source, int newWidth, int newHeight)
		{
			source.filterMode = FilterMode.Point;
			RenderTexture temporary = RenderTexture.GetTemporary(newWidth, newHeight);
			temporary.filterMode = FilterMode.Point;
			RenderTexture.active = temporary;
			Graphics.Blit(source, temporary);
			Texture2D texture2D = new Texture2D(newWidth, newHeight);
			texture2D.ReadPixels(new Rect(0f, 0f, newWidth, newHeight), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}

		public void AnimateTimer(float p_duration, bool p_popUp = true)
		{
			float num = (float)(DateTime.Now - data.timestamp).TotalSeconds;
			if (!p_popUp)
			{
				if (num >= p_duration)
				{
					inactiveOverlay.SetActive(value: true);
					inactive = true;
					timerBar.fillAmount = 0f;
					return;
				}
				timerBar.fillAmount = 1f - num / p_duration;
			}
			Tween.Kill(timerBar, "fillAmount");
			Tween tween = Tween.Add(timerBar, "fillAmount", 0f, p_popUp ? p_duration : (p_duration - num), Tween.Linear);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				if (!connecting)
				{
					if (p_popUp)
					{
						if (base.validContext)
						{
							Notify("notifications.ui.timeout", id);
						}
						Hide(0.3f);
					}
					else
					{
						inactiveOverlay.SetActive(value: true);
						inactive = true;
					}
				}
			});
		}

		public void Show(float p_duration)
		{
			float p_to = ((type == NotificationTypeFlag.Information) ? infoMaxHeight : inviteMaxHeight);
			layout.preferredHeight = 0f;
			Tween.Kill(layout, "preferredHeight");
			Tween tween = Tween.Add(layout, "preferredHeight", p_to, p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				if (!(rect == null))
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
				}
			});
			if (UINavigation.focus != navigation)
			{
				SetButtonsVisible(p_flag: false);
			}
		}

		public void Hide(float p_duration)
		{
			Tween.Kill(layout, "preferredHeight");
			Tween tween = Tween.Add(layout, "preferredHeight", 0f, p_duration, Cubic.Out);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				if (!(rect == null))
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
				}
			});
		}

		public void SetAcceptActive(bool p_enabled)
		{
			acceptButton.interactable = p_enabled;
		}

		public void StartConnecting()
		{
			connecting = true;
			m_connected = false;
			feedback.SetActive(value: true);
			connectingLabel.SetActive(value: true);
			InviteNotificationData inviteNotificationData = (InviteNotificationData)data;
			Notify("notifications.action", inviteNotificationData);
			if (inviteNotificationData.type != NotificationTypeFlag.QuickMatchInvite)
			{
				m_inviteExpTimer = Activity.RunOnce(delegate
				{
					Debug.Log("UINotificationCardView> StartConnecting / Expired!");
					SetErrorFeedback(NotificationCardErrorType.Expired);
				}, 20f);
			}
		}

		public void StopConnecting()
		{
			if (connecting)
			{
				connecting = false;
				m_connected = false;
				ResetUI();
				if (m_inviteExpTimer != null)
				{
					m_inviteExpTimer.Stop();
				}
				if (base.app.model.network != null)
				{
					base.app.model.network.LeaveRoom();
				}
			}
		}

		public void OnConnected(string p_roomID = "")
		{
			if (connecting)
			{
				InviteNotificationData inviteNotificationData = (InviteNotificationData)data;
				if (inviteNotificationData != null && base.app.model.network != null && p_roomID == inviteNotificationData.inviteRoomId)
				{
					connecting = false;
					m_connected = true;
					connectingLabel.SetActive(value: false);
					feedback.SetActive(value: false);
					Notify("notifications.ui.connected", inviteNotificationData.id);
				}
			}
		}

		public void SetErrorFeedback(NotificationCardErrorType p_error, float p_duration = 3f)
		{
			if (!m_connected)
			{
				expiredLabel.SetActive(value: true);
				connectingLabel.SetActive(value: false);
				qmStatesContainer.SetActive(value: false);
				switch (p_error)
				{
				case NotificationCardErrorType.Expired:
					errorText.text = "INVITE EXPIRED";
					break;
				case NotificationCardErrorType.LobbyJoinFailed:
					errorText.text = "LOBBY CAN'T BE REACHED";
					break;
				case NotificationCardErrorType.RoomNotActive:
					errorText.text = "ROOM NO LONGER ACTIVE";
					break;
				case NotificationCardErrorType.RoomFull:
					errorText.text = "ROOM FULL";
					break;
				default:
					errorText.text = "LOBBY CAN'T BE REACHED";
					break;
				}
				this.TimerRunOnce(delegate
				{
					connecting = false;
					m_connected = false;
					Hide(0.3f);
					Notify("notifications.ui.expired", id);
				}, 2f);
			}
		}

		public void SetFeedback(string p_message, bool is_error = false, float p_duration = 3f)
		{
			feedback.SetActive(value: true);
			expiredLabel.SetActive(is_error);
			connectingLabel.SetActive(value: false);
			qmStatesContainer.SetActive(!is_error);
			if (is_error)
			{
				errorText.text = p_message;
			}
			else
			{
				qmStatesText.text = p_message;
			}
			this.TimerRunOnce(delegate
			{
				connecting = false;
				m_connected = false;
				Hide(0.3f);
				Notify("notifications.ui.expired", id);
			}, p_duration);
		}

		public void SetFeedback(string p_feedback)
		{
			if (string.IsNullOrEmpty(p_feedback))
			{
				feedback.SetActive(value: false);
				qmStatesContainer.SetActive(value: false);
				connectingLabel.SetActive(value: false);
				expiredLabel.SetActive(value: false);
				qmStatesText.text = "";
			}
			else
			{
				connectingLabel.SetActive(value: false);
				expiredLabel.SetActive(value: false);
				feedback.SetActive(value: true);
				qmStatesContainer.SetActive(value: true);
				qmStatesText.text = p_feedback;
			}
		}

		public void SetFeedbackConnecting()
		{
			feedback.SetActive(value: true);
			connectingLabel.SetActive(value: true);
			expiredLabel.SetActive(value: false);
			qmStatesContainer.SetActive(value: false);
		}

		public void OnQMStateChange(QuickMatchState p_state, NetworkRoom p_room)
		{
			if (!connecting)
			{
				return;
			}
			feedback.SetActive(value: true);
			connectingLabel.SetActive(value: false);
			expiredLabel.SetActive(value: false);
			qmStatesContainer.SetActive(value: true);
			switch (p_state)
			{
			case QuickMatchState.ConnectedBestServer:
				qmStatesText.text = "CONNECTED TO SERVER";
				break;
			case QuickMatchState.CreatingRoom:
				qmStatesText.text = "CREATING ROOM";
				break;
			case QuickMatchState.Failed:
				connecting = false;
				m_connected = false;
				SetErrorFeedback(NotificationCardErrorType.LobbyJoinFailed);
				break;
			case QuickMatchState.MatchmakingChanged:
				if (p_room != null)
				{
					int num = p_room.LobbyCountdown;
					if (num < 0)
					{
						num = 0;
					}
					leaveQMQueueButton.SetActive(p_room.RacersCount == 1);
					if (p_room.RacersCount > 1)
					{
						qmStatesText.text = "RACE STARTS IN " + num + "\nQUEUED: " + p_room.RacersCount + "/" + p_room.MaxRacers;
					}
					else
					{
						qmStatesText.text = "WAITING FOR PLAYERS!";
					}
					if (p_room.State == NetworkRoom.StateCode.MatchLocked)
					{
						qmStatesText.text = "CONNECTED!";
						Notify("notifications.ui.connected", data.id);
					}
				}
				break;
			case QuickMatchState.JoinedRoom:
				qmStatesText.text = "JOINED ROOM";
				break;
			}
		}

		public void SetButtonsVisible(bool p_flag)
		{
			if (!inactive)
			{
				acceptFade.Fade(p_flag ? 1f : 0.15f, 0f);
				declineFade.Fade(p_flag ? 1f : 0.15f, 0f);
				registerFade.Fade(p_flag ? 1f : 0.15f, 0f);
				joinFade.Fade(p_flag ? 1f : 0.15f, 0f);
				dismissFade.Fade(p_flag ? 1f : 0.15f, 0f);
			}
		}

		private void OnDisable()
		{
			if (m_inviteExpTimer != null)
			{
				m_inviteExpTimer.Stop();
				m_inviteExpTimer.manager.Remove(m_inviteExpTimer);
				m_inviteExpTimer = null;
			}
		}
	}
}
