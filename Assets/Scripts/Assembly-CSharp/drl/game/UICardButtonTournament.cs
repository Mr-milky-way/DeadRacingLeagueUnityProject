using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICardButtonTournament : UICardView
	{
		private DRLTournamentData m_data;

		private static Dictionary<string, Texture> m_backgroundCache;

		private static Dictionary<string, Texture> m_winnerImageCache;

		[SerializeField]
		private Text m_tournamentNameField;

		[SerializeField]
		private Text m_tournamentDescField;

		public GameObject tournamentDateBackground;

		public GameObject dateStopwatchIcon;

		public Text tournamentDateCaptionField;

		public GameObject tournamentStartedBackground;

		public Text tournamentDateField;

		public Text tournamentWinnerField;

		public Text tournamentPrizeField;

		public UITruncateText tournamentPrizeTrunc;

		public FadeComponent tournamentPrizeFader;

		public FadeComponent unavailableCoverFade;

		public FadeComponent contentFade;

		public UINavigation nav;

		public RawImage winnerPhotoField;

		public Image winnerColorField;

		public GameObject winnerField;

		[SerializeField]
		private FadeComponent m_winnerPhotoFader;

		[SerializeField]
		private RawImage m_imageField;

		[SerializeField]
		private FadeComponent m_imageFader;

		[SerializeField]
		private UITruncateText m_descriptionTrunc;

		private AsyncRequest m_photo_loader;

		public float timer;

		public MonoActivity timerActivity;

		public override UICardType type => UICardType.ButtonTournamentItem;

		public DRLTournamentData tournamentData => m_data;

		internal Dictionary<string, Texture> backgroundCache => Reflection<object>.Assert(ref m_backgroundCache);

		internal static Dictionary<string, Texture> winnerImageCache => Reflection<object>.Assert(ref m_winnerImageCache);

		public Text tournamentNameField
		{
			get
			{
				if (!m_tournamentNameField)
				{
					return m_tournamentNameField = Find<Text>("content.body.title-0");
				}
				return m_tournamentNameField;
			}
		}

		public Text tournamentDescField
		{
			get
			{
				if (!m_tournamentDescField)
				{
					return m_tournamentDescField = Find<Text>("content.body.title-1");
				}
				return m_tournamentDescField;
			}
		}

		public FadeComponent winnerPhotoFader
		{
			get
			{
				if (!m_winnerPhotoFader)
				{
					return m_winnerPhotoFader = winnerPhotoField.GetComponent<FadeComponent>();
				}
				return m_winnerPhotoFader;
			}
		}

		public RawImage imageField
		{
			get
			{
				if (!m_imageField)
				{
					return m_imageField = Find<RawImage>("backgrounds.image");
				}
				return m_imageField;
			}
		}

		public FadeComponent imageFader
		{
			get
			{
				if (!m_imageFader)
				{
					return m_imageFader = imageField.GetComponent<FadeComponent>();
				}
				return m_imageFader;
			}
		}

		public UITruncateText descriptionTrunc
		{
			get
			{
				if (!m_descriptionTrunc)
				{
					return m_descriptionTrunc = tournamentDescField.GetComponent<UITruncateText>();
				}
				return m_descriptionTrunc;
			}
		}

		public string tournamentName
		{
			set
			{
				UIReflection.Set(tournamentNameField, value);
			}
		}

		public string tournamentDescription
		{
			set
			{
				UIReflection.Set(tournamentDescField, value);
			}
		}

		public string tournamentDate
		{
			set
			{
				UIReflection.Set(tournamentDateField, value);
			}
		}

		public string tournamentDateCaption
		{
			set
			{
				UIReflection.Set(tournamentDateCaptionField, value);
			}
		}

		public string tournamentWinner
		{
			set
			{
				UIReflection.Set(tournamentWinnerField, value);
			}
		}

		public Texture image
		{
			set
			{
				UIReflection.Set(imageField, value);
			}
		}

		public Texture winnerImage
		{
			set
			{
				UIReflection.Set(winnerPhotoField, value);
			}
		}

		public void Set(DRLTournamentData p_data, UITournamentsListCardType p_type)
		{
			if (p_data == null)
			{
				tournamentName = string.Empty;
				if ((bool)tournamentDescField)
				{
					tournamentDescription = string.Empty;
				}
				imageField.enabled = false;
				if ((bool)winnerField)
				{
					winnerField.SetActive(value: false);
				}
				tournamentDate = string.Empty;
				tournamentDateCaption = string.Empty;
				if ((bool)unavailableCoverFade)
				{
					unavailableCoverFade.FadeIn(0.01f);
					contentFade.FadeOut(0.01f);
				}
				else
				{
					base.interactable = false;
				}
				if (tournamentDateBackground != null)
				{
					tournamentDateBackground.SetActive(value: false);
				}
				m_data = null;
				return;
			}
			base.interactable = true;
			if ((bool)unavailableCoverFade)
			{
				unavailableCoverFade.FadeOut(0.01f);
				contentFade.FadeIn(0.01f);
				nav.enabled = true;
			}
			if (tournamentDateBackground != null)
			{
				tournamentDateBackground.SetActive(value: true);
			}
			Clear();
			m_data = p_data;
			tournamentName = m_data.title.ToUpper();
			switch (p_type)
			{
			case UITournamentsListCardType.Active:
				SetActive();
				break;
			case UITournamentsListCardType.Registration:
				SetRegistration();
				break;
			case UITournamentsListCardType.Past:
				SetPast();
				break;
			case UITournamentsListCardType.Future:
				SetFuture();
				break;
			}
			if ((bool)tournamentPrizeField)
			{
				if (string.IsNullOrEmpty(p_data.prizeDescription))
				{
					tournamentPrizeField.text = "";
					tournamentPrizeFader.FadeOut(0.1f);
				}
				else
				{
					tournamentPrizeField.text = base.app.model.storage.locale.Get("vdrl.label.winners-prize", "WINNERS PRIZE:") + " " + p_data.prizeDescription.ToUpper();
					tournamentPrizeFader.FadeIn(0.1f);
					if ((bool)tournamentPrizeTrunc)
					{
						tournamentPrizeTrunc.Refresh();
					}
				}
			}
			if (p_type != UITournamentsListCardType.Past)
			{
				LoadBackground();
			}
		}

		public void SetPast()
		{
			DateTime registerEndDate = m_data.registerEndDate;
			string empty = string.Empty;
			winnerImage = null;
			if (m_data.rankings != null && m_data.rankings.Length != 0)
			{
				empty = m_data.rankings[0].profileName;
				if ((bool)winnerField)
				{
					winnerField.SetActive(value: true);
				}
				tournamentWinner = empty.ToUpper();
				if ((bool)winnerColorField)
				{
					winnerColorField.color = m_data.rankings[0].profileColor;
				}
				if ((bool)winnerPhotoField && winnerPhotoFader != null)
				{
					if (string.IsNullOrEmpty(m_data.rankings[0].profileThumbURL))
					{
						winnerPhotoFader.FadeOut(0.1f);
						winnerImage = null;
					}
					else if (winnerImageCache.ContainsKey(m_data.rankings[0].profileThumbURL))
					{
						winnerImage = winnerImageCache[m_data.rankings[0].profileThumbURL];
						winnerPhotoFader.FadeIn(0.1f);
					}
					else
					{
						winnerPhotoFader.Fade(0f, 0.001f);
						m_photo_loader = Web.Load(m_data.rankings[0].profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
						{
							if (!(p_result == null))
							{
								winnerPhotoFader.FadeIn();
								winnerImage = p_result;
								winnerImageCache[m_data.rankings[0].profileThumbURL] = p_result;
							}
						});
					}
				}
			}
			else if ((bool)winnerField)
			{
				winnerField.SetActive(value: false);
			}
			tournamentName = m_data.title.ToUpper();
			tournamentDate = registerEndDate.ToString("MM/dd/yyyy");
		}

		public void SetFuture()
		{
			DateTime registerEndDate = m_data.registerEndDate;
			tournamentName = m_data.title.ToUpper();
			tournamentDescription = m_data.callToAction.ToUpper();
			tournamentDate = registerEndDate.ToString("MM/dd/yyyy");
			tournamentDateCaption = base.app.model.storage.locale.Get("vdrl.label.starts", "STARTS");
			if ((bool)dateStopwatchIcon)
			{
				dateStopwatchIcon.SetActive(value: false);
			}
		}

		public void SetActive()
		{
			if ((bool)tournamentDescField)
			{
				tournamentDescription = m_data.callToAction.ToUpper();
				if ((bool)descriptionTrunc)
				{
					descriptionTrunc.Refresh();
				}
			}
			SetTimeStatus();
		}

		public void SetRegistration()
		{
			SetActive();
		}

		public void SetOverview(DRLTournamentData p_data)
		{
			m_data = p_data;
			tournamentName = m_data.title.ToUpper();
			LoadBackground();
			SetTimeStatus();
		}

		private void LoadBackground()
		{
			string img_url = m_data.imageURL;
			if (string.IsNullOrEmpty(img_url))
			{
				imageField.enabled = false;
				imageField.texture = null;
				return;
			}
			if (backgroundCache.ContainsKey(img_url))
			{
				image = backgroundCache[img_url];
				imageField.enabled = true;
				return;
			}
			imageFader.Fade(0f, 0.001f);
			m_photo_loader = Web.Load(img_url, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_result == null) && base.validContext && !(imageField == null) && !(imageFader == null))
				{
					imageFader.FadeIn();
					image = p_result;
					backgroundCache[img_url] = p_result;
					imageField.enabled = true;
				}
			});
		}

		public override void Build()
		{
			base.Build();
			FocusResize focusResize = GetComponent<FocusResize>();
			if (!focusResize)
			{
				focusResize = base.gameObject.AddComponent<FocusResize>();
			}
			focusResize.enabled = true;
			focusResize.min = new Vector2(420f, 540f);
			focusResize.max = new Vector2(500f, 650f);
			focusResize.duration = 0.1f;
			image = null;
			((RectTransform)base.transform).sizeDelta = focusResize.min;
		}

		public void Clear()
		{
			Tween.Kill(this);
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
		}

		private void SetTimeStatus()
		{
			Localization locale = base.app.model.storage.locale;
			DateTime dateTime = m_data.registerEndDate;
			DateTime registerStartDate = m_data.registerStartDate;
			double totalSeconds = (registerStartDate - m_data.currentTime).TotalSeconds;
			if (totalSeconds > 0.0)
			{
				dateTime = registerStartDate;
			}
			double totalDays = (dateTime - m_data.currentTime).TotalDays;
			bool active = m_data.status == TournamentState.active;
			bool flag = m_data.status == TournamentState.canceled || m_data.status == TournamentState.fail;
			if (tournamentStartedBackground != null)
			{
				tournamentStartedBackground.SetActive(value: false);
			}
			if (totalDays < 0.0)
			{
				string text = locale.Get("vdrl.label.started", "STARTED");
				if (flag)
				{
					text = locale.Get("vdrl.label.event-canceled", "CANCELLED");
				}
				tournamentDateCaption = text;
				tournamentDate = string.Empty;
				if (tournamentStartedBackground != null)
				{
					tournamentStartedBackground.SetActive(active);
				}
				StopTimerActivity();
				if ((bool)dateStopwatchIcon)
				{
					dateStopwatchIcon.SetActive(value: false);
				}
				return;
			}
			int num = (int)Math.Floor(totalDays);
			double num2 = (totalDays - (double)num) * 24.0;
			int num3 = (int)Math.Floor(num2);
			double num4 = (num2 - (double)num3) * 60.0;
			int num5 = (int)Math.Floor(num4);
			Math.Floor((num4 - (double)num5) * 60.0);
			string text2 = ((num == 1) ? locale.Get("vdrl.label.day", "DAY") : locale.Get("vdrl.label.days", "DAYS"));
			string text3 = ((num3 == 1) ? locale.Get("vdrl.label.hour", "HOUR") : locale.Get("vdrl.label.hours", "HOURS"));
			string text4 = ((num5 == 1) ? locale.Get("vdrl.label.minute", "MIN") : locale.Get("vdrl.label.minutes", "MINS"));
			string text5 = ((num > 0) ? (num + " " + text2 + " / ") : "");
			text5 += ((num3 > 0) ? (num3 + " " + text3 + " / ") : "");
			text5 += ((num5 > 0) ? (num5 + " " + text4) : "");
			tournamentDateCaption = ((totalSeconds < 0.0) ? locale.Get("vdrl.label.starts-in", "STARTS IN:") : locale.Get("vdrl.label.registrations-opens", "OPENS:")) + " ";
			tournamentDate = text5;
			if (flag)
			{
				tournamentDateCaption = locale.Get("vdrl.label.failed", "FAILED");
				tournamentDate = "";
			}
			timer = (float)(dateTime - m_data.currentTime).TotalSeconds;
			if ((bool)dateStopwatchIcon)
			{
				dateStopwatchIcon.SetActive(!flag);
			}
			StartTimerActivity();
		}

		public void StartTimerActivity()
		{
			if (timerActivity != null)
			{
				if (timerActivity.IsRunning)
				{
					return;
				}
				timerActivity = null;
			}
			timerActivity = Run((Func<bool>)delegate
			{
				if (timer <= 0f)
				{
					return false;
				}
				timer -= Time.deltaTime;
				UpdateTournamentCountdown();
				return true;
			}, 0f, false);
		}

		public void StopTimerActivity()
		{
			if (timerActivity != null)
			{
				timerActivity.Stop();
				timerActivity = null;
			}
		}

		public void OnDisable()
		{
			StopTimerActivity();
		}

		private void UpdateTournamentCountdown()
		{
			Localization locale = base.app.model.storage.locale;
			int num = (int)Mathf.Floor(timer / 86400f);
			int num2 = (int)Mathf.Floor((timer - (float)(num * 86400)) / 3600f);
			int num3 = (int)Mathf.Floor((timer - (float)(num * 86400) - (float)num2 * 3600f) / 60f);
			string text = ((num == 1) ? locale.Get("vdrl.label.day", "DAY") : locale.Get("vdrl.label.days", "DAYS"));
			string text2 = ((num2 == 1) ? locale.Get("vdrl.label.hour", "HOUR") : locale.Get("vdrl.label.hours", "HOURS"));
			string text3 = ((num3 <= 1) ? locale.Get("vdrl.label.minute", "MIN") : locale.Get("vdrl.label.minutes", "MINS"));
			string text4 = ((num > 0) ? (num + " " + text + " / ") : "");
			text4 += ((num2 > 0) ? (num2 + " " + text2 + " / ") : "");
			text4 += ((num3 > 0) ? (num3 + " " + text3) : "");
			if (num == 0 && num2 == 0 && num3 == 0)
			{
				text4 = text4 + "<1 " + text3;
			}
			tournamentDate = text4;
			if (num2 >= 0 && timer <= 0f)
			{
				tournamentDate = string.Empty;
				tournamentDateCaption = locale.Get("vdrl.label.started", "STARTED");
			}
		}

		private void OnDestroy()
		{
			if (m_backgroundCache != null)
			{
				backgroundCache.Clear();
			}
		}
	}
}
