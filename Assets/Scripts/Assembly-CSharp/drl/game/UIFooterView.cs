using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIFooterView : View<DRLApp>, ILocaleElement, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[Serializable]
		public class UINetworkWidgetView
		{
			public FadeComponent lanStatusFade;

			public Text lanStatusField;

			public void SetLanStatus(string p_message = "")
			{
				if (!string.IsNullOrEmpty(p_message))
				{
					lanStatusFade.FadeIn(1f);
					lanStatusField.text = p_message;
				}
				else
				{
					lanStatusFade.FadeOut(1f);
				}
			}
		}

		[Header("Player")]
		[SerializeField]
		private Image m_userColorField;

		[SerializeField]
		private RawImage m_userPhotoField;

		[SerializeField]
		private Image m_userBadgeRank;

		[SerializeField]
		private Text m_usernameField;

		[SerializeField]
		private Text m_timeField;

		[SerializeField]
		private Image m_droneColorField;

		[SerializeField]
		private RawImage m_droneImageField;

		[SerializeField]
		private Text m_droneNameField;

		[SerializeField]
		private UITruncateText m_drone_name_trunc;

		[SerializeField]
		private UIElementView m_droneButton;

		[SerializeField]
		private List<Sprite> m_badgeSprites;

		[SerializeField]
		private HorizontalLayoutGroup _fields;

		public Image controllerBackgroundField;

		public FadeComponent controllerFieldFade;

		[SerializeField]
		private Text m_controllerField;

		[SerializeField]
		private UITruncateText m_controller_field_trunc;

		[SerializeField]
		private Image m_controllerIconField;

		[SerializeField]
		private Sprite playstationControllerIcon;

		public ParticleSystem propsParticles;

		[SerializeField]
		private Image m_mainBackground;

		[SerializeField]
		private Image m_chatBackground;

		[Space]
		[Header("Tryouts disabled")]
		public GameObject profileFocus;

		public GameObject droneSelectionFocus;

		public GameObject socialMedia;

		public GameObject lastSeparator;

		[Space]
		[Header("Social Button")]
		public UIElementView socialButtonView;

		public Text socialButtonUserCountField;

		public UINavigation socialButtonNavigation;

		public CanvasGroup socialButtonGlobalNotificationCanvasGroup;

		public CanvasGroup socialButtonPrivateNotificationCanvasGroup;

		public Text socialButtonPrivateNotificationText;

		public GameObject socialTwitchLiveOutline;

		public GameObject socialSeparator;

		public LayoutElement socialGroupLayout;

		public GameObject socialToggle;

		public Transform socialToggleStates;

		[Space]
		[Header("Quit Button")]
		public GameObject quitButton;

		[Space]
		[Header("Notifications")]
		public UIElementView notificationsButton;

		public Image notificationStripe;

		[Space]
		[Header("Network")]
		public UINetworkWidgetView network;

		public UINavigation lobbyButton;

		private static bool lobbyShowing;

		public Image connectionStatusIcon;

		public Text connectionStatusField;

		public Color connectionActiveColor;

		public Color connectionInactiveColor;

		public GameObject connectionStatusButton;

		public GameObject connectionStatusSeparator;

		[Space]
		[Header("Misc")]
		[SerializeField]
		private List<Image> m_separators;

		public Color menuColorDark = Color.black;

		public Color menuColorLight = Color.white;

		public Color inGameColorDark = Color.black;

		public Color inGameColorLight = Color.white;

		public Color garageColorDark = Color.black;

		public Color garageColorLight = Color.white;

		public static List<UINavigation> buttonNavs = new List<UINavigation>(10);

		public List<UINavigation> buttonNavigations;

		public static List<UINavigation> lobbyNavs = new List<UINavigation>();

		public List<UINavigation> lobbyButtonNavigations;

		[Space]
		[Header("Progression")]
		public ImageClip progressionRankField;

		public Text progressionLevelField;

		public Text progressionXPField;

		public RectTransform progressionXPProgressBar;

		public RectTransform progressionXPIcon;

		public RectTransform progressionStreakContainer;

		public Image progressionStreakFireIconField;

		public Rotator progressionStreakPropRotator;

		public Text progressionStreakField;

		public Text progressionStreakProgressField;

		public RectTransform progressionStreakProgressBar;

		public ImageClip progressionLevelRangeField;

		private static bool m_isVisible = true;

		private DateTime m_dt;

		private DateTimeFormatInfo m_dti;

		private int m_dmonth = -1;

		private int m_dday = -1;

		private int m_dhour = -1;

		private int m_dminute = -1;

		private int m_dsecond = -1;

		private DayOfWeek m_ddweek = (DayOfWeek)(-1);

		private string m_ddweek_s;

		private string m_dmonth_s;

		private string m_dday_s;

		private string m_dhour_s;

		private string m_dminute_s;

		private string m_dsecond_s;

		private CultureInfo m_ci = new CultureInfo("en-US");

		private bool m_is_social_group_expanded;

		private int m_league;

		private int m_xp_current;

		private int m_xp_prev;

		private int m_xp_next;

		private int m_streak;

		private int m_streak_current;

		private int m_streak_goal;

		private Activity m_usercount_timer;

		public Image userColorField
		{
			get
			{
				if (!m_userColorField)
				{
					return m_userColorField = Find<Image>("content.user.color.field");
				}
				return m_userColorField;
			}
		}

		public RawImage userPhotoField
		{
			get
			{
				if (!m_userPhotoField)
				{
					return m_userPhotoField = Find<RawImage>("content.user.photo");
				}
				return m_userPhotoField;
			}
		}

		public Image userBadgeRank
		{
			get
			{
				if (!m_userBadgeRank)
				{
					return m_userBadgeRank = Find<Image>("content.user.color.field");
				}
				return m_userBadgeRank;
			}
		}

		public Text usernameField
		{
			get
			{
				if (!m_usernameField)
				{
					return m_usernameField = Find<Text>("content.user.field");
				}
				return m_usernameField;
			}
		}

		public Text timeField
		{
			get
			{
				if (!m_timeField)
				{
					return m_timeField = Find<Text>("content.time.field");
				}
				return m_timeField;
			}
		}

		public Image droneColorField
		{
			get
			{
				if (!m_droneColorField)
				{
					return m_droneColorField = Find<Image>("content.user.color.field");
				}
				return m_droneColorField;
			}
		}

		public RawImage droneImageField
		{
			get
			{
				if (!m_droneImageField)
				{
					return m_droneImageField = Find<RawImage>("content.user.photo");
				}
				return m_droneImageField;
			}
		}

		public Text droneNameField
		{
			get
			{
				if (!m_droneNameField)
				{
					return m_droneNameField = Find<Text>("content.user.field");
				}
				return m_droneNameField;
			}
		}

		public UITruncateText droneNameFieldTrunc
		{
			get
			{
				if (!m_drone_name_trunc)
				{
					return m_drone_name_trunc = droneNameField.GetComponent<UITruncateText>();
				}
				return m_drone_name_trunc;
			}
		}

		public UIElementView droneButton
		{
			get
			{
				if (!m_droneButton)
				{
					return m_droneButton = Find<UIElementView>("content.drone");
				}
				return m_droneButton;
			}
		}

		public Text controllerField
		{
			get
			{
				if (!m_controllerField)
				{
					return m_controllerField = Find<Text>("content.controller.field");
				}
				return m_controllerField;
			}
		}

		public UITruncateText controllerFieldTrunc
		{
			get
			{
				if (!m_controller_field_trunc)
				{
					return m_controller_field_trunc = Find<UITruncateText>("content.controller.field");
				}
				return m_controller_field_trunc;
			}
		}

		public Image controllerIconField
		{
			get
			{
				if (!m_controllerIconField)
				{
					return m_controllerIconField = Find<Image>("content.controller.icon.controller");
				}
				return m_controllerIconField;
			}
		}

		public string username
		{
			get
			{
				return usernameField.text;
			}
			set
			{
				usernameField.text = value.ToUpper();
			}
		}

		public Texture userPhoto
		{
			get
			{
				return userPhotoField.texture;
			}
			set
			{
				userPhotoField.texture = value;
				userPhotoField.enabled = value;
				userColorField.transform.parent.gameObject.SetActive(value);
			}
		}

		public Color userColor
		{
			get
			{
				return userColorField.color;
			}
			set
			{
				userColorField.color = value;
			}
		}

		public string droneName
		{
			get
			{
				return droneNameField.text;
			}
			set
			{
				droneNameField.text = value;
				if ((bool)droneNameFieldTrunc)
				{
					droneNameFieldTrunc.Refresh();
				}
			}
		}

		public Texture droneImage
		{
			get
			{
				return droneImageField.texture;
			}
			set
			{
				droneImageField.texture = value;
				droneImageField.GetComponent<FadeComponent>()?.FadeIn();
			}
		}

		public Color droneColor
		{
			get
			{
				return droneColorField.color;
			}
			set
			{
				droneColorField.color = value;
			}
		}

		public string controllerStatus
		{
			get
			{
				return controllerField.text;
			}
			set
			{
				controllerField.text = value;
				if ((bool)controllerFieldTrunc)
				{
					controllerFieldTrunc.Refresh();
				}
			}
		}

		public Color controllerColor
		{
			get
			{
				return controllerIconField.color;
			}
			set
			{
				controllerIconField.color = value;
			}
		}

		public Image mainBackground
		{
			get
			{
				if (!m_mainBackground)
				{
					return m_mainBackground = Find<Image>("background");
				}
				return m_mainBackground;
			}
		}

		public Image chatBackground
		{
			get
			{
				if (!m_chatBackground)
				{
					return m_chatBackground = Find<Image>("content.chat.background");
				}
				return m_chatBackground;
			}
		}

		public List<Image> separators
		{
			get
			{
				if (m_separators != null)
				{
					return m_separators = new List<Image>();
				}
				return m_separators;
			}
		}

		public static bool isVisible
		{
			get
			{
				return m_isVisible;
			}
			set
			{
				m_isVisible = value;
			}
		}

		public DateTime date
		{
			set
			{
				m_dt = value;
				if ((bool)timeField && timeField.gameObject.activeInHierarchy)
				{
					RefreshDateTime();
				}
			}
		}

		private void RefreshDateTime()
		{
			DateTime dt = m_dt;
			CultureInfo ci = m_ci;
			if (m_dti == null)
			{
				m_dti = ci.DateTimeFormat;
			}
			if (m_ddweek != dt.DayOfWeek)
			{
				m_ddweek_s = m_dti.GetDayName(dt.DayOfWeek).Substring(0, 3).ToUpper();
				m_ddweek = dt.DayOfWeek;
			}
			if (m_dmonth != dt.Month)
			{
				m_dmonth_s = dt.Month.ToString("00");
				m_dmonth = dt.Month;
			}
			if (m_dday != dt.Day)
			{
				m_dday_s = dt.Day.ToString("00");
				m_dday = dt.Day;
			}
			if (m_dhour != dt.Hour)
			{
				m_dhour_s = dt.Hour.ToString("00");
				m_dhour = dt.Hour;
			}
			if (m_dminute != dt.Minute)
			{
				m_dminute_s = dt.Minute.ToString("00");
				m_dminute = dt.Minute;
			}
			if (m_dsecond != dt.Second)
			{
				m_dsecond_s = dt.Second.ToString("00");
				m_dsecond = dt.Second;
			}
			timeField.text = m_ddweek_s + ", " + m_dmonth_s + "/" + m_dday_s + " " + m_dhour_s + ":" + m_dminute_s + ":" + m_dsecond_s;
		}

		protected void Awake()
		{
			username = "";
			userPhoto = null;
			userColor = Color.gray;
			userBadgeRank.gameObject.SetActive(value: false);
			controllerStatus = "";
		}

		protected void Start()
		{
			Localization.Add(this);
			progressionRankField.frames = base.app.model.storage.state.player.progression.GetLeagueThumbSmallSprites().ToArray();
			progressionLevelRangeField.frames = base.app.model.storage.state.player.progression.GetLevelRangeThumbSprites().ToArray();
			RefreshTwitchStatus();
			RefreshSocialUserCount();
			buttonNavs = new List<UINavigation>(buttonNavigations);
			lobbyNavs = new List<UINavigation>(lobbyButtonNavigations);
		}

		public void SetProgressionEnabled(bool p_flag)
		{
			progressionStreakContainer.gameObject.SetActive(value: false);
			progressionLevelField.gameObject.SetActive(p_flag);
			progressionRankField.gameObject.SetActive(p_flag);
			progressionXPProgressBar.gameObject.SetActive(p_flag);
			progressionXPIcon.gameObject.SetActive(p_flag);
			progressionXPField.gameObject.SetActive(p_flag);
			RectTransform rectTransform = usernameField.rectTransform;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, p_flag ? 52f : 45f);
			LayoutElement component = ((RectTransform)usernameField.rectTransform.parent).GetComponent<LayoutElement>();
			if ((bool)component)
			{
				component.minWidth = (p_flag ? 180f : 0f);
			}
		}

		public void SetSocialExpanded(bool p_flag, float p_duration = 0f)
		{
			m_is_social_group_expanded = p_flag;
			Tween.Add(socialGroupLayout, "minWidth", p_flag ? 360f : 0f, p_duration, Cubic.Out);
			socialToggleStates.GetChild(0).gameObject.SetActive(!p_flag);
			socialToggleStates.GetChild(1).gameObject.SetActive(p_flag);
			List<UINavigation> list = Hierarchy.FindAll<UINavigation>(socialGroupLayout.transform.GetChild(0));
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = list[i].gameObject.activeInHierarchy && p_flag;
				list[i].enabled = flag;
			}
		}

		public void ToggleSocialGroup(float p_duration = 0f)
		{
			SetSocialExpanded(!m_is_social_group_expanded, p_duration);
		}

		public void SetConnectionStatusActive(bool p_flag)
		{
			connectionStatusIcon.color = (p_flag ? connectionActiveColor : connectionInactiveColor);
			connectionStatusField.color = (p_flag ? connectionActiveColor : connectionInactiveColor);
			string text = base.app.model.storage.locale.Get(p_flag ? "ui.connection.online" : "ui.connection.offline", p_flag ? "ONLINE" : "OFFLINE");
			connectionStatusField.text = text;
			socialButtonView.gameObject.SetActive(p_flag);
			socialSeparator.SetActive(p_flag);
		}

		public void SetProgression(int p_league, int p_level, int p_xp_current, int p_xp_prev, int p_xp_next, int p_streak, int p_streak_current, int p_streak_goal)
		{
			float num = Mathf.Max(0f, p_xp_next - p_xp_prev);
			float x = Mathf.Clamp01((num <= 0f) ? 0f : ((float)(p_xp_current - p_xp_prev) / num));
			float num2 = ((p_streak_goal <= 0) ? 0f : Mathf.Clamp01((float)p_streak_current / (float)p_streak_goal));
			progressionRankField.frame = ((p_league >= 0) ? p_league : 0);
			bool active = progressionXPProgressBar.gameObject.activeInHierarchy && p_league >= 0;
			progressionRankField.gameObject.SetActive(active);
			progressionLevelField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("ui.footer.level", "LEVEL"), p_level);
			progressionXPField.text = string.Format("{0} {1}/{2}", base.app.model.storage.locale.Get("ui.footer.xp", "XP"), p_xp_current, p_xp_next);
			progressionStreakField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("ui.footer.streak", "STREAK"), p_streak);
			if ((bool)progressionStreakProgressField)
			{
				progressionStreakProgressField.text = string.Format("{0} {1}/{2}", base.app.model.storage.locale.Get("ui.footer.race", "RACE"), p_streak_current + 1, p_streak_goal);
			}
			progressionXPProgressBar.localScale = new Vector3(x, 1f, 1f);
			progressionStreakProgressBar.localScale = new Vector3(num2, 1f, 1f);
			progressionLevelRangeField.frame = base.app.model.storage.state.player.progression.GetLevelRangeIndexByLevel(p_level);
			if ((bool)progressionStreakFireIconField)
			{
				Color color;
				Color a = (color = progressionStreakFireIconField.color);
				a.a = 0.06f;
				color.a = 0.25f;
				progressionStreakFireIconField.color = Color.Lerp(a, color, Mathf.Pow(num2, 4f));
			}
			if ((bool)progressionStreakPropRotator)
			{
				progressionStreakPropRotator.speed = Vector3.Lerp(Vector3.forward * -45f, Vector3.forward * -900f, Mathf.Pow(num2, 4f));
			}
			m_league = p_league;
			m_xp_current = p_xp_current;
			m_xp_prev = p_xp_prev;
			m_xp_next = p_xp_next;
			m_streak = p_streak;
			m_streak_current = p_streak_current;
			m_streak_goal = p_streak_goal;
		}

		public void SetProgression(DRLProgressionStateData p_state)
		{
			int leagueIndexByGUID = base.app.model.storage.state.player.progression.GetLeagueIndexByGUID(p_state.league.guid);
			int level = p_state.level;
			int previousLevelXP = p_state.previousLevelXP;
			int xp = p_state.xp;
			int nextLevelXP = p_state.nextLevelXP;
			int streak = p_state.streak;
			int streakMapIndex = p_state.streakMapIndex;
			int streakMapCount = p_state.streakMapCount;
			SetProgression(leagueIndexByGUID, level, xp, previousLevelXP, nextLevelXP, streak, streakMapIndex, streakMapCount);
		}

		public void SetCalibrationWarning(bool p_needs_calibration)
		{
			Localization locale = base.app.model.storage.locale;
			if (p_needs_calibration)
			{
				controllerFieldFade.pulse = true;
				controllerFieldFade.alpha = 1f;
				controllerField.color = Color.red;
				controllerStatus = locale.Get("calibration.footer.needscalibration", "PLEASE CALIBRATE");
			}
			else
			{
				controllerFieldFade.pulse = false;
				controllerFieldFade.alpha = 1f;
				controllerField.color = Color.white;
				controllerStatus = "";
			}
		}

		public void SetColors(bool p_ingame, bool p_ingarage = false)
		{
			Color color;
			Color color2;
			if (p_ingame)
			{
				color = inGameColorDark;
				color2 = inGameColorLight;
			}
			else
			{
				color = menuColorDark;
				color2 = menuColorLight;
			}
			if (p_ingarage)
			{
				color = garageColorDark;
				color2 = garageColorLight;
			}
			m_mainBackground.color = color;
			foreach (Image separator in m_separators)
			{
				separator.color = color2;
			}
		}

		public void Fade(float p_transition, float p_duration = 0.4f, float p_delay = 0f)
		{
			RectTransform component = GetComponent<RectTransform>();
			Vector2 anchoredPosition = component.anchoredPosition;
			anchoredPosition.y = Mathf.Lerp(-80f, 0f, p_transition);
			Tween.Kill(component);
			if (p_duration <= 0f)
			{
				component.anchoredPosition = anchoredPosition;
			}
			else
			{
				Tween.Add(component, "anchoredPosition", anchoredPosition, p_duration, p_delay, Cubic.Out);
			}
			HorizontalLayoutGroup component2 = base.transform.Find("content").GetComponent<HorizontalLayoutGroup>();
			if (component2 != null)
			{
				component2.enabled = p_transition > 0f;
			}
			RefreshTwitchStatus();
			RefreshSocialUserCount();
			SetProgressionEnabled(base.app.online);
		}

		public void Show(float p_duration = 0.4f, float p_delay = 0f)
		{
			base.gameObject.SetActive(value: true);
			object[] p_data = new string[2]
			{
				p_duration.ToString(),
				p_delay.ToString()
			};
			Notify("ui.footer@open", p_data);
			isVisible = true;
			Fade(1f, p_duration, p_delay);
			RefreshNavigationButtons();
		}

		public void Hide(float p_duration = 0.4f, float p_delay = 0f)
		{
			object[] p_data = new string[2]
			{
				p_duration.ToString(),
				p_delay.ToString()
			};
			Notify("ui.footer@close", p_data);
			isVisible = false;
			Fade(0f, p_duration, p_delay);
			base.gameObject.SetActive(value: false);
		}

		public void Kill()
		{
			Tween.Kill(GetComponent<RectTransform>());
		}

		public void OnLocaleRefresh()
		{
			string text = "en-US";
			switch (Localization.instance.language)
			{
			case "en-us":
				text = "en-US";
				break;
			case "pt-br":
				text = "pt-BR";
				break;
			case "zh":
				text = "zh-Hans";
				break;
			}
			m_ci = new CultureInfo(text);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (base.isActiveAndEnabled)
			{
				Notify("footer@over");
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (base.isActiveAndEnabled)
			{
				Notify("footer@out");
			}
		}

		protected void RefreshTwitchStatus()
		{
		}

		protected void RefreshSocialUserCount()
		{
			if (base.validContext)
			{
				int num = (base.app.model.chat ? base.app.model.chat.OnlinePlayers : 0);
				if ((bool)socialButtonUserCountField)
				{
					socialButtonUserCountField.text = ((num <= 0) ? "1" : num.ToString());
				}
				if (m_usercount_timer != null)
				{
					m_usercount_timer.Stop();
				}
				m_usercount_timer = Activity.RunOnce(RefreshSocialUserCount, 10f);
			}
		}

		public void SetRankBadge(int rank)
		{
			if ((bool)userBadgeRank)
			{
				if (rank > 0 && rank <= m_badgeSprites.Count)
				{
					userBadgeRank.sprite = m_badgeSprites[rank - 1];
					userBadgeRank.gameObject.SetActive(userBadgeRank.sprite != null);
				}
				else
				{
					userBadgeRank.gameObject.SetActive(value: false);
				}
				if (rank != 8)
				{
					userBadgeRank.gameObject.SetActive(value: false);
				}
			}
		}

		public static void SetNavigationTop(Component p_target)
		{
			foreach (UINavigation buttonNav in buttonNavs)
			{
				buttonNav.up = p_target;
			}
			int num = 7;
			if (buttonNavs.Count >= num + 1)
			{
				Component component = ((lobbyNavs.Count <= 0) ? null : lobbyNavs[0]);
				Component component2 = ((buttonNavs.Count <= 0) ? null : buttonNavs[0].up);
				buttonNavs[num].up = (lobbyShowing ? component : component2);
			}
		}

		public void RefreshNavigationButtons()
		{
		}

		public void SetConnectionButtonActive(bool p_show)
		{
			connectionStatusButton.SetActive(p_show);
			connectionStatusSeparator.SetActive(p_show);
		}

		public void SetupLobbyNavigation(bool p_show)
		{
			lobbyShowing = p_show;
			lobbyButton.up = (p_show ? lobbyNavs[0] : buttonNavs[0].up);
		}

		public static bool IsLobbyShowing()
		{
			return lobbyShowing;
		}
	}
}
