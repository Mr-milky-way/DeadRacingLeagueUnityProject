using System;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerLobbyItemView : UIElementView<DRLApp>
	{
		public RawImage photoField;

		public Image colorField;

		public Text roomNameField;

		public Text mapTrackField;

		public UITruncateText mapTrackTrim;

		public Text gameTypeField;

		public Image iconQuickRoom;

		public Image iconCustomRoom;

		public Text playerCountField;

		public FadeComponent[] playerCountFades;

		public Text spectatorCountField;

		public FadeComponent[] spectatorCountFades;

		public Image[] actionIcons;

		public Text actionField;

		public UINavigation entryNav;

		public UINavigation actionNav;

		public FadeComponent[] timeFades;

		public Text timeField;

		public FadeComponent[] progressFades;

		public RectTransform progressFillRT;

		public GameObject publicJoin;

		public GameObject privateJoin;

		public GameObject privateRoomJoin;

		public GameObject privateJoinButtonContent;

		public GameObject privateJoinPassword;

		public DRLInputFieldView privateJoinPasswordInput;

		public UINavigationLinkList privateJoinNavigationProxy;

		[Header("Password input outline:")]
		public DRLFocusTransition passwordFocusTransition;

		public Image passwordOutline;

		public Image passwordBlink;

		public RawImage passwordPulse;

		private Image m_progressFillField;

		public float progressFillWidth = 260f;

		public new Lobby.NetworkRoomInfo data;

		private float m_progress;

		private AsyncRequest m_photo_loader;

		private Activity m_ingame_loop;

		private string m_photo_url;

		private bool m_ingame;

		private float m_ingame_elapsed;

		public Image progressFillField
		{
			get
			{
				if (!m_progressFillField)
				{
					return m_progressFillField = progressFillRT.gameObject.GetComponent<Image>();
				}
				return m_progressFillField;
			}
		}

		public Color color
		{
			set
			{
				colorField.color = value;
			}
		}

		public Texture photo
		{
			set
			{
				photoField.texture = value;
				photoField.enabled = value != null;
			}
		}

		public string roomName
		{
			set
			{
				roomNameField.text = value;
			}
		}

		public string mapTrack
		{
			set
			{
				mapTrackField.text = value;
				mapTrackTrim.Refresh();
			}
		}

		public string gameType
		{
			set
			{
				gameTypeField.text = value;
			}
		}

		public float roomProgress
		{
			get
			{
				return m_progress;
			}
			set
			{
				m_progress = Mathf.Clamp01(value);
				SetProgress(m_progress);
			}
		}

		public void SetPlayerCount(int p_count, int p_total)
		{
			playerCountFades[0].Fade(0.1f, 0.5f, 0f, Cubic.Out);
			playerCountFades[1].Fade(0.1f, 0.5f, 0f, Cubic.Out);
			playerCountField.text = "--/--";
			if (p_total > 0)
			{
				playerCountFades[0].Fade(1f, 0.5f, 0f, Cubic.Out);
				playerCountFades[1].Fade(1f, 0.5f, 0f, Cubic.Out);
				playerCountField.text = p_count.ToString("00") + " / " + p_total.ToString("00");
			}
		}

		public void SetSpectatorCount(int p_count, int p_total)
		{
			spectatorCountFades[0].Fade(0.1f, 0.5f, 0f, Cubic.Out);
			spectatorCountFades[1].Fade(0.1f, 0.5f, 0f, Cubic.Out);
			spectatorCountField.text = "-- / --";
			if (p_total > 0)
			{
				spectatorCountFades[0].Fade(1f, 0.5f, 0f, Cubic.Out);
				spectatorCountFades[1].Fade(1f, 0.5f, 0f, Cubic.Out);
				spectatorCountField.text = p_count.ToString("00") + " / " + p_total.ToString("00");
			}
		}

		public void SetTime(float p_time)
		{
			timeFades[0].alpha = ((p_time <= 0f) ? 0f : 1f);
			timeFades[1].alpha = ((p_time <= 0f) ? 0f : 1f);
			timeFades[2].alpha = ((p_time <= 0f) ? 0f : 1f);
			timeField.text = Format.SecondsToTime(p_time);
		}

		public void SetProgress(float p_value)
		{
			m_progress = Mathf.Clamp01(p_value);
			bool flag = p_value > 0f;
			float alpha = (flag ? 1f : 0f);
			for (int i = 0; i < 3; i++)
			{
				progressFades[i].gameObject.SetActive(flag);
				progressFades[i].alpha = alpha;
			}
			Vector2 sizeDelta = progressFillRT.sizeDelta;
			sizeDelta.x = m_progress * progressFillWidth;
			progressFillRT.sizeDelta = sizeDelta;
		}

		public void SetAction(string p_label, string p_icon)
		{
			for (int i = 0; i < actionIcons.Length; i++)
			{
				if ((bool)actionIcons[i])
				{
					actionIcons[i].gameObject.SetActive(actionIcons[i].name == p_icon);
				}
			}
			actionField.text = p_label.ToUpper();
		}

		public void SetActionJoin()
		{
			if (!data.IsPrivate)
			{
				HidePasswordInput();
				publicJoin.gameObject.SetActive(value: true);
				privateJoin.gameObject.SetActive(value: false);
				SetAction("Join", "arrow");
				actionNav = publicJoin.GetComponent<UINavigation>();
			}
			else
			{
				SetPrivateActionJoin();
			}
			if (m_ingame_loop != null)
			{
				m_ingame_loop.Stop();
			}
			m_ingame = false;
			m_ingame_elapsed = 0f;
			SetProgress(0f);
		}

		public void SetPrivateActionJoin()
		{
			if (!privateJoinPassword.activeInHierarchy)
			{
				HidePasswordInput();
			}
			publicJoin.gameObject.SetActive(value: false);
			privateJoin.gameObject.SetActive(value: true);
			actionNav = privateJoinNavigationProxy;
		}

		public void SetActionInGame()
		{
			SetAction("In Game", "prop");
			if (m_ingame)
			{
				return;
			}
			m_ingame = true;
			if (m_ingame_loop != null)
			{
				m_ingame_loop.Stop();
			}
			float t = m_ingame_elapsed;
			m_ingame_loop = Activity.Run((Func<bool>)delegate
			{
				if (!base.gameObject)
				{
					return false;
				}
				if (data == null)
				{
					SetProgress(0f);
					return false;
				}
				if (!base.gameObject.activeInHierarchy)
				{
					SetProgress(0f);
					return false;
				}
				float progress = data.Progress;
				float num = (float)data.TimeLimit;
				t += Time.deltaTime;
				float num2 = ((num <= 0f) ? 1f : Mathf.Clamp01(Mathf.Max(0f, t) / num));
				float num3 = 1f - num2;
				float num4 = num2 + num3 * progress;
				if (data.State == NetworkRoom.StateCode.GameFinished)
				{
					num4 = 1f;
				}
				m_ingame_elapsed = t;
				roomProgress = Mathf.Lerp(roomProgress, num4, Time.deltaTime * 2f);
				progressFillField.color = ((num4 < 0.99f) ? Color.white : Colorf.ARGBToColor(4286561835u));
				return true;
			}, 0f, false);
		}

		public void SetActionSpectate()
		{
			if (!data.IsPrivate)
			{
				HidePasswordInput();
				publicJoin.gameObject.SetActive(value: true);
				privateJoin.gameObject.SetActive(value: false);
				SetAction("Spectate", "eye");
			}
			else
			{
				SetPrivateActionJoin();
			}
			if (m_ingame_loop != null)
			{
				m_ingame_loop.Stop();
			}
			m_ingame = false;
			SetProgress(0f);
		}

		public void SetActionFull()
		{
			SetAction("Full", "cross");
			if (m_ingame_loop != null)
			{
				m_ingame_loop.Stop();
			}
			m_ingame = false;
			SetProgress(0f);
		}

		public void UpdateRoomIcon(bool p_isQuick)
		{
			Color yellow = Color.yellow;
			Color white = Color.white;
			white.a = 0f;
			if (!p_isQuick)
			{
				yellow.a = 0f;
			}
			if ((bool)iconCustomRoom)
			{
				iconCustomRoom.color = white;
			}
			if ((bool)iconQuickRoom)
			{
				iconQuickRoom.color = yellow;
			}
		}

		public void Set(Lobby.NetworkRoomInfo p_data)
		{
			data = p_data;
			if (data == null)
			{
				return;
			}
			string p_input = (string.IsNullOrEmpty(data.RoomTitle) ? "" : data.RoomTitle.ToUpper());
			PlatformService platform = base.app.model.service.platform;
			gameType = data.GameMode.ToString().ToUpper();
			if (!platform)
			{
				roomName = p_input;
				UITruncateText component = roomNameField.GetComponent<UITruncateText>();
				if ((bool)component)
				{
					component.Refresh();
				}
			}
			else
			{
				platform.TextValidate(p_input, delegate(bool p_res, string p_v)
				{
					roomName = (p_res ? p_v : (data.GameMode.ToString().ToUpper() + " ROOM"));
					UITruncateText component2 = roomNameField.GetComponent<UITruncateText>();
					if ((bool)component2)
					{
						component2.Refresh();
					}
				});
			}
			color = data.MasterProfileColour;
			UpdateRoomIcon(p_data.IsQuick);
			SetSpectatorCount(data.SpectatorsCount, data.MaxSpectators);
			SetPlayerCount(data.RacersCount, data.MaxRacers);
			Texture texture = null;
			string text = string.Empty;
			string text2 = string.Empty;
			if (data.UsingCustomMap)
			{
				DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(data.MapId);
				if ((bool)dRLMap)
				{
					texture = dRLMap.preview;
					text = dRLMap.label;
					text2 = data.CustomMapName.ToUpper();
				}
			}
			else
			{
				DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(data.TrackId);
				if ((bool)dRLMapTrack)
				{
					texture = dRLMapTrack.map.preview;
					text = dRLMapTrack.map.label;
					text2 = dRLMapTrack.label;
				}
			}
			photo = texture;
			mapTrack = text + " / " + text2;
			if (data.CanRace)
			{
				SetActionJoin();
			}
			else if (data.CanSpectate)
			{
				SetActionSpectate();
			}
			else if (data.InGame)
			{
				SetActionInGame();
			}
			else if (data.IsFull)
			{
				SetActionFull();
			}
		}

		public void ShowPasswordInput()
		{
			if (!privateJoinPassword.activeInHierarchy)
			{
				privateJoinButtonContent.SetActive(value: false);
				privateJoinPassword.SetActive(value: true);
				UINavigation.Focus(privateJoinPasswordInput.GetComponent<UINavigation>());
			}
		}

		public void HidePasswordInput()
		{
			if (privateJoinPassword.activeInHierarchy)
			{
				privateJoinPasswordInput.text = "";
				privateJoinButtonContent.SetActive(value: true);
				privateJoinPassword.SetActive(value: false);
				UINavigation.Focus(actionNav);
			}
		}

		public void TogglePasswordInput()
		{
			if (privateJoinPassword.activeSelf)
			{
				HidePasswordInput();
			}
			else
			{
				ShowPasswordInput();
			}
		}

		public void PulseIncorrectPassword()
		{
			UINavigation.focus = privateJoinPasswordInput.GetComponent<UINavigation>();
			passwordBlink.color = Color.red;
			passwordOutline.color = Color.red;
			passwordPulse.color = Color.red;
			passwordFocusTransition.Blink();
			this.TimerRunOnce(delegate
			{
				passwordBlink.color = Color.white;
				passwordOutline.color = Color.white;
				passwordPulse.color = Color.white;
			}, 0.6f);
		}
	}
}
