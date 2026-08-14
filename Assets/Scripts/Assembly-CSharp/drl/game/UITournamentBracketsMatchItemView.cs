using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentBracketsMatchItemView : UICardView
	{
		private static Dictionary<string, Texture> m_cache;

		private DRLTournamentMatchData m_data;

		public string matchID;

		[Header("Card Accent")]
		public Transform miniMapSpriteTran;

		public SpriteRenderer miniMapSprite;

		public Color miniMapActiveColor;

		public Color miniMapDefaultColor;

		public FadeComponent activeOutline;

		public FadeComponent outline;

		[Header("Body")]
		public int maxPilots = 6;

		public Color pilotBackground1;

		public Color pilotBackground2;

		public RectTransform midPointL;

		public RectTransform midPointR;

		public RectTransform midPointLstatic;

		public RectTransform midPointRstatic;

		public FadeComponent bodyContracted;

		public FadeComponent bodyExpanded;

		public ListComponent contractedPilotsList;

		public ListComponent expandedPilotsList;

		public RectTransform pilotsExpandedRect;

		[Header("Header")]
		public Image plusMinusImage;

		public Image listImage;

		public Sprite plusSprite;

		public Sprite minusSprite;

		public Text groupNameField;

		public Text currentHeatField;

		public Text pilotCountField;

		public GameObject timeField;

		public GameObject heatsField;

		public GameObject pilotsField;

		[Header("Footer")]
		public GameObject footer;

		public GameObject footerProgressRow;

		public GameObject footerSpectateRow;

		public GameObject footerEnterRow;

		[Header("Progress and Status")]
		public Image statusIcon;

		public Text statusLabel;

		public GameObject status;

		public GameObject progress;

		public RectTransform progressBar;

		public RectTransform progressBarFillRT;

		[Header("Lobby actions")]
		public UIElementView enterButton;

		public UIElementView spectateButton;

		public UINavigation resultsButton;

		public Text wideEnterText;

		public UITruncateText wideEnterTrunc;

		public Text enterText;

		public Text spectateText;

		public Text heatText;

		public Text enterCountdownText;

		public Text spectateCountdownText;

		public GameObject heatOverlay;

		public FadeComponent waitOverlay;

		public Text waitOverlayText;

		[Header("Contracting/Expanding")]
		public float contractedSize = 90f;

		public float profileLineHeight = 45f;

		public float extendedFooterHeight = 45f;

		public float footerButtonHeight = 42f;

		public float expandedSize;

		public float contractedSpriteSize;

		public float rectToSpriteRatio;

		private float expandedSpriteSize;

		private bool isExpanded;

		public float onePilotSlotHeight = 140f;

		[Header("Pilots order")]
		public Vector2 contractedStartPos;

		public Vector2 contractedDelta;

		public Vector2 expandedStartPos;

		public Vector2 expandedDelta;

		private Dictionary<string, RectTransform> expandedSlots;

		private Dictionary<string, RectTransform> contractedSlots;

		private float p_roomCountdown;

		public MonoActivity roomCountdownActivity;

		private bool playersMatch;

		public TournamentRoundGameMode gameMode;

		public string roundTitle;

		private int m_currentHeat;

		private AsyncRequest m_photo_loader;

		public override UICardType type => UICardType.ButtonTournamentMatchItem;

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public UINavigation navigation => AssertLocal<UINavigation>("navigation");

		public new DRLTournamentMatchData data
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
				matchID = m_data?.Id;
			}
		}

		public float roomCountdown
		{
			get
			{
				return p_roomCountdown;
			}
			set
			{
				p_roomCountdown = value;
				if (value > 0f)
				{
					StartRoomCountdownActivity();
				}
				else
				{
					StopRoomCountdownActivity();
				}
			}
		}

		public RectTransform mpL
		{
			get
			{
				if (gameMode == TournamentRoundGameMode.leaderboard)
				{
					return midPointLstatic;
				}
				return midPointL;
			}
		}

		public RectTransform mpR
		{
			get
			{
				if (gameMode == TournamentRoundGameMode.leaderboard)
				{
					return midPointRstatic;
				}
				return midPointR;
			}
		}

		public float height
		{
			get
			{
				return ((RectTransform)base.transform).sizeDelta.y;
			}
			set
			{
				RectTransform obj = (RectTransform)base.transform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = value;
				obj.sizeDelta = sizeDelta;
			}
		}

		public string PilotCount
		{
			set
			{
				if ((bool)pilotCountField)
				{
					pilotCountField.text = value;
				}
			}
		}

		public int CurrentHeat
		{
			get
			{
				return m_currentHeat;
			}
			set
			{
				m_currentHeat = value;
				string text = ((m_currentHeat == 0) ? "" : m_currentHeat.ToString());
				if ((bool)currentHeatField)
				{
					currentHeatField.text = text;
				}
			}
		}

		public void Clear()
		{
			Tween.Kill(this);
			height = contractedSize;
			data = null;
			if ((bool)bodyContracted)
			{
				bodyContracted.FadeIn(0f);
			}
			if ((bool)bodyExpanded)
			{
				bodyExpanded.FadeOut(0f);
			}
			if ((bool)plusMinusImage)
			{
				plusMinusImage.sprite = plusSprite;
			}
			SetMatchState(TournamentMatchState.idle);
			for (int i = 0; i < maxPilots; i++)
			{
				FadeComponent fadeComponent = contractedPilotsList.Get<FadeComponent>(i);
				if ((bool)fadeComponent)
				{
					fadeComponent.FadeOut(0f);
				}
			}
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			expandedSlots = new Dictionary<string, RectTransform>();
			contractedSlots = new Dictionary<string, RectTransform>();
			expandedPilotsList.Clear();
		}

		public void SetLobbyActions(bool p_canJoin, bool p_canSpectate, bool p_inRace = false, int p_heatNumber = 0)
		{
			if (data.state == TournamentMatchState.active)
			{
				if ((bool)heatOverlay)
				{
					_ = heatOverlay.gameObject.activeSelf;
					heatOverlay.gameObject.SetActive(p_inRace);
					heatText.text = base.app.model.storage.locale.Get("vdrl.label.heat", "HEAT") + " " + m_currentHeat;
					UpdateMatchHeight();
				}
				if (IsPlayerInMatch() && (bool)enterButton)
				{
					enterButton.interactable = !p_inRace && p_canJoin;
					_ = spectateButton.gameObject.activeSelf;
					spectateButton.gameObject.SetActive(value: false);
					UpdateMatchHeight();
				}
				else if ((bool)spectateButton)
				{
					_ = spectateButton.gameObject.activeSelf;
					spectateButton.gameObject.SetActive(!p_inRace && p_canSpectate);
					UpdateMatchHeight();
				}
			}
		}

		public void ColorRoomPlayers(Lobby.NetworkRoomInfo p_roomInfo)
		{
			for (int i = 0; i < expandedPilotsList.Count; i++)
			{
				UITournamentBracketsPilotItemView uITournamentBracketsPilotItemView = expandedPilotsList.Get<UITournamentBracketsPilotItemView>(i);
				RawImage profileImage = uITournamentBracketsPilotItemView.profileImage;
				Image profileStripe = uITournamentBracketsPilotItemView.profileStripe;
				float num = ((p_roomInfo == null || !p_roomInfo.SteamIds.Contains(uITournamentBracketsPilotItemView.data.playerId.ToString())) ? 1 : 0);
				profileImage.material.SetFloat("_BlackAndWhite", num);
				profileStripe.color = ((num == 0f) ? uITournamentBracketsPilotItemView.data.profileColor : Color.gray);
				if (i < 6)
				{
					contractedPilotsList.Get<RawImage>(i).material.SetFloat("_BlackAndWhite", num);
				}
			}
		}

		public void ColorWinners()
		{
			if (data == null || data.players == null)
			{
				return;
			}
			for (int i = 0; i < data.players.Length; i++)
			{
				if (data.players[i] == null)
				{
					continue;
				}
				string playerId = data.players[i].playerId;
				if (!expandedSlots.ContainsKey(playerId))
				{
					continue;
				}
				RectTransform rectTransform = expandedSlots[playerId];
				if (rectTransform == null)
				{
					continue;
				}
				UITournamentBracketsPilotItemView component = rectTransform.GetComponent<UITournamentBracketsPilotItemView>();
				if (!(component == null))
				{
					RawImage profileImage = component.profileImage;
					Image profileStripe = component.profileStripe;
					float num = ((!data.players[i].isWinner) ? 1 : 0);
					profileImage.material.SetFloat("_BlackAndWhite", num);
					profileStripe.color = ((num == 0f) ? component.data.profileColor : Color.gray);
					if (contractedSlots.ContainsKey(playerId))
					{
						contractedSlots[playerId].GetComponent<RawImage>().material.SetFloat("_BlackAndWhite", num);
					}
				}
			}
		}

		public void ColorPlayers(List<string> p_ids)
		{
			for (int i = 0; i < data.players.Length; i++)
			{
				UITournamentBracketsPilotItemView uITournamentBracketsPilotItemView = expandedPilotsList.Get<UITournamentBracketsPilotItemView>(i);
				if (!(uITournamentBracketsPilotItemView == null))
				{
					RawImage profileImage = uITournamentBracketsPilotItemView.profileImage;
					Image profileStripe = uITournamentBracketsPilotItemView.profileStripe;
					float num = ((!p_ids.Contains(uITournamentBracketsPilotItemView.data.playerId)) ? 1 : 0);
					profileImage.material.SetFloat("_BlackAndWhite", num);
					profileStripe.color = ((num == 0f) ? uITournamentBracketsPilotItemView.data.profileColor : Color.gray);
					if (i < 6)
					{
						contractedPilotsList.Get<RawImage>(i).material.SetFloat("_BlackAndWhite", num);
					}
				}
			}
		}

		public void SetMatchState(TournamentMatchState p_state)
		{
			switch (p_state)
			{
			case TournamentMatchState.idle:
				if ((bool)spectateButton)
				{
					spectateButton.gameObject.SetActive(value: false);
				}
				if ((bool)statusLabel)
				{
					statusLabel.text = base.app.model.storage.locale.Get("vdrl.label.inactive", "INACTIVE");
				}
				StopRoomCountdownActivity();
				base.interactable = false;
				footerProgressRow.SetActive(value: true);
				progress.SetActive(value: false);
				footerEnterRow.SetActive(value: false);
				break;
			case TournamentMatchState.waiting:
				if ((bool)spectateButton)
				{
					spectateButton.gameObject.SetActive(value: false);
				}
				if ((bool)statusLabel)
				{
					statusLabel.text = base.app.model.storage.locale.Get("vdrl.label.waiting", "WAITING");
				}
				StopRoomCountdownActivity();
				base.interactable = true;
				footerProgressRow.SetActive(value: true);
				progress.SetActive(value: false);
				footerEnterRow.SetActive(value: false);
				break;
			case TournamentMatchState.active:
				if ((bool)progress)
				{
					progress.SetActive(value: false);
				}
				if ((bool)statusLabel)
				{
					statusLabel.text = base.app.model.storage.locale.Get("vdrl.label.active", "ACTIVE");
				}
				HideWaitOverlay();
				base.interactable = true;
				break;
			case TournamentMatchState.complete:
				if ((bool)statusIcon)
				{
					statusIcon.color = Color.red;
				}
				if ((bool)statusLabel)
				{
					statusLabel.text = base.app.model.storage.locale.Get("vdrl.label.complete", "MATCH COMPLETE");
				}
				if ((bool)progress)
				{
					progress.SetActive(value: false);
				}
				if ((bool)spectateButton)
				{
					spectateButton.gameObject.SetActive(value: false);
				}
				if (playersMatch)
				{
					playersMatch = false;
					SetPlayersMatchLayout(p_true: false);
				}
				UpdateMatchHeight();
				HideHeatOverlay();
				HideWaitOverlay();
				StopRoomCountdownActivity();
				base.interactable = true;
				break;
			default:
				StopRoomCountdownActivity();
				if ((bool)statusLabel)
				{
					statusLabel.text = base.app.model.storage.locale.Get("vdrl.label.failed", "FAILED");
				}
				if ((bool)progress)
				{
					progress.SetActive(value: false);
				}
				if ((bool)spectateButton)
				{
					spectateButton.gameObject.SetActive(value: false);
				}
				base.interactable = false;
				HideHeatOverlay();
				HideWaitOverlay();
				break;
			}
		}

		public void SetPlayersMatchLayout(bool p_true)
		{
			footerEnterRow.SetActive(p_true);
			miniMapSprite.color = (p_true ? miniMapActiveColor : miniMapDefaultColor);
			if (!IsExpanded())
			{
				footerProgressRow.SetActive(!p_true && (data.state == TournamentMatchState.active || data.state == TournamentMatchState.complete || data.state == TournamentMatchState.fail || data.state == TournamentMatchState.waiting));
			}
			if (p_true)
			{
				activeOutline.FadeIn();
				outline.FadeOut();
			}
			else if (activeOutline.alpha > 0f)
			{
				activeOutline.FadeOut();
				outline.FadeIn();
			}
		}

		public void SetPlayersRaceMatch()
		{
			footerEnterRow.SetActive(data.state == TournamentMatchState.active);
			enterButton.interactable = data.state == TournamentMatchState.active;
		}

		public bool IsPlayerInMatch()
		{
			string text = base.app.model.service.backend.playerId.ToString();
			if (data.playerIds == null || data.playerIds.Length == 0)
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < data.playerIds.Length; i++)
			{
				flag = data.playerIds[i] == text;
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public void UpdateMatchCountdown()
		{
			if (p_roomCountdown > 0f)
			{
				enterCountdownText.text = "(" + p_roomCountdown.ToString("N0") + ")";
				spectateCountdownText.text = "(" + p_roomCountdown.ToString("N0") + ")";
			}
			else
			{
				enterCountdownText.text = string.Empty;
				spectateCountdownText.text = string.Empty;
			}
		}

		private void StartRoomCountdownActivity()
		{
			if (roomCountdownActivity != null)
			{
				if (roomCountdownActivity.IsRunning)
				{
					return;
				}
				roomCountdownActivity = null;
			}
			roomCountdownActivity = Run((Func<bool>)delegate
			{
				if (data.state != TournamentMatchState.active)
				{
					return false;
				}
				p_roomCountdown -= Time.deltaTime;
				UpdateMatchCountdown();
				return !(p_roomCountdown < 0f);
			}, 0f, false);
		}

		private void StopRoomCountdownActivity()
		{
			if (roomCountdownActivity != null)
			{
				roomCountdownActivity.Stop();
				roomCountdownActivity = null;
				p_roomCountdown = 0f;
				UpdateMatchCountdown();
			}
		}

		public void Set(DRLTournamentMatchData p_data, bool p_playersGroup, bool p_init = false, bool p_showGroupName = true)
		{
			if (p_data == null)
			{
				return;
			}
			Clear();
			data = p_data;
			SetMatchState(data.state);
			if ((bool)groupNameField)
			{
				groupNameField.text = (p_showGroupName ? (base.app.model.storage.locale.Get("vdrl.label.group", "GROUP") + " " + data.groupNumber) : "");
			}
			int num = data.players.Length;
			PilotCount = "0/" + data.playersCount;
			expandedSize = contractedSize + (float)(num - 1) * profileLineHeight + extendedFooterHeight;
			float imgFadeInTime = 0.3f;
			if (gameMode == TournamentRoundGameMode.leaderboard)
			{
				enterText.gameObject.SetActive(value: false);
				wideEnterText.gameObject.SetActive(value: true);
				wideEnterText.text = base.app.model.storage.locale.Get("vdrl.label.solo-race", "START") + " " + roundTitle;
				wideEnterTrunc.Refresh();
				listImage.gameObject.SetActive(value: true);
				plusMinusImage.gameObject.SetActive(value: false);
				timeField.gameObject.SetActive(value: true);
				pilotsField.gameObject.SetActive(value: true);
				heatsField.gameObject.SetActive(value: false);
				playersMatch = IsPlayerInMatch() && data.state == TournamentMatchState.active;
				Expand();
				if (playersMatch)
				{
					SetPlayersRaceMatch();
				}
				SetPlayersMatchLayout(playersMatch);
			}
			else
			{
				enterText.text = base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN");
				enterText.gameObject.SetActive(value: true);
				wideEnterText.gameObject.SetActive(value: false);
				enterButton.interactable = false;
				playersMatch = p_playersGroup;
				SetPlayersMatchLayout(p_playersGroup);
				listImage.gameObject.SetActive(value: false);
				plusMinusImage.gameObject.SetActive(value: true);
				timeField.gameObject.SetActive(value: false);
				pilotsField.gameObject.SetActive(value: true);
				heatsField.gameObject.SetActive(value: true);
				UpdateMatchHeight();
			}
			for (int i = 0; i < num; i++)
			{
				DRLTournamentPlayerData profile = data.players[i];
				Texture reuseImage = null;
				if (i < 6)
				{
					if (!string.IsNullOrEmpty(profile.playerId) && !contractedSlots.ContainsKey(profile.playerId))
					{
						contractedSlots.Add(profile.playerId, contractedPilotsList[i].GetComponent<RectTransform>());
					}
					RawImage cph = contractedPilotsList[i].GetComponent<RawImage>();
					FadeComponent cfc = contractedPilotsList.Get<FadeComponent>(i);
					if ((bool)cph)
					{
						if (p_init)
						{
							float value = ((!string.IsNullOrEmpty(profile.playerId)) ? 1 : 0);
							cph.material = UnityEngine.Object.Instantiate(cph.material);
							cph.material.SetFloat("_BlackAndWhite", value);
						}
						if (!string.IsNullOrEmpty(profile.profileThumbURL))
						{
							if (cache.ContainsKey(profile.profileThumbURL))
							{
								cph.texture = cache[profile.profileThumbURL];
								reuseImage = cph.texture;
								if ((bool)cfc && cfc.alpha < 0.99f)
								{
									cfc.FadeIn(imgFadeInTime);
								}
							}
							else
							{
								m_photo_loader = Web.Load(profile.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
								{
									if (base.validContext)
									{
										cph.texture = p_result;
										reuseImage = p_result;
										cache[profile.profileThumbURL] = p_result;
										if ((bool)cfc && cfc.alpha < 0.99f)
										{
											cfc.FadeIn(imgFadeInTime);
										}
									}
								});
							}
						}
					}
				}
				UITournamentBracketsPilotItemView uITournamentBracketsPilotItemView = expandedPilotsList.Push<UITournamentBracketsPilotItemView>();
				uITournamentBracketsPilotItemView.Set(profile, reuseImage);
				if (p_init)
				{
					float value2 = ((!string.IsNullOrEmpty(profile.playerId)) ? 1 : 0);
					uITournamentBracketsPilotItemView.profileImage.material = UnityEngine.Object.Instantiate(uITournamentBracketsPilotItemView.profileImage.material);
					uITournamentBracketsPilotItemView.profileImage.material.SetFloat("_BlackAndWhite", value2);
				}
				if (!string.IsNullOrEmpty(profile.playerId) && !expandedSlots.ContainsKey(profile.playerId))
				{
					expandedSlots.Add(profile.playerId, uITournamentBracketsPilotItemView.GetComponent<RectTransform>());
				}
			}
			UpdatePilotsOrder();
		}

		public bool IsExpanded()
		{
			return isExpanded;
		}

		private void SetExpandedProfileImagesHeight(float p_height)
		{
			if (expandedPilotsList == null)
			{
				return;
			}
			for (int i = 0; i < expandedPilotsList.Count; i++)
			{
				RectTransform rectTransform = expandedPilotsList.Get<RectTransform>(i);
				if ((bool)rectTransform)
				{
					Vector2 sizeDelta = rectTransform.sizeDelta;
					sizeDelta.y = p_height;
					rectTransform.sizeDelta = sizeDelta;
				}
			}
		}

		public void Expand()
		{
			Tween.Kill(this);
			isExpanded = true;
			if ((bool)bodyContracted)
			{
				bodyContracted.FadeOut(0.1f);
			}
			if ((bool)bodyExpanded)
			{
				bodyExpanded.FadeIn(0.1f);
			}
			if ((bool)plusMinusImage)
			{
				plusMinusImage.sprite = minusSprite;
			}
			UpdateMatchHeight();
		}

		private void UpdateMatchHeight(bool p_showProgress = true)
		{
			bool flag = data != null && data.state == TournamentMatchState.active;
			bool flag2 = (spectateButton.gameObject.activeSelf || heatOverlay.gameObject.activeSelf) && flag;
			footerProgressRow.SetActive(p_showProgress);
			if (IsExpanded())
			{
				footerSpectateRow.SetActive(flag2);
				float num = ((!playersMatch) ? (flag2 ? footerButtonHeight : (2f * footerButtonHeight)) : (flag2 ? 0f : footerButtonHeight));
				float num2 = (playersMatch ? 0f : (0f - footerButtonHeight));
				float num3 = expandedSize - num + 1f;
				if (!(Mathf.Abs(height - num3) < 0.5f))
				{
					Tween.Add(this, "height", num3, 0.25f, 0f, Cubic.Out);
					expandedSpriteSize = (expandedSize - num) * rectToSpriteRatio;
					miniMapSpriteTran.localScale = new Vector3(miniMapSpriteTran.localScale.x, expandedSpriteSize, miniMapSpriteTran.localScale.z);
					pilotsExpandedRect.sizeDelta = new Vector2(0f, 0f - (footerButtonHeight * 2f + num2 + 1f) + pilotsExpandedRect.anchoredPosition.y);
				}
			}
			else
			{
				footerSpectateRow.SetActive(flag2);
				footerProgressRow.SetActive(!playersMatch && !flag2);
				if (!(Mathf.Abs(height - contractedSize) < 0.5f))
				{
					Tween.Add(this, "height", contractedSize, 0.25f, 0f, Cubic.Out);
					miniMapSpriteTran.localScale = new Vector3(miniMapSpriteTran.localScale.x, contractedSpriteSize, miniMapSpriteTran.localScale.z);
				}
			}
		}

		public void Contract()
		{
			Tween.Kill(this);
			isExpanded = false;
			if ((bool)bodyContracted)
			{
				bodyContracted.FadeIn(0.1f, 0.2f);
			}
			if ((bool)bodyExpanded)
			{
				bodyExpanded.FadeOut(0.1f, 0.2f);
			}
			if ((bool)plusMinusImage)
			{
				plusMinusImage.sprite = plusSprite;
			}
			UpdateMatchHeight();
		}

		private void ContractFooter()
		{
			footerSpectateRow.SetActive(value: false);
			footerProgressRow.SetActive(!playersMatch);
		}

		public void UpdatePilotsOrder()
		{
			int num = data.players.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				string text = data.playerIds[i];
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (!expandedSlots.ContainsKey(text))
				{
					DRLTournamentPlayerData dRLTournamentPlayerData = null;
					for (int j = 0; j < num; j++)
					{
						if (data.players[j].playerId == text)
						{
							dRLTournamentPlayerData = data.players[j];
							break;
						}
					}
					if (dRLTournamentPlayerData != null)
					{
						num2++;
						UITournamentBracketsPilotItemView uITournamentBracketsPilotItemView = expandedPilotsList.Push<UITournamentBracketsPilotItemView>();
						uITournamentBracketsPilotItemView.Set(dRLTournamentPlayerData);
						uITournamentBracketsPilotItemView.profileImage.material = UnityEngine.Object.Instantiate(uITournamentBracketsPilotItemView.profileImage.material);
						uITournamentBracketsPilotItemView.profileImage.material.SetFloat("_BlackAndWhite", 0f);
						if (!string.IsNullOrEmpty(text) && !expandedSlots.ContainsKey(text))
						{
							expandedSlots.Add(text, uITournamentBracketsPilotItemView.GetComponent<RectTransform>());
						}
					}
				}
				if (expandedSlots.ContainsKey(text))
				{
					RectTransform rectTransform = expandedSlots[text];
					if (rectTransform != null && Mathf.Abs(rectTransform.anchoredPosition.y - (expandedStartPos.y + (float)i * expandedDelta.y)) > 0.1f)
					{
						Tween.Add(rectTransform, "anchoredPosition", expandedStartPos + i * expandedDelta, 0.25f, 0f, Cubic.Out);
					}
				}
				if (num == 6 && contractedSlots.ContainsKey(text))
				{
					RectTransform rectTransform2 = contractedSlots[text];
					if (rectTransform2 != null && Mathf.Abs(rectTransform2.anchoredPosition.x - (contractedStartPos.x + (float)i * contractedDelta.x)) > 0.1f)
					{
						Tween.Add(rectTransform2, "anchoredPosition", contractedStartPos + i * contractedDelta, 0.25f, 0f, Cubic.Out);
					}
				}
			}
			if (num2 <= 0)
			{
				return;
			}
			List<UITournamentBracketsPilotItemView> list = expandedPilotsList.GetList<UITournamentBracketsPilotItemView>();
			for (int k = num; k < num + num2 && k < data.playerIds.Length; k++)
			{
				string text2 = data.playerIds[k];
				for (int l = 0; l < list.Count; l++)
				{
					if (list[l].data.playerId == text2)
					{
						expandedPilotsList.Remove(list[l], p_destroy: true);
					}
				}
			}
		}

		public void ShowWaitOverlay(string p_message = "")
		{
			if (p_message == string.Empty)
			{
				p_message = "WAIT";
			}
			waitOverlayText.text = p_message.ToUpper();
			waitOverlay.alpha = 1f;
			footerProgressRow.SetActive(value: false);
		}

		public void HideWaitOverlay()
		{
			waitOverlay.alpha = 0f;
		}

		public void HideHeatOverlay()
		{
			if ((bool)heatOverlay)
			{
				heatOverlay.gameObject.SetActive(value: false);
			}
		}
	}
}
