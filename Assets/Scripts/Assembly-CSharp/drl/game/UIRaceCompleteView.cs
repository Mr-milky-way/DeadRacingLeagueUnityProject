using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIRaceCompleteView : UIScreenView
	{
		[Header("Screen")]
		public FadeComponent nextButtonFade;

		public UINavigation nextButtonNav;

		public UIElementView nextButton;

		public Text nextButtonField;

		public UINavigation restartButtonFade;

		public UINavigation restartButtonNav;

		public UINavigation shareButtonNav;

		public FadeComponent spectateButtonFade;

		public UINavigation spectateButtonNav;

		public DRLStandingsView standings;

		public UIVoteTrackView votes;

		public UINavigation exitButtonNav;

		public UINavigation tournamentRestartButton;

		public RaceController race;

		public GameCollectableController collectable;

		public RenderTexture backgroundCapture;

		public RawImage backgroundField;

		[Header("Header")]
		public LayoutElement leftSpaceLayout;

		public LayoutElement timeContainer;

		public Text timeField;

		public LayoutElement leaderboardContainer;

		public FadeComponent leaderboardFade;

		public Text leaderboardField;

		public LayoutElement leaderboardCircuitContainer;

		public Text leaderboardCircuitField;

		public GameObject promo;

		public UIElementView droneRatingCard;

		public UIElementView mapRatingCard;

		public DRLStepperView droneRating;

		public DRLStepperView mapRating;

		public FadeComponent[] droneRatingStarFades;

		public FadeComponent[] mapRatingStarFades;

		[Header("Header.Profile")]
		public ImageClip progressionRankField;

		public ImageClip progressionLevelRangeField;

		public RectTransform progressionContainer;

		public RawImage profilePhotoField;

		public LayoutElement profileNameContainer;

		public Text profileNameField;

		public Text progressionLevelField;

		public Text progressionXPField;

		public RectTransform progressionXPProgressBar;

		public RectTransform progressionStreakContainer;

		public Text progressionStreakField;

		public RectTransform progressionStreakProgressBar;

		[Header("Race Analytics")]
		public Text playerTimeField;

		public Text playerCrashField;

		public Text playerTopSpeedField;

		public Text playerTimeInFirstField;

		public Text playerPercentileField;

		public Text playerDistanceField;

		public Image timelineField;

		public ListComponent listField;

		public List<LayoutElement> raceAnalyticsContainers;

		public List<FadeComponent> raceAnalyticsFades;

		[Header("Testing")]
		public FadeComponent testFade;

		public LayoutElement testLayoutElement;

		public bool showStandings;

		public bool nextEnabled;

		public bool replayUploadStarted;

		private float m_header_timer;

		private int m_header_leaderboard;

		private string m_header_leaderboard_name = "LEADERBOARD";

		private MonoActivity m_headername_delay;

		public bool willSetLeaderboard;

		[HideInInspector]
		public bool willUpdateCircuits;

		public DRLToggleView toggleView;

		private bool _saveComplete;

		private int m_progression_rank;

		private int m_xp_current;

		private int m_xp_prev;

		private int m_xp_next;

		private int m_streak;

		private int m_streak_current;

		private int m_streak_goal;

		private Activity m_progression_loop;

		public GameTypeController gameMode
		{
			get
			{
				if (!race)
				{
					if (!collectable)
					{
						return null;
					}
					return collectable;
				}
				return race;
			}
		}

		public StorageModel storage => base.app.model.storage;

		public float playerTime
		{
			set
			{
				playerTimeField.text = Format.SecondsToMMSSFFF(value);
			}
		}

		public int playerCrash
		{
			set
			{
				Text text = playerCrashField;
				string text4;
				if (value > 99)
				{
					string text2 = (playerCrashField.text = "99+");
					text4 = text2;
				}
				else
				{
					string text2 = (playerCrashField.text = " " + value);
					text4 = text2;
				}
				text.text = text4;
			}
		}

		public float playerTopSpeed
		{
			set
			{
				playerTopSpeedField.text = value.ToString("0") + " KPH";
			}
		}

		public float playerTimeInFirst
		{
			set
			{
				playerTimeInFirstField.text = Format.SecondsToMMSSFFF(value);
			}
		}

		public float playerPercentile
		{
			set
			{
				Text text = playerPercentileField;
				string text4;
				if (value != 0f)
				{
					string text2 = (playerPercentileField.text = value.ToString("0") + "%");
					text4 = text2;
				}
				else
				{
					string text2 = (playerPercentileField.text = "--");
					text4 = text2;
				}
				text.text = text4;
			}
		}

		public float playerDistance
		{
			set
			{
				playerDistanceField.text = ((value > 1000f) ? ((value / 1000f).ToString("0.0") + "KM / ") : (value.ToString("0") + "M / "));
			}
		}

		public string headerName
		{
			set
			{
				profileNameField.text = value;
			}
		}

		public float headerTime
		{
			get
			{
				return m_header_timer;
			}
			set
			{
				m_header_timer = value;
				string text = Format.SecondsToMMSSFFF(m_header_timer);
				timeField.text = base.app.model.storage.locale.Get("leaderboard.result.race-time", "RACE TIME") + "\n" + text;
			}
		}

		public int headerLeaderboard
		{
			get
			{
				return m_header_leaderboard;
			}
			set
			{
				m_header_leaderboard = value;
				leaderboardField.text = ((m_header_leaderboard == 1) ? base.app.model.storage.locale.Get("leaderboard.result.new-record", "NEW RECORD") : base.app.model.storage.locale.Get("leaderboard.result.personal-best", "NEW PERSONAL BEST")) + "\n#" + m_header_leaderboard + " " + base.app.model.storage.locale.Get("leaderboard.result.on", "ON") + " " + m_header_leaderboard_name;
			}
		}

		public string headerLeaderboardName
		{
			get
			{
				return m_header_leaderboard_name;
			}
			set
			{
				m_header_leaderboard_name = value;
				headerLeaderboard = m_header_leaderboard;
			}
		}

		public Texture headerPhoto
		{
			set
			{
				profilePhotoField.texture = value;
				profilePhotoField.enabled = value != null;
			}
		}

		public bool saveComplete
		{
			get
			{
				if (!_saveComplete)
				{
					return storage.saveComplete;
				}
				return true;
			}
			set
			{
				_saveComplete = value;
			}
		}

		protected void Awake()
		{
			SetHeaderVisible(p_flag: false);
			headerName = "";
			headerTime = 0f;
			headerLeaderboard = -1;
		}

		public void Clear()
		{
			listField.Clear();
		}

		public void SetPromoEnabled(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
		}

		public void SetProgressionEnabled(bool p_flag)
		{
			profileNameField.alignment = (p_flag ? TextAnchor.LowerLeft : TextAnchor.MiddleLeft);
			Vector2 anchoredPosition = profileNameField.rectTransform.anchoredPosition;
			anchoredPosition.x = (p_flag ? 97f : 0f);
			profileNameField.rectTransform.anchoredPosition = anchoredPosition;
			progressionContainer.gameObject.SetActive(p_flag);
			progressionLevelRangeField.gameObject.SetActive(p_flag);
		}

		public void SetHeaderVisible(bool p_flag, bool p_leaderboard_flag = false)
		{
			leftSpaceLayout.minWidth = (p_flag ? 335f : 0f);
			RectTransform rectTransform = profileNameField.transform as RectTransform;
			float num = Mathf.Max(250f, rectTransform.sizeDelta.x);
			float num2 = 101f;
			float preferredWidth = (p_flag ? (num + num2) : 0f);
			profileNameContainer.preferredWidth = preferredWidth;
			rectTransform = progressionStreakContainer;
			rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x + 10f + 85f, rectTransform.anchoredPosition.y);
			timeContainer.preferredWidth = (p_flag ? 124f : 0f);
			leaderboardContainer.preferredWidth = (p_leaderboard_flag ? 326f : 0f);
			leaderboardFade.alpha = (p_leaderboard_flag ? 1f : 0f);
			headerLeaderboard = ((!p_leaderboard_flag) ? (-1) : 0);
			progressionRankField.frames = base.app.model.storage.state.player.progression.GetLeagueThumbSmallSprites().ToArray();
			progressionLevelRangeField.frames = base.app.model.storage.state.player.progression.GetLevelRangeThumbSprites().ToArray();
		}

		public void SetProgression(int p_league, int p_level, int p_xp_current, int p_xp_prev, int p_xp_next, int p_streak, float p_streak_current, int p_streak_goal)
		{
			float num = Mathf.Max(0f, p_xp_next - p_xp_prev);
			float x = Mathf.Clamp01((num <= 0f) ? 0f : ((float)(p_xp_current - p_xp_prev) / num));
			float x2 = ((p_streak_goal <= 0) ? 0f : Mathf.Clamp01(p_streak_current / (float)p_streak_goal));
			if ((bool)progressionRankField)
			{
				progressionRankField.frame = ((p_league >= 0) ? p_league : 0);
				progressionRankField.gameObject.SetActive(p_league >= 0);
				progressionLevelField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("ui.footer.level", "LEVEL"), p_level);
				progressionXPField.text = string.Format("{0} {1}/{2}", base.app.model.storage.locale.Get("ui.footer.xp", "XP"), p_xp_current, p_xp_next);
				progressionStreakField.text = string.Format("{0} {1}", base.app.model.storage.locale.Get("ui.footer.streak", "STREAK"), p_streak);
				progressionLevelRangeField.frame = base.app.model.storage.state.player.progression.GetLevelRangeIndexByLevel(p_level);
				progressionXPProgressBar.localScale = new Vector3(x, 1f, 1f);
				progressionStreakProgressBar.localScale = new Vector3(x2, 1f, 1f);
				m_progression_rank = p_league;
				m_xp_current = p_xp_current;
				m_xp_prev = p_xp_prev;
				m_xp_next = p_xp_next;
				m_streak = p_streak;
				m_streak_current = (int)p_streak_current;
				m_streak_goal = p_streak_goal;
			}
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

		public void SetProgression()
		{
			if (base.validContext)
			{
				ProgressionStateModel progression = base.app.model.storage.state.player.progression;
				SetProgression(progression.state);
			}
		}

		public void SetProgressionNext(int p_league, int p_level, int p_xp_current, int p_xp_prev, int p_xp_next, int p_streak, int p_streak_current, int p_streak_goal, float p_duration)
		{
			int xp_prev = m_xp_current;
			int xp_next = p_xp_current;
			int stk_prev = m_streak_current;
			int stk_next = p_streak_current;
			float e = 0f;
			if (m_progression_loop != null)
			{
				m_progression_loop.Stop();
			}
			m_progression_loop = Activity.Run((Func<bool>)delegate
			{
				e += Time.deltaTime;
				float num = Mathf.Clamp01((p_duration <= 0f) ? 1f : (e / p_duration));
				int p_xp_current2 = (int)Mathf.Lerp(t: Cubic.Out(Mathf.Clamp01((num - 0f) / 0.45f)), a: xp_prev, b: xp_next);
				float p_streak_current2 = Mathf.Lerp(t: Cubic.Out(Mathf.Clamp01((num - 0.55f) / 0.45f)), a: stk_prev, b: stk_next);
				if (num < 0.99f)
				{
					SetProgression(p_league, p_level, p_xp_current2, p_xp_prev, p_xp_next, p_streak, p_streak_current2, p_streak_goal);
					return true;
				}
				SetProgression(p_league, p_level, p_xp_current, p_xp_prev, p_xp_next, p_streak, p_streak_current, p_streak_goal);
				return false;
			}, 0f, false);
		}

		public void SetProgressionNext(DRLProgressionStateData p_state, float p_duration)
		{
			int leagueIndex = base.app.model.storage.state.player.progression.GetLeagueIndex();
			int level = p_state.level;
			int previousLevelXP = p_state.previousLevelXP;
			int xp = p_state.xp;
			int nextLevelXP = p_state.nextLevelXP;
			int streak = p_state.streak;
			int streakMapIndex = p_state.streakMapIndex;
			int streakMapCount = p_state.streakMapCount;
			SetProgressionNext(leagueIndex, level, xp, previousLevelXP, nextLevelXP, streak, streakMapIndex, streakMapCount, p_duration);
		}

		public void Fade(bool p_flag, string p_name, float p_time, float p_duration)
		{
			bool is_offline = DRLApp.offline;
			SetHeaderVisible(!p_flag);
			SetProgression();
			SetProgressionEnabled(!is_offline);
			if (p_flag)
			{
				headerName = p_name;
			}
			if (m_headername_delay != null)
			{
				m_headername_delay.Stop();
			}
			LayoutElement le;
			RectTransform rt;
			float w;
			m_headername_delay = RunOnce(delegate
			{
				float num = p_duration;
				float num2 = 0f;
				if (p_flag)
				{
					le = leftSpaceLayout;
					Tween.Kill(le);
					Tween.Add(le, "minWidth", 335f, num * 0.4f, num2, Cubic.Out);
					num2 += 0.1f;
					rt = profileNameField.transform as RectTransform;
					float num3 = Mathf.Max(250f, rt.sizeDelta.x);
					float num4 = (is_offline ? 0f : 101f);
					w = num3 + num4;
					le = profileNameContainer;
					Tween.Kill(le);
					Tween.Add(le, "preferredWidth", w, num * 0.3f, num2, Cubic.Out);
					rt = progressionStreakContainer;
					rt.anchoredPosition = new Vector2(num3 + 5f + 85f, rt.anchoredPosition.y);
					num2 += num * 0.3f - 0.1f;
					if (p_time > 0f)
					{
						le = timeContainer;
						Tween.Kill(le);
						Tween.Add(le, "preferredWidth", 124f, num * 0.3f, num2, Cubic.Out);
						num2 += 1f;
						this.TimerRunOnce(delegate
						{
							if (base.validContext && !DRLApp.isLoading)
							{
								Notify("game.race-complete.time.animation@start");
							}
							else
							{
								Notify("game.race-complete.time.animation@complete");
							}
						}, num2);
						headerTime = 0f;
						Tween.Kill(this);
						Tween.Add(this, "headerTime", p_time, 0.8f, num2, Cubic.Out);
						num2 += 0.8f;
						Activity.RunOnce(delegate
						{
							Notify("game.race-complete.time.animation@complete");
						}, num2);
					}
				}
				else
				{
					if (headerLeaderboard >= 0)
					{
						le = leaderboardContainer;
						Tween.Add(le, "preferredWidth", 0f, num * 0.3f, num2, Cubic.InOut);
						num2 += num * 0.3f * 0.3f;
						leaderboardFade.Fade(0f, num * 0.3f, num2);
						num2 += num * 0.3f * 0.3f * 0.3f;
					}
					le = timeContainer;
					Tween.Add(le, "preferredWidth", 0f, num * 0.3f, num2, Cubic.InOut);
					num2 += num * 0.3f * 0.3f;
					rt = profileNameField.transform as RectTransform;
					le = profileNameContainer;
					Tween.Add(le, "preferredWidth", 0f, num * 0.3f, num2, Cubic.InOut);
					num2 += num * 0.3f;
					le = leftSpaceLayout;
					Tween.Kill(le);
					Tween.Add(le, "minWidth", 0f, num * 0.4f, num2, Cubic.InOut);
				}
			}, p_flag ? (1f / 12f) : 0f);
		}

		public void FadeLeaderboard(int p_position, string p_name, float p_duration)
		{
			float num = 0f;
			if (p_position < 0)
			{
				Tween.Add(leaderboardContainer, "preferredWidth", 0f, p_duration * 0.3f, num, Cubic.InOut);
				num += p_duration * 0.3f * 0.3f;
				leaderboardFade.Fade(0f, p_duration * 0.3f, num);
				num += p_duration * 0.3f * 0.3f * 0.3f;
			}
			else
			{
				headerLeaderboard = p_position;
				headerLeaderboardName = p_name;
				leaderboardFade.Fade(1f, p_duration * 0.3f, num);
				num += p_duration * 0.3f * 0.3f;
				Tween.Add(leaderboardContainer, "preferredWidth", 326f, p_duration * 0.3f, num, Cubic.InOut);
			}
		}

		public void FadeLeaderboardCircuit(int p_position, string p_time, float p_duration)
		{
			float p_delay = 0f;
			int num = p_position;
			leaderboardCircuitField.text = "CIRCUIT COMPLETE - " + p_time + "\n#" + p_position + " ON DRL LEADERBOARD";
			if (num < 0)
			{
				Tween.Add(leaderboardCircuitContainer, "preferredWidth", 0f, p_duration * 0.3f, p_delay, Cubic.InOut);
				return;
			}
			if (leaderboardFade.alpha < 1f)
			{
				leaderboardFade.Fade(1f, p_duration * 0.3f);
			}
			Tween.Add(leaderboardCircuitContainer, "preferredWidth", 320f, p_duration * 0.3f, p_delay, Cubic.InOut);
		}

		public void Set(DroneCamera p_camera)
		{
			if (backgroundField.texture != null)
			{
				return;
			}
			backgroundField.enabled = false;
			if ((bool)p_camera)
			{
				p_camera.CaptureAsync(delegate(RenderTexture p_rt)
				{
					backgroundField.enabled = true;
					backgroundCapture = RenderTexture.GetTemporary(p_rt.width, p_rt.height, p_rt.depth, p_rt.format);
					Graphics.CopyTexture(p_rt, backgroundCapture);
					backgroundField.texture = backgroundCapture;
				});
			}
		}

		public void SetRestartEnabled(bool p_flag)
		{
			Debug.Log("UIRaceCompleteView> Restart enabled: " + p_flag);
			if (base.app.arguments.game.tournamentData != null)
			{
				p_flag = false;
			}
			restartButtonNav.gameObject.SetActive(p_flag);
		}

		public void SetTournamentRestartEnabled()
		{
			DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
			if (tournamentData != null)
			{
				TournamentRoundGameMode activeRoundMode = tournamentData.GetActiveRoundMode();
				if ((uint)(activeRoundMode - 3) <= 1u && tournamentData.GetActiveRoundState() == TournamentRoundState.active)
				{
					tournamentRestartButton.gameObject.SetActive(value: true);
				}
			}
		}

		public void SetNextEnabled(bool p_flag, float p_duration = 0f)
		{
			Debug.Log("UIRaceCompleteView> SetNextEnabled: " + p_flag);
			if (p_flag)
			{
				nextButton.gameObject.SetActive(value: true);
			}
			if (base.app.controller.game.model.fromEditor)
			{
				p_flag = true;
				p_duration = 0f;
			}
			float p_alpha = (p_flag ? 1f : 0.2f);
			nextEnabled = p_flag;
			if (p_duration <= 0f)
			{
				nextButtonFade.alpha = p_alpha;
			}
			else
			{
				nextButtonFade.Fade(p_alpha, p_duration);
			}
			if (p_flag && race != null && race.model.racersCount > 1)
			{
				race.model.RefreshStandings();
			}
			if (!p_flag)
			{
				return;
			}
			this.TimerRunOnce(delegate
			{
				if (base.validContext && base.current)
				{
					UINavigation.Focus(nextButton);
				}
			}, 0.25f);
		}

		public void SetExitEnabled(bool p_flag)
		{
			exitButtonNav.gameObject.SetActive(p_flag);
			Debug.Log("UIRaceCompleteView> SetExitEnabled: " + p_flag + StackTraceUtility.ExtractStackTrace());
			this.TimerRunOnce(delegate
			{
				if (base.validContext && base.current)
				{
					UINavigation.Focus(p_flag ? exitButtonNav : nextButtonNav);
				}
			}, 0.25f);
		}

		public void SetSaveFeedback(bool p_flag)
		{
			Transform obj = nextButtonFade.transform.Find("content");
			Transform transform = obj.Find("save");
			Transform obj2 = obj.Find("next");
			transform.gameObject.SetActive(p_flag);
			obj2.gameObject.SetActive(!p_flag);
			ImageClip imageClip = Hierarchy.Find<ImageClip>(transform);
			if ((bool)imageClip)
			{
				if (p_flag)
				{
					imageClip.Play();
				}
				else
				{
					imageClip.Stop();
				}
			}
		}

		public void SetSpectateEnabled(bool p_flag)
		{
			if (!p_flag && (UINavigation.focus ? UINavigation.focus.gameObject : null) == spectateButtonNav.gameObject)
			{
				UINavigation.Focus(nextButtonNav);
			}
			spectateButtonFade.gameObject.SetActive(p_flag);
		}

		public void SetRaceAnalytics()
		{
			playerCrash = race.model.crashes;
			playerTopSpeed = race.model.topSpeed;
			playerTimeInFirst = race.model.timeInFirstPlace;
			playerPercentile = race.model.playerPercentile;
			Debug.Log($"<color=green>RACE COMPLETE ANALYTICS:</color> PLAYER TIME:{race.model.time} , PLAYER TOPSPEED:{race.model.topSpeed}  , PLAYER TIME IN FIRST:{race.model.timeInFirstPlace}  , PLAYER PERCENTILE:{race.model.playerPercentile}  , PLAYER DISTANCE:{race.model.distanceTraveled} ");
			for (int i = 0; i < race.model.lapTimes.Count; i++)
			{
				bool p_slowestLap = false;
				bool p_fastestLap = false;
				if (race.model.slowestLapIndex == i)
				{
					p_slowestLap = true;
				}
				if (race.model.fastestLapIndex == i)
				{
					p_fastestLap = true;
				}
				AddCard(i, race.model.lapTimes[i], p_fastestLap, p_slowestLap);
			}
			playerTime = race.model.time;
			playerDistance = race.model.distanceTraveled;
		}

		private void AddCard(int lap_index, float p_lapTime, bool p_fastestLap = false, bool p_slowestLap = false)
		{
			listField.Push<UIRaceOverviewItemView>().Set(lap_index, p_lapTime, p_fastestLap, p_slowestLap);
		}

		public void FadeInAnalytics(float p_duration)
		{
			for (int i = 0; i < raceAnalyticsFades.Count; i++)
			{
				raceAnalyticsFades[i].Fade(1f, p_duration + (float)i * 0.25f);
			}
		}

		public void ClearMapRating(float p_duration)
		{
			for (int i = 0; i < mapRatingStarFades.Length; i++)
			{
				mapRatingStarFades[i].Fade(0.1f, p_duration);
			}
		}

		public void ClearDroneRating(float p_duration)
		{
			for (int i = 0; i < droneRatingStarFades.Length; i++)
			{
				droneRatingStarFades[i].Fade(0.1f, p_duration);
			}
		}

		public void FadeInMapRating(float p_duration, int p_rating)
		{
			if (p_rating > mapRatingStarFades.Length)
			{
				p_rating = mapRatingStarFades.Length;
			}
			mapRating.index = p_rating;
			for (int i = 0; i < mapRating.index; i++)
			{
				mapRatingStarFades[i].Fade(1f, p_duration + (float)i * 0.5f);
			}
		}

		public void FadeInDroneRating(float p_duration, int p_rating)
		{
			if (p_rating > droneRatingStarFades.Length)
			{
				p_rating = droneRatingStarFades.Length;
			}
			droneRating.index = p_rating;
			for (int i = 0; i < droneRating.index; i++)
			{
				droneRatingStarFades[i].Fade(1f, p_duration + (float)i * 0.5f);
			}
		}

		private void OnDestroy()
		{
			if (backgroundCapture != null)
			{
				backgroundCapture.DiscardContents();
				RenderTexture.ReleaseTemporary(backgroundCapture);
				backgroundCapture = null;
			}
			if (backgroundField.texture != null)
			{
				RenderTexture renderTexture = backgroundField.texture as RenderTexture;
				if (renderTexture != null)
				{
					renderTexture.DiscardContents();
					RenderTexture.ReleaseTemporary(renderTexture);
					renderTexture = null;
				}
				backgroundField.texture = null;
			}
		}
	}
}
