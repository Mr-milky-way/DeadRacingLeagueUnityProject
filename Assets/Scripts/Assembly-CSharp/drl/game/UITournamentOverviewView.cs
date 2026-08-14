using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentOverviewView : UIScreenView
	{
		private Canvas m_canvas;

		public DRLTournamentData tournament;

		public RectTransform body;

		[Header("Action buttons")]
		public UIElementView registerButton;

		public UIElementView unRegisterButton;

		public UIElementView subscribeMeButton;

		public UIElementView unSubscribeMeButton;

		public UIElementView standingsButton;

		public UIElementView enterButton;

		public UIElementView spectateButton;

		public GameObject watchButton;

		public GameObject termsAndAge;

		public DRLToggleView ageConfirmation;

		public DRLToggleView termsAccepting;

		public GameObject termsButton;

		public List<UINavigation> rightButtons;

		[Header("Video")]
		public VideoPlayer videoPlayer;

		public FadeComponent videoPlayerFader;

		public FadeComponent videoPlayerIcon;

		[Header("Text")]
		public Text title;

		public Text description;

		public UITruncateText descriptionTrunc;

		public Text registrationDates;

		public Text registeredPlayers;

		public Text droneClass;

		public UITruncateText droneClassTrunc;

		public Text ageRestrictionField;

		[Header("Images")]
		public RawImage prizeImage;

		public FadeComponent prizeImageFader;

		[Header("Content")]
		public UICardButtonTournament bigCard;

		public GameObject ageRestriction;

		public GameObject prizeArea;

		public GameObject prizeAreaSpace;

		public FadeComponent contentFade;

		public FadeComponent feedbackFade;

		public List<GameObject> feedbacks;

		private UITournamentsOverviewFeedbackType status;

		[HideInInspector]
		public int minimumSkill;

		public DroneRigData rig;

		private AsyncRequest m_background_loader;

		private AsyncRequest m_image_loader;

		private AsyncRequest m_prize_loader;

		private WebAsyncRequest m_communityDrones;

		private static Dictionary<string, Texture> m_backgroundCache;

		private Dictionary<string, Texture> m_prizeImageCache;

		private bool initialAgeRestriction = true;

		private bool lastAgeRestriction;

		private WebAsyncRequest m_subscribed_service;

		public Canvas canvas
		{
			get
			{
				if (!m_canvas)
				{
					return m_canvas = Hierarchy.FindReverse<Canvas>(base.transform);
				}
				return m_canvas;
			}
		}

		public Texture prize
		{
			set
			{
				UIReflection.Set(prizeImage, value);
			}
		}

		internal static Dictionary<string, Texture> backgroundCache => Reflection<object>.Assert(ref m_backgroundCache);

		internal Dictionary<string, Texture> prizeImageCache => Reflection<object>.Assert(ref m_prizeImageCache);

		public void Set(DRLTournamentData p_data, int p_minSkill)
		{
			rig = null;
			tournament = p_data;
			bigCard.SetOverview(p_data);
			minimumSkill = p_minSkill;
			if (tournament.droneClass != 1)
			{
				droneClass.text = ((tournament.droneClass == 0) ? base.app.model.storage.locale.Get("vdrl.label.all-drones", "ALL DRONES") : (tournament.droneClass + "\""));
			}
			else
			{
				LoadCustomDrone();
			}
			title.text = tournament.title.ToUpper();
			description.text = tournament.description.ToUpper();
			if ((bool)descriptionTrunc)
			{
				descriptionTrunc.Refresh();
			}
			bool flag = string.IsNullOrEmpty(description.text);
			description.gameObject.SetActive(!flag);
			title.gameObject.SetActive(!flag);
			LayoutRebuilder.ForceRebuildLayoutImmediate(body);
			if (!flag)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(description.rectTransform);
			}
			Localization locale = base.app.model.storage.locale;
			registrationDates.text = GenerateTimeString();
			int registeredPlayersCount = tournament.registeredPlayersCount;
			int maxPlayers = tournament.maxPlayers;
			registeredPlayers.text = ((maxPlayers <= 0) ? registeredPlayersCount.ToString() : (registeredPlayersCount + "/" + maxPlayers));
			Text text = registeredPlayers;
			text.text = text.text + " " + locale.Get("vdrl.label.registered", "REGISTERED");
			droneClassTrunc.Refresh();
			termsButton.SetActive(!string.IsNullOrEmpty(tournament.termsURL));
			termsAccepting.gameObject.SetActive(!string.IsNullOrEmpty(tournament.termsURL));
			if ((bool)ageRestriction)
			{
				string text2 = locale.Get("vdrl.label.age-restriction", "I AM OVER THE AGE OF");
				int num = tournament.ageRestriction;
				if (num == 0)
				{
					num = 18;
				}
				ageRestriction.SetActive(tournament.ageRestricted && !termsButton.activeSelf);
				ageRestrictionField.text = text2 + " " + num;
			}
			if (initialAgeRestriction)
			{
				initialAgeRestriction = false;
				lastAgeRestriction = tournament.ageRestricted;
			}
			else if (lastAgeRestriction != tournament.ageRestricted)
			{
				lastAgeRestriction = tournament.ageRestricted;
			}
			termsAndAge.SetActive(termsButton.activeSelf || ageRestriction.activeSelf);
			OnTermsAndAgeChange();
			if (tournament.prizeURL != string.Empty)
			{
				if (prizeImageCache.ContainsKey(tournament.prizeURL))
				{
					prize = prizeImageCache[tournament.prizeURL];
					return;
				}
				prizeImageFader.FadeOut(0.001f);
				prizeArea.SetActive(value: true);
				m_prize_loader = Web.Load(tournament.prizeURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_result == null))
					{
						prizeImageFader.FadeIn();
						prize = p_result;
						prizeImageCache[tournament.prizeURL] = p_result;
					}
				});
			}
			else
			{
				prizeArea.SetActive(value: false);
				prizeImageFader.FadeOut(0.001f);
			}
		}

		private string GenerateTimeString()
		{
			_ = string.Empty;
			DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(tournament.registerEndDate, TimeZoneInfo.Local);
			_ = tournament.registerEndDate;
			string text = base.app.model.storage.locale.Get("vdrl.label.begins", "BEGINS");
			string text2 = "";
			string language = Localization.instance.language;
			text2 = ((language == null || !(language == "zh")) ? dateTime.ToString("dddd, MMMM dd yyyy - h:mmtt") : dateTime.ToString("yyyy'年'MM'月'dd'日' - h:mmtt"));
			return (text + ": " + text2).ToUpper();
		}

		public void LoadCustomDrone(Action p_callback = null)
		{
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			ServiceModel service = base.app.model.service;
			Debug.Log("<b><color=white>UITournamentOverviewView> Load Custom Drone " + tournament.droneGuid + "</color></b>");
			rig = garage.GetOriginalByGUID(tournament.droneGuid);
			if (rig != null)
			{
				droneClass.text = rig.name.ToUpper();
				if (p_callback != null)
				{
					if (rig != null)
					{
						base.app.model.storage.state.player.garage.activeRigData = rig;
					}
					p_callback();
				}
				return;
			}
			if (m_communityDrones != null)
			{
				m_communityDrones.Cancel();
				m_communityDrones = null;
			}
			m_communityDrones = service.GetCommunityDrones(tournament.droneGuid, delegate(DRLCommunityDroneResult p_result)
			{
				DRLCommunityDroneData dRLCommunityDroneData = ((p_result.data.Length == 0) ? null : p_result.data[0]);
				rig = ((dRLCommunityDroneData == null) ? null : DroneRigData.FromJson(dRLCommunityDroneData.droneRigData));
				droneClass.text = ((rig == null) ? "INVALID DRONE" : rig.name.ToUpper());
				if (p_callback != null)
				{
					if (rig != null)
					{
						base.app.model.storage.state.player.garage.activeRigData = rig;
					}
					p_callback();
				}
			});
		}

		private bool IsRegistrationTime()
		{
			DateTime currentTime = tournament.currentTime;
			DateTime registerEndDate = tournament.registerEndDate;
			DateTime registerStartDate = tournament.registerStartDate;
			if (currentTime.CompareTo(registerEndDate) < 0 && currentTime.CompareTo(registerStartDate) >= 0)
			{
				return true;
			}
			return false;
		}

		public bool IsPlayerRegistered()
		{
			if (tournament == null)
			{
				return false;
			}
			string playerId = base.app.model.storage.state.player.profile.playerId;
			return tournament.IsPlayerRegistered(playerId);
		}

		private bool AreTermsAndAgeFulfilled()
		{
			bool flag = false;
			bool flag2 = false;
			if (!termsAccepting.gameObject.activeSelf)
			{
				flag = true;
			}
			else if (termsAccepting.toggle.isOn)
			{
				flag = true;
			}
			if (!ageRestriction.activeSelf)
			{
				flag2 = true;
			}
			else if (ageConfirmation.toggle.isOn)
			{
				flag2 = true;
			}
			return flag && flag2;
		}

		private void CheckIsPlayerSubscribed()
		{
			if (base.app.model.service == null)
			{
				return;
			}
			if (m_subscribed_service != null)
			{
				m_subscribed_service.Cancel();
			}
			m_subscribed_service = base.app.model.service.CheckUserSubscription(tournament.guid, delegate(DRLServiceResult p_result)
			{
				m_subscribed_service = null;
				if (base.validContext)
				{
					if (p_result == null || p_result.data == null)
					{
						Debug.LogWarning("UITournamentOverviewView> User subscription result is null");
					}
					else
					{
						Serialize.FromJson<DRLTournamentSubscription[]>(p_result.data.ToString());
						unSubscribeMeButton.gameObject.SetActive(value: false);
						subscribeMeButton.gameObject.SetActive(value: false);
					}
				}
			});
		}

		public void SetSubscriptionButtons(bool p_subscribed)
		{
			unSubscribeMeButton.gameObject.SetActive(value: false);
			subscribeMeButton.gameObject.SetActive(value: true);
			RefreshControlButtons();
		}

		public void RefreshControlButtons()
		{
			enterButton.gameObject.SetActive(value: false);
			spectateButton.gameObject.SetActive(value: false);
			registerButton.gameObject.SetActive(value: false);
			unRegisterButton.gameObject.SetActive(value: false);
			standingsButton.gameObject.SetActive(value: false);
			if (tournament != null)
			{
				bool allowRegistrations = tournament.allowRegistrations;
				bool flag = minimumSkill >= 0;
				bool flag2 = IsPlayerRegistered();
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				bool num = tournament.status == TournamentState.active;
				bool flag6 = true;
				bool flag7 = tournament.maxPlayers <= 0 || tournament.registeredPlayersCount < tournament.maxPlayers;
				flag3 = allowRegistrations && flag && !flag2 && flag7;
				flag4 = allowRegistrations && flag && flag2;
				flag5 = num;
				flag6 = base.app.model.storage.state.player.profile.isDeveloper || !tournament.disablePublicSpectators;
				bool active = !flag6 && !string.IsNullOrEmpty(tournament.streamingURL);
				if (tournament.status == TournamentState.canceled || tournament.status == TournamentState.fail)
				{
					return;
				}
				registerButton.gameObject.SetActive(flag3);
				unRegisterButton.gameObject.SetActive(flag4);
				unRegisterButton.interactable = flag4;
				spectateButton.gameObject.SetActive(flag5 && !flag2 && flag6);
				enterButton.gameObject.SetActive(flag5 && flag2);
				watchButton.SetActive(active);
				if (enterButton.gameObject.activeSelf || unRegisterButton.gameObject.activeSelf)
				{
					termsAccepting.toggle.isOn = true;
					ageConfirmation.toggle.isOn = true;
					termsAccepting.SetState(p_flag: true);
					ageConfirmation.SetState(p_flag: true);
					OnTermsAndAgeChange();
				}
			}
			for (int i = 0; i < rightButtons.Count; i++)
			{
				if (!rightButtons[i].gameObject.activeInHierarchy || i >= rightButtons.Count - 1)
				{
					continue;
				}
				for (int j = i + 1; j < rightButtons.Count; j++)
				{
					if (rightButtons[j].gameObject.activeInHierarchy)
					{
						rightButtons[i].down = rightButtons[j];
						rightButtons[j].up = rightButtons[i];
					}
				}
			}
		}

		public void ResetTermsAndAgeToggles()
		{
			termsAccepting.toggle.isOn = false;
			ageConfirmation.toggle.isOn = false;
			termsAccepting.SetState(p_flag: false);
			ageConfirmation.SetState(p_flag: false);
		}

		public void SetFeedback(UITournamentsOverviewFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UITournamentsOverviewFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UITournamentsOverviewFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			status = p_type;
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.2f, 0.05f, Cubic.Out);
				contentFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UITournamentsOverviewFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void OnTermsAndAgeChange()
		{
			registerButton.interactable = AreTermsAndAgeFulfilled();
		}

		private void OnDestroy()
		{
			if (m_prizeImageCache != null)
			{
				m_prizeImageCache.Clear();
			}
		}
	}
}
