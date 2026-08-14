using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentBracketsView : UIScreenView
	{
		public enum AspectRatios
		{
			ar21x10 = 0,
			ar16x9 = 1,
			ar16x10 = 2,
			ar3x2 = 3,
			ar4x3 = 4,
			ar5x4 = 5
		}

		private Canvas m_canvas;

		public bool networkDebug;

		public bool UIDebug;

		private Vector2 startingBracketsPosition = new Vector2(1f, 0.2f);

		[Header("Header")]
		public Text headerTournamentLabel;

		public UITruncateText headerTournamentTrunc;

		public Text headerSpectatorsLabel;

		public Text headerParticipantsLabel;

		public Text headerNextRoundLabel;

		public UIElementView enterMatchButton;

		public UIElementView exitButton;

		public UIElementView backButton;

		public Text enterMatchButtonText;

		public GameObject enterMatchPropeller;

		public GameObject enterMatchArrow;

		public Text enterMatchCountdown;

		public RectTransform progressBar;

		public RectTransform progressBarFillRT;

		public GameObject exitTournamentEndButton;

		public GameObject winnersTournamentEndButton;

		public List<UINavigation> headerNavs;

		public GameObject placementsButton;

		[Header("Map")]
		public RawImage mapCardImage;

		public Text mapCardMapLabel;

		public Text mapCardTrackLabel;

		[Header("Drone")]
		public RawImage droneCardImage;

		public Text droneCardLabel;

		public UINavigation droneCardNav;

		public UIElementView droneCardButton;

		[Header("Body")]
		public UINavigationScroll navSc;

		public ListComponent headersListField;

		public RectTransform headersContainer;

		public RectTransform matchesContainer;

		public RectTransform scrollingContainer;

		public Vector2 scrollContainerInitAnchor;

		public ListComponent matchColumnsList;

		public Color headerColor1;

		public Color headerColor2;

		public UILineRenderer connectionLines;

		public FadeComponent contentFade;

		[Header("Minimap")]
		[SerializeField]
		private Camera m_miniMapCamera;

		public RectTransform minimapImage;

		public RectTransform miniMapScreen;

		public RectTransform miniMapScreenFrame;

		public float minimapImageWidth = 320f;

		public float minimapImageHeight = 238f;

		public float minimapMarginRatio;

		private bool m_minimapInit;

		private Vector3[] p_vc;

		private float m_marginY;

		private float m_marginX;

		[Header("Navigation")]
		public GameObject xboxNavigationTooltip;

		public GameObject psNavigationTooltip;

		private Vector2[][] m_pts;

		private Vector2[] m_accPts;

		private bool m_playerParticipates;

		[HideInInspector]
		public bool completeWinnersScreenShown;

		[HideInInspector]
		public bool forceIntoMatch;

		[HideInInspector]
		public string forceMatchID = "";

		public float autoJoinRoomPeriod = 10f;

		private float p_roomCountdown;

		public MonoActivity roomCountdownActivity;

		private bool m_autoJoined;

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

		public DRLTournamentData tournament { get; protected set; }

		public DRLTournamentRoundData activeRound { get; protected set; }

		public DRLTournamentMatchData activeMatch { get; protected set; }

		private int roundIndex
		{
			get
			{
				if (activeRound == null)
				{
					return -1;
				}
				return activeRound.index;
			}
		}

		private int matchIndex
		{
			get
			{
				if (activeMatch == null)
				{
					return -1;
				}
				return activeMatch.index;
			}
		}

		public UITournamentBracketsMatchItemView activeMatchItem { get; protected set; }

		public bool backButtonEnabled { get; set; }

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

		public void RefreshTournamentData(DRLTournamentData p_data, DRLTournamentRoundData p_activeRound, DRLTournamentMatchData p_activeMatch)
		{
			if (p_data == null)
			{
				return;
			}
			tournament = p_data;
			activeRound = p_activeRound;
			activeMatch = p_activeMatch;
			if (matchIndex >= 0 && roundIndex >= 0)
			{
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(roundIndex);
				if (!(uITournamentBracketsMatchColumnItem == null))
				{
					activeMatchItem = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(matchIndex);
				}
			}
		}

		public void RefreshActiveRoundInfo(int p_racerCount, int p_totalRacerCount)
		{
			headerParticipantsLabel.text = p_racerCount + " / " + p_totalRacerCount;
			headerSpectatorsLabel.text = "-";
		}

		public void UpdateMatchRoomData(Lobby.NetworkRoomInfo p_roomInfo, UITournamentBracketsMatchItemView p_match, bool p_joining = false, bool p_canSpectate = false)
		{
			bool p_inRace = p_roomInfo != null && (p_roomInfo.State == NetworkRoom.StateCode.GameRunning || p_roomInfo.State == NetworkRoom.StateCode.GameWarmup || p_roomInfo.State == NetworkRoom.StateCode.GameFinished || p_roomInfo.State == NetworkRoom.StateCode.GameLoading);
			int p_heatNumber = p_roomInfo?.HeatIdx ?? 0;
			int num = ((p_roomInfo != null && p_roomInfo.LobbyCountdownAllowed) ? p_roomInfo.LobbyCountdown : 0);
			if (p_match.roomCountdown == 0f || num == 0)
			{
				p_match.roomCountdown = num;
			}
			if (p_match.IsPlayerInMatch() && (roomCountdown == 0f || num == 0))
			{
				roomCountdown = num;
			}
			bool flag = !p_canSpectate;
			if (base.app.model.storage.state.player.profile.isDeveloper)
			{
				flag = false;
			}
			p_match.SetLobbyActions(p_roomInfo?.CanRace ?? true, p_roomInfo != null && !flag, p_inRace, p_heatNumber);
			p_match.PilotCount = ((p_roomInfo == null) ? "0" : p_roomInfo.RacersCount.ToString()) + "/" + p_match.data.players.Length;
			p_match.CurrentHeat = p_roomInfo?.HeatIdx ?? 0;
			if (p_match.data.state == TournamentMatchState.active && p_match.gameMode != TournamentRoundGameMode.leaderboard && !p_joining)
			{
				p_match.ColorRoomPlayers(p_roomInfo);
			}
		}

		public void UpdateLobbyButtons(bool p_canJoin)
		{
			NetworkModel network = base.app.model.network;
			if (network == null || matchColumnsList.Count == 0 || activeRound == null)
			{
				return;
			}
			UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(roundIndex);
			UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = ((matchIndex > -1) ? uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(matchIndex) : null);
			if (uITournamentBracketsMatchColumnItem.roundGameMode == TournamentRoundGameMode.leaderboard)
			{
				enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.solo-race", "START");
				if (uITournamentBracketsMatchColumnItem.data != null && uITournamentBracketsMatchColumnItem.data.state == TournamentRoundState.active && uITournamentBracketsMatchItemView != null)
				{
					bool flag = false;
					flag = p_canJoin;
					uITournamentBracketsMatchItemView.enterText.gameObject.SetActive(value: false);
					uITournamentBracketsMatchItemView.wideEnterText.gameObject.SetActive(value: true);
					if (uITournamentBracketsMatchItemView.wideEnterText.text == string.Empty)
					{
						uITournamentBracketsMatchItemView.wideEnterText.text = base.app.model.storage.locale.Get("vdrl.label.solo-race", "START");
					}
					uITournamentBracketsMatchItemView.enterButton.interactable = flag;
					enterMatchButton.interactable = flag;
					enterMatchArrow.gameObject.SetActive(value: true);
					enterMatchPropeller.gameObject.SetActive(value: false);
				}
				return;
			}
			for (int i = 0; i < uITournamentBracketsMatchColumnItem.matchesList.Count; i++)
			{
				UITournamentBracketsMatchItemView match = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(i);
				if (match.data == null)
				{
					Debug.Log("UITournamentBracketsView> match data is null");
					match.HideWaitOverlay();
					continue;
				}
				if (network.connectionState != PhotonService.ServiceState.InLobby)
				{
					enterMatchButton.interactable = false;
					enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN");
					enterMatchPropeller.gameObject.SetActive(value: false);
					enterMatchArrow.gameObject.SetActive(value: true);
					UpdateMatchRoomData(null, match, p_joining: true);
					match.SetLobbyActions(p_canJoin: false, p_canSpectate: false);
					if (matchIndex == i)
					{
						match.ShowWaitOverlay("CONNECTING...");
					}
					enterMatchCountdown.gameObject.SetActive(value: false);
					StopRoomCountdownActivity();
					continue;
				}
				match.HideWaitOverlay();
				Lobby.NetworkRoomInfo networkRoomInfo = network.lobby.Rooms.Find((Lobby.NetworkRoomInfo r) => r.MatchId == match.data.Id);
				UpdateMatchRoomData(networkRoomInfo, match, p_joining: false, base.app.model.tournament.CanSpectate());
				bool flag2 = base.app.model.tournament.GetTournamentProgressionType() == TournamentProgression.manual;
				if (matchIndex == i)
				{
					match.enterText.text = base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN");
					match.enterText.gameObject.SetActive(value: true);
					match.wideEnterText.gameObject.SetActive(value: false);
					enterMatchButton.interactable = (networkRoomInfo?.CanRace ?? true) && p_canJoin;
					enterMatchButtonText.text = ((!enterMatchButton.interactable) ? base.app.model.storage.locale.Get("vdrl.label.in-game", "IN GAME") : base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN"));
					if (networkRoomInfo != null && networkRoomInfo.State == NetworkRoom.StateCode.MatchMaking)
					{
						enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN") + "     ";
						enterMatchCountdown.gameObject.SetActive(!flag2);
						enterMatchPropeller.gameObject.SetActive(value: false);
						enterMatchArrow.gameObject.SetActive(flag2);
					}
					else if (networkRoomInfo == null)
					{
						enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.mp-race", "JOIN");
						enterMatchArrow.gameObject.SetActive(value: true);
						enterMatchPropeller.gameObject.SetActive(value: false);
						enterMatchCountdown.gameObject.SetActive(value: false);
						StopRoomCountdownActivity();
					}
					else
					{
						StopRoomCountdownActivity();
						enterMatchCountdown.gameObject.SetActive(value: false);
						enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.heat", "HEAT") + " " + networkRoomInfo.HeatIdx + "  ";
						enterMatchPropeller.gameObject.SetActive(value: true);
						enterMatchArrow.gameObject.SetActive(value: false);
					}
				}
			}
		}

		public void UpdateHeaderStatus(bool p_isRacer, float p_timer)
		{
			if (tournament == null)
			{
				return;
			}
			DRLTournamentData dRLTournamentData = tournament;
			DRLTournamentMatchData dRLTournamentMatchData = activeMatch;
			DRLTournamentRoundData dRLTournamentRoundData = activeRound;
			if (dRLTournamentRoundData == null || (dRLTournamentMatchData == null && p_isRacer))
			{
				return;
			}
			if (dRLTournamentMatchData == null)
			{
				if (dRLTournamentRoundData == null || dRLTournamentRoundData.matches.Length == 0)
				{
					headerNextRoundLabel.text = "WAITING FOR MATCH TO START";
					return;
				}
				dRLTournamentMatchData = dRLTournamentRoundData.matches[0];
			}
			if (dRLTournamentMatchData == null)
			{
				headerNextRoundLabel.text = "WAITING FOR MATCH TO START";
				return;
			}
			if (dRLTournamentData.progression == TournamentProgression.auto || (dRLTournamentRoundData.gameMode == TournamentRoundGameMode.leaderboard && dRLTournamentMatchData.state == TournamentMatchState.active))
			{
				UpdateNextRoundTimer(p_timer);
				return;
			}
			switch (dRLTournamentMatchData.state)
			{
			case TournamentMatchState.active:
				headerNextRoundLabel.text = "MATCH ACTIVE - PLEASE JOIN";
				break;
			case TournamentMatchState.idle:
			case TournamentMatchState.waiting:
				headerNextRoundLabel.text = "WAITING FOR MATCH TO START";
				break;
			}
		}

		public void UpdateNextRoundTimer(float p_time)
		{
			if (tournament == null)
			{
				return;
			}
			exitTournamentEndButton.SetActive(value: false);
			winnersTournamentEndButton.SetActive(value: false);
			if (p_time >= 0f && activeRound != null && activeRound.state == TournamentRoundState.active)
			{
				int num = (int)Math.Floor(p_time / 60f);
				int num2 = (int)Math.Floor(p_time - (float)(num * 60));
				string text = num.ToString("00") + ":" + num2.ToString("00");
				string title = activeRound.title;
				headerNextRoundLabel.text = ((title.Length > 20) ? new StringInfo().SubstringByTextElements(0, 20) : title) + " " + base.app.model.storage.locale.Get("vdrl.label.ends-in", "ENDS IN") + " " + text;
				UITournamentBracketsHeaderItem uITournamentBracketsHeaderItem = headersListField.Get<UITournamentBracketsHeaderItem>(activeRound.index);
				if (uITournamentBracketsHeaderItem != null)
				{
					text = ((p_time == 0f) ? "" : text);
					uITournamentBracketsHeaderItem.SetCountdown(text);
				}
				return;
			}
			Localization locale = base.app.model.storage.locale;
			switch (tournament.status)
			{
			case TournamentState.idle:
				headerNextRoundLabel.text = locale.Get("vdrl.label.waiting-players", "WAITING FOR PLAYERS") + "...";
				break;
			case TournamentState.canceled:
				headerNextRoundLabel.text = locale.Get("vdrl.label.event-canceled", "EVENT CANCELED BY THE HOST");
				winnersTournamentEndButton.SetActive(value: false);
				break;
			case TournamentState.complete:
				headerNextRoundLabel.text = locale.Get("vdrl.label.ended", "TOURNAMENT ENDED");
				winnersTournamentEndButton.SetActive(value: true);
				break;
			case TournamentState.fail:
				headerNextRoundLabel.text = locale.Get("vdrl.label.tournament-failed", "TOURNAMENT FAILED");
				winnersTournamentEndButton.SetActive(value: false);
				break;
			default:
				headerNextRoundLabel.text = locale.Get("vdrl.label.tournament-update", "UPDATING STATUS") + "...";
				winnersTournamentEndButton.SetActive(value: false);
				break;
			}
		}

		public void UpdateGeneralTournamentUI()
		{
			if (tournament != null && (tournament.status == TournamentState.complete || tournament.status == TournamentState.fail))
			{
				enterMatchButton.gameObject.SetActive(value: false);
				StopRoomCountdownActivity();
			}
			else if (tournament != null && tournament.status == TournamentState.active)
			{
				enterMatchButton.gameObject.SetActive(value: true);
			}
			else
			{
				enterMatchButton.gameObject.SetActive(value: false);
				StopRoomCountdownActivity();
			}
		}

		public void SetMapCard(DRLTournamentRoundData p_data)
		{
			if (p_data == null)
			{
				return;
			}
			if (p_data.isCustomMap)
			{
				DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(p_data.mapId);
				if ((bool)dRLMap)
				{
					mapCardImage.texture = dRLMap.preview;
					mapCardMapLabel.text = dRLMap.label;
					if (!string.IsNullOrEmpty(p_data.customMapTitle))
					{
						mapCardTrackLabel.text = p_data.customMapTitle.ToUpper();
					}
					else
					{
						mapCardTrackLabel.text = "";
					}
				}
			}
			else
			{
				DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(p_data.trackId);
				if ((bool)dRLMapTrack)
				{
					mapCardImage.texture = dRLMapTrack.map.preview;
					mapCardMapLabel.text = dRLMapTrack.map.label;
					mapCardTrackLabel.text = dRLMapTrack.label;
				}
			}
			UITruncateText component = mapCardTrackLabel.GetComponent<UITruncateText>();
			if (component != null)
			{
				component.Refresh();
			}
		}

		public void SetDroneCard(bool p_canRace)
		{
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			droneCardLabel.text = currentRigData.name.ToUpper();
			base.app.model.storage.state.player.garage.GetRigThumbnail(currentRigData, 320, 0, delegate(Texture2D p_result)
			{
				if (p_result != null && p_result.width > 128)
				{
					droneCardImage.texture = p_result;
				}
			});
			UIElementView uIElementView = droneCardButton;
			bool flag = (droneCardNav.enabled = tournament.droneClass != 1 && p_canRace);
			uIElementView.enabled = flag;
			if ((bool)base.app.view.ui.footer)
			{
				base.app.view.ui.footer.droneButton.interactable = tournament.droneClass != 1 && p_canRace;
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
			m_autoJoined = false;
			roomCountdownActivity = Run((Func<bool>)delegate
			{
				p_roomCountdown -= Time.deltaTime;
				enterMatchCountdown.text = p_roomCountdown.ToString("N0");
				if (p_roomCountdown <= autoJoinRoomPeriod && !m_autoJoined)
				{
					m_autoJoined = true;
				}
				return !(p_roomCountdown < 0f);
			}, 0f, false);
		}

		public void StopRoomCountdownActivity()
		{
			if (roomCountdownActivity != null)
			{
				roomCountdownActivity.Stop();
				roomCountdownActivity = null;
				p_roomCountdown = 0f;
				enterMatchCountdown.text = string.Empty;
				enterMatchCountdown.gameObject.SetActive(value: false);
				enterMatchArrow.gameObject.SetActive(value: true);
			}
		}

		public void EnableMiniMap(bool inGame)
		{
			m_miniMapCamera.enabled = true;
			float aspect = base.app.view.ui.canvas.worldCamera.aspect;
			if (aspect >= 2.05f)
			{
				SetARForMiniMap(AspectRatios.ar21x10, inGame);
			}
			else if (aspect >= 1.7f)
			{
				SetARForMiniMap(AspectRatios.ar16x9, inGame);
			}
			else if (aspect >= 1.59f)
			{
				SetARForMiniMap(AspectRatios.ar16x10, inGame);
			}
			else if (aspect >= 1.489f)
			{
				SetARForMiniMap(AspectRatios.ar3x2, inGame);
			}
			else if (aspect >= 1.32f)
			{
				SetARForMiniMap(AspectRatios.ar4x3, inGame);
			}
			else if (aspect >= 1.23f)
			{
				SetARForMiniMap(AspectRatios.ar5x4, inGame);
			}
			else
			{
				SetARForMiniMap(AspectRatios.ar16x9, inGame);
			}
		}

		public void SetARForMiniMap(AspectRatios ar, bool p_inGame)
		{
			_ = m_miniMapCamera.transform.localPosition;
			_ = m_miniMapCamera.orthographicSize;
			this.TimerRunOnce(delegate
			{
				Vector3[] array = new Vector3[4];
				Vector3[] array2 = new Vector3[4];
				RectTransform rectTransform = matchesContainer;
				RectTransform viewrect = base.scroll.viewrect;
				rectTransform.GetWorldCorners(array);
				float num = minimapImageWidth;
				float num2 = minimapImageHeight;
				float num3 = minimapMarginRatio;
				float num4 = num / num2;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				float num8 = viewrect.sizeDelta.x / viewrect.sizeDelta.y;
				float num9 = array[2].y - array[3].y;
				float num10 = array[2].x - array[1].x;
				bool flag = num10 > num9;
				base.scroll.viewrect.GetWorldCorners(array2);
				float num11 = Mathf.Abs(array2[2].y - array2[3].y);
				float num12 = Mathf.Abs(array2[1].x - array2[2].x);
				Vector2 vector;
				if ((num9 < num11 && num10 < num12) || num9 < num12)
				{
					vector = new Vector2(array[1].x + num10 / 2f, array[2].y - num9 / 2f);
					m_marginY = num11 * num4 * num3;
					m_marginX = num12 * num3;
					num5 = (num12 + m_marginX) / (2f * m_miniMapCamera.aspect);
					num6 = viewrect.sizeDelta.x / (viewrect.sizeDelta.x * (1f + num3));
					num7 = num6 / num8;
				}
				else
				{
					vector = new Vector2(array[1].x + num10 / 2f, array[2].y - num9 / 2f);
					m_marginY = num9 * num4 * num3;
					m_marginX = num10 * num3;
					if (flag)
					{
						num5 = (num10 + m_marginX) / (2f * m_miniMapCamera.aspect);
						num6 = viewrect.sizeDelta.x / (rectTransform.sizeDelta.x * (1f + num3));
						num7 = num6 / num8;
					}
					else
					{
						num5 = (num9 + m_marginY) / 2f;
						num7 = viewrect.sizeDelta.y / (rectTransform.sizeDelta.y * (1f + num4 * num3));
						num6 = num7 * num8;
					}
				}
				m_miniMapCamera.orthographicSize = num5;
				m_miniMapCamera.transform.position = new Vector3(vector.x, vector.y, m_miniMapCamera.transform.position.z);
				miniMapScreen.sizeDelta = new Vector2(num6 * num, num7 * num2);
				miniMapScreenFrame.sizeDelta = new Vector2(num6 * num, num7 * num2);
				m_minimapInit = true;
				p_vc = new Vector3[4];
				UpdateMMFramePosition();
			}, 0.5f);
		}

		private void UpdateMMFramePosition()
		{
			if (m_minimapInit)
			{
				base.scroll.viewrect.GetWorldCorners(p_vc);
				float num = Mathf.Abs(p_vc[2].y - p_vc[3].y);
				float num2 = Mathf.Abs(p_vc[1].x - p_vc[2].x);
				Vector2 vector = new Vector2(p_vc[1].x + num2 / 2f, p_vc[2].y - num / 2f);
				Vector3 position = m_miniMapCamera.WorldToScreenPoint(vector);
				Vector3 vector2 = m_miniMapCamera.ScreenToViewportPoint(position);
				miniMapScreen.anchoredPosition = new Vector2(vector2.x * minimapImageWidth, (0f - (1f - vector2.y)) * minimapImageHeight);
				miniMapScreenFrame.anchoredPosition = miniMapScreen.anchoredPosition;
			}
		}

		public void DisableMiniMap()
		{
			m_miniMapCamera.enabled = false;
		}

		public void Populate(bool p_layoutChanged = false)
		{
			RefreshNavigationTooltips();
			if (tournament == null)
			{
				Debug.LogError("UITournamentBracketsView> Tournament data is null");
				return;
			}
			NetworkModel network = base.app.model.network;
			bool isRacer = base.app.model.tournament.isRacer;
			contentFade.Fade(0f, 0f);
			if (base.app.arguments.tournament == null)
			{
				base.app.arguments.tournament = new DRLAppArguments.Tournament();
			}
			base.app.arguments.tournament.data = tournament;
			string playerId = base.app.model.storage.state.player.profile.playerId;
			contentFade.Fade(0f, 0f);
			headersListField.Clear();
			matchColumnsList.Clear();
			StopRoomCountdownActivity();
			enterMatchButton.interactable = false;
			connectionLines.Points = null;
			connectionLines.enabled = false;
			DRLTournamentRoundData[] rounds = tournament.rounds;
			int num = rounds.Length;
			int[] array = new int[num];
			int[] array2 = new int[num];
			int[] array3 = new int[num];
			if (rounds.Length == 0)
			{
				return;
			}
			for (int i = 0; i < rounds.Length; i++)
			{
				if (num >= 0)
				{
					array[i] = rounds[i].matches.Length;
					array2[i] = ((array[i] > 0) ? ((rounds[i].gameMode != TournamentRoundGameMode.leaderboard) ? rounds[i].matches[0].heatCount : (-1)) : 0);
					array3[i] = ((array[i] > 0) ? rounds[i].matches[0].maxWinners : 0);
				}
			}
			SetMapCard(tournament.GetActiveRound());
			SetDroneCard(isRacer);
			if (roundIndex > -1 && rounds[roundIndex].gameMode == TournamentRoundGameMode.leaderboard)
			{
				enterMatchButtonText.text = base.app.model.storage.locale.Get("vdrl.label.solo-race", "START");
			}
			headerTournamentLabel.text = tournament.title.ToUpper();
			headerTournamentTrunc.Refresh();
			UpdateHeaderStatus(isRacer, 0f);
			AllocatePoints(num, array);
			bool flag = num >= 3;
			UINavigationScroll component = GetComponent<UINavigationScroll>();
			if ((bool)component)
			{
				component.containerSizeCalculationTargets = new RectTransform[num + 1];
				component.containerSizeCalculationTargets[num] = headersContainer;
				component.disableFocusing = true;
			}
			for (int j = 0; j < num; j++)
			{
				UITournamentBracketsHeaderItem uITournamentBracketsHeaderItem = headersListField.Push<UITournamentBracketsHeaderItem>();
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = matchColumnsList.Push<UITournamentBracketsMatchColumnItem>();
				uITournamentBracketsMatchColumnItem.data = rounds[j];
				uITournamentBracketsHeaderItem.Set(rounds[j].title, "", uITournamentBracketsMatchColumnItem.data.totalPlayerCount, array2[j], array3[j], 0.1f);
				uITournamentBracketsHeaderItem.UpdateBodyImage(j == 0, flag && j > 0 && j < num - 1, j == num - 1);
				uITournamentBracketsHeaderItem.SetColor(Color.Lerp(t: (num > 1) ? ((float)j / (float)(num - 1)) : 1f, a: headerColor1, b: headerColor2));
				if (!uITournamentBracketsMatchColumnItem)
				{
					continue;
				}
				if ((bool)component)
				{
					component.containerSizeCalculationTargets[j] = uITournamentBracketsMatchColumnItem.GetComponent<RectTransform>();
				}
				uITournamentBracketsMatchColumnItem.matchesList.Clear();
				int num2 = rounds[j].matches.Length;
				int playerMatchIndex = rounds[j].GetPlayerMatchIndex(playerId);
				for (int k = 0; k < num2; k++)
				{
					DRLTournamentMatchData dRLTournamentMatchData = rounds[j].matches[k];
					UITournamentBracketsMatchItemView matchItem = uITournamentBracketsMatchColumnItem.matchesList.Push<UITournamentBracketsMatchItemView>();
					bool flag2 = playerMatchIndex == k;
					bool flag3 = dRLTournamentMatchData.state == TournamentMatchState.active;
					matchItem.gameMode = rounds[j].gameMode;
					matchItem.roundTitle = rounds[j].title;
					matchItem.Contract();
					bool p_showGroupName = rounds[j].matches.Length > 1;
					matchItem.Set(dRLTournamentMatchData, flag2 && flag3, p_init: true, p_showGroupName);
					uITournamentBracketsMatchColumnItem.roundGameMode = matchItem.gameMode;
					NetworkModel network2 = base.app.model.network;
					switch (matchItem.data.state)
					{
					case TournamentMatchState.active:
					{
						if (network2.connectionState != PhotonService.ServiceState.InLobby)
						{
							if (matchIndex == k)
							{
								matchItem.ShowWaitOverlay("CONNECTING...");
							}
							matchItem.SetLobbyActions(p_canJoin: false, p_canSpectate: false);
							break;
						}
						matchItem.HideWaitOverlay();
						Lobby.NetworkRoomInfo networkRoomInfo = network.lobby.Rooms.Find((Lobby.NetworkRoomInfo r) => r.MatchId == matchItem.data.Id);
						UpdateMatchRoomData(networkRoomInfo, matchItem, p_joining: false, base.app.model.tournament.CanSpectate());
						if (matchIndex == k)
						{
							enterMatchButton.interactable = (networkRoomInfo == null || networkRoomInfo.CanRace) && base.app.model.tournament.CanJoin();
						}
						break;
					}
					case TournamentMatchState.idle:
						matchItem.ShowWaitOverlay("WAITING...");
						break;
					case TournamentMatchState.complete:
						matchItem.ColorWinners();
						matchItem.HideWaitOverlay();
						matchItem.HideHeatOverlay();
						break;
					case TournamentMatchState.fail:
						matchItem.HideWaitOverlay();
						matchItem.HideHeatOverlay();
						break;
					}
				}
			}
			placementsButton.SetActive(tournament.isDAWC);
			((Component)this).TimerRun((Action)delegate
			{
				if ((bool)headersContainer)
				{
					LayoutElement component2 = headersContainer.GetComponent<LayoutElement>();
					if ((bool)component2)
					{
						component2.ignoreLayout = true;
					}
				}
				RefreshLayout(0.3f, 0f, 0.2f, p_init: true);
				if (p_layoutChanged && base.scroll != null && base.scroll.container != null)
				{
					RectTransform component3 = base.scroll.container.GetComponent<RectTransform>();
					if (component3 != null)
					{
						component3.anchoredPosition = new Vector2(0f, 0f);
					}
				}
				contentFade.Fade(1f, 0.3f);
			}, 0f, 0.5f);
			RefreshUINavigation();
		}

		private void RefreshUINavigation()
		{
			List<UITournamentBracketsMatchColumnItem> list = matchColumnsList.GetList<UITournamentBracketsMatchColumnItem>();
			for (int i = 0; i < list.Count; i++)
			{
				List<UITournamentBracketsMatchItemView> list2 = list[i].matchesList.GetList<UITournamentBracketsMatchItemView>();
				for (int j = 0; j < list2.Count; j++)
				{
					UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = list2[j];
					if (i == 0 && j == 0)
					{
						foreach (UINavigation headerNav in headerNavs)
						{
							headerNav.down = uITournamentBracketsMatchItemView.navigation;
						}
						UINavigation.Focus(uITournamentBracketsMatchItemView.navigation);
					}
					uITournamentBracketsMatchItemView.navigation.up = ((j == 0) ? headerNavs[0] : list2[j - 1].navigation);
					if (j < list2.Count - 1)
					{
						uITournamentBracketsMatchItemView.resultsButton.down = list2[j + 1].navigation;
					}
					if (i > 0 && list[i - 1].matchesList.Count > 0)
					{
						if (list[i - 1].matchesList.Count - 1 < j)
						{
							UINavigation navigation = list[i - 1].matchesList.Get<UITournamentBracketsMatchItemView>(list[i - 1].matchesList.Count - 1).navigation;
							uITournamentBracketsMatchItemView.navigation.left = navigation;
							uITournamentBracketsMatchItemView.resultsButton.left = navigation;
						}
						else
						{
							UINavigation navigation2 = list[i - 1].matchesList.Get<UITournamentBracketsMatchItemView>(j).navigation;
							uITournamentBracketsMatchItemView.navigation.left = navigation2;
							uITournamentBracketsMatchItemView.resultsButton.left = navigation2;
						}
					}
					if (i < list.Count - 1 && list[i + 1].matchesList.Count > 0)
					{
						if (list[i + 1].matchesList.Count - 1 < j)
						{
							UINavigation navigation3 = list[i + 1].matchesList.Get<UITournamentBracketsMatchItemView>(list[i + 1].matchesList.Count - 1).navigation;
							uITournamentBracketsMatchItemView.navigation.right = navigation3;
							uITournamentBracketsMatchItemView.resultsButton.right = navigation3;
						}
						else
						{
							UINavigation navigation4 = list[i + 1].matchesList.Get<UITournamentBracketsMatchItemView>(j).navigation;
							uITournamentBracketsMatchItemView.navigation.right = navigation4;
							uITournamentBracketsMatchItemView.resultsButton.right = navigation4;
						}
					}
					if (i == list.Count - 1)
					{
						uITournamentBracketsMatchItemView.navigation.right = droneCardNav;
						droneCardNav.left = uITournamentBracketsMatchItemView.navigation;
					}
				}
			}
		}

		public void RefreshLayout(float p_duration, float p_diff, float p_delay = 0f, bool p_init = false)
		{
			((Component)this).TimerRun((Action)delegate
			{
				DrawLines();
			}, p_duration + p_delay, 0f);
		}

		private void AllocatePoints(int p_maxLevels, int[] p_matchesCount)
		{
			m_pts = new Vector2[p_maxLevels][];
			int num = 0;
			for (int i = 0; i < p_maxLevels; i++)
			{
				int num2 = 1;
				int num3;
				if (i == 0)
				{
					num3 = 1;
					num2 = 1;
				}
				else if (i == p_maxLevels - 1)
				{
					num3 = 1;
					num2 = ((p_maxLevels == 2) ? 1 : 0);
				}
				else
				{
					num3 = 2;
					num2 = 2;
				}
				m_pts[i] = new Vector2[p_matchesCount[i] * 2 * num3 + num2 * 2];
				num += p_matchesCount[i] * 2 * num3 + num2 * 2;
			}
			m_accPts = new Vector2[num];
		}

		private void DrawLines()
		{
			if (matchColumnsList.Count == 1)
			{
				return;
			}
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i < matchColumnsList.Count; i++)
			{
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(i);
				int num = ((i == 0) ? 1 : ((i == matchColumnsList.Count - 1) ? 1 : 2));
				int num2 = 0;
				if (m_pts.Length != 0)
				{
					connectionLines.enabled = true;
				}
				for (int j = 0; j < uITournamentBracketsMatchColumnItem.matchesList.Count; j++)
				{
					UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(j);
					for (int k = 0; k < num; k++)
					{
						int num3 = j * 2 * num;
						RectTransform rectTransform = uITournamentBracketsMatchItemView.mpR;
						float num4 = uITournamentBracketsMatchColumnItem.rightLineWidth;
						float num5 = 1f;
						if (k == 1 || i == matchColumnsList.Count - 1)
						{
							rectTransform = uITournamentBracketsMatchItemView.mpL;
							num5 = -1f;
							num4 = uITournamentBracketsMatchColumnItem.leftLineWidth;
						}
						RectTransformUtility.ScreenPointToLocalPointInRectangle(connectionLines.rectTransform, rectTransform.position, null, out m_pts[i][num3 + k * 2]);
						m_pts[i][num3 + 1 + k * 2].x = m_pts[i][num3 + k * 2].x + num5 * num4;
						m_pts[i][num3 + 1 + k * 2].y = m_pts[i][num3 + k * 2].y;
						num2 = num3 + 1 + k * 2;
					}
				}
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem2 = matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(i - 1);
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem3 = matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(i + 1);
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView2 = ((uITournamentBracketsMatchColumnItem2 == null) ? null : uITournamentBracketsMatchColumnItem2.matchesList.Get<UITournamentBracketsMatchItemView>(0));
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView3 = ((uITournamentBracketsMatchColumnItem3 == null) ? null : uITournamentBracketsMatchColumnItem3.matchesList.Get<UITournamentBracketsMatchItemView>(0));
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView4 = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(0);
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView5 = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(uITournamentBracketsMatchColumnItem.matchesList.Count - 1);
				if (matchColumnsList.Count == 2 || i != matchColumnsList.Count - 1)
				{
					if (uITournamentBracketsMatchItemView4 == null)
					{
						Debug.LogWarning("UITournamentsBracketsView> matchFirst is null");
						return;
					}
					if (uITournamentBracketsMatchItemView5 == null)
					{
						Debug.LogWarning("UITournamentsBracketsView> matchLast is null");
						return;
					}
					int num6 = num2;
					RectTransform mpR = uITournamentBracketsMatchItemView4.mpR;
					RectTransform rectTransform2 = null;
					if ((matchColumnsList.Count != 2 || i != 1) && uITournamentBracketsMatchItemView3 != null)
					{
						rectTransform2 = ((uITournamentBracketsMatchColumnItem.matchesList.Count != 1) ? uITournamentBracketsMatchItemView5.mpR : uITournamentBracketsMatchItemView3.mpL);
						RectTransformUtility.ScreenPointToLocalPointInRectangle(connectionLines.rectTransform, mpR.position, null, out m_pts[i][num6 + 1]);
						RectTransformUtility.ScreenPointToLocalPointInRectangle(connectionLines.rectTransform, rectTransform2.position, null, out m_pts[i][num6 + 2]);
						m_pts[i][++num6].x += uITournamentBracketsMatchColumnItem.rightLineWidth;
						m_pts[i][++num6].x += ((uITournamentBracketsMatchColumnItem.matchesList.Count != 1) ? uITournamentBracketsMatchColumnItem.rightLineWidth : (0f - uITournamentBracketsMatchColumnItem.leftLineWidth));
					}
					if (i != 0)
					{
						if (uITournamentBracketsMatchItemView4 == null)
						{
							Debug.LogWarning("UITournamentsBracketsView> matchFirst is null!");
							return;
						}
						mpR = uITournamentBracketsMatchItemView4.mpL;
						rectTransform2 = ((uITournamentBracketsMatchColumnItem.matchesList.Count != 1) ? uITournamentBracketsMatchItemView5.mpL : uITournamentBracketsMatchItemView2.mpR);
						RectTransformUtility.ScreenPointToLocalPointInRectangle(connectionLines.rectTransform, mpR.position, null, out m_pts[i][num6 + 1]);
						RectTransformUtility.ScreenPointToLocalPointInRectangle(connectionLines.rectTransform, rectTransform2.position, null, out m_pts[i][num6 + 2]);
						m_pts[i][++num6].x -= uITournamentBracketsMatchColumnItem.leftLineWidth;
						m_pts[i][++num6].x += ((uITournamentBracketsMatchColumnItem.matchesList.Count != 1) ? (0f - uITournamentBracketsMatchColumnItem.leftLineWidth) : uITournamentBracketsMatchColumnItem.rightLineWidth);
					}
				}
				list.AddRange(m_pts[i]);
			}
			m_accPts = list.ToArray();
			connectionLines.Points = m_accPts;
		}

		private void Update()
		{
			UpdateMMFramePosition();
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool active = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool active2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			xboxNavigationTooltip.SetActive(active);
			psNavigationTooltip.SetActive(active2);
		}
	}
}
