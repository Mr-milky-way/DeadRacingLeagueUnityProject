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
	public class UICollectablesCompleteView : UIScreenView
	{
		[Header("Screen")]
		public FadeComponent nextButtonFade;

		public UINavigation nextButtonNav;

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

		public LayoutElement collectablesContainer;

		public Text collectablesField;

		public LayoutElement leaderboardContainer;

		public FadeComponent leaderboardFade;

		public Text leaderboardField;

		public GameObject promo;

		public UIElementView droneRatingCard;

		public UIElementView mapRatingCard;

		public DRLStepperView droneRating;

		public DRLStepperView mapRating;

		public FadeComponent[] droneRatingStarFades;

		public FadeComponent[] mapRatingStarFades;

		public GameObject progressionCard;

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

		private bool dnf;

		private float m_header_timer;

		private int m_header_collectables;

		private int m_header_collectables_total;

		private int m_header_leaderboard;

		private string m_header_leaderboard_name = "LEADERBOARD";

		private MonoActivity m_headername_delay;

		public bool willSetLeaderboard;

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
				if (dnf)
				{
					timeField.text = "DNF\n" + Format.SecondsToMMSSFFF(value);
				}
				else
				{
					timeField.text = base.app.model.storage.locale.Get("leaderboard.result.time", "TIME") + "\n" + Format.SecondsToMMSSFFF(value);
				}
			}
		}

		public int headerCollectables
		{
			get
			{
				return m_header_collectables;
			}
			set
			{
				m_header_collectables = value;
				collectablesField.text = m_header_collectables + " / " + Header_collectables_total;
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

		public int Header_collectables_total
		{
			get
			{
				return m_header_collectables_total;
			}
			set
			{
				m_header_collectables_total = value;
			}
		}

		protected void Awake()
		{
			SetHeaderVisible(p_flag: false);
			headerName = "";
			headerTime = 0f;
			headerCollectables = 0;
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

		public void Fade(bool p_flag, string p_name, float p_time, int p_score, int p_total, float p_duration, bool p_success = false)
		{
			SetHeaderVisible(!p_flag);
			SetProgression();
			dnf = !p_success;
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
					float num4 = 101f;
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
						le = collectablesContainer;
						Tween.Kill(le);
						Tween.Add(le, "preferredWidth", 148f, num * 0.3f, num2, Cubic.Out);
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
						Tween.Add(this, "headerCollectables", p_score, 0.8f, num2, Cubic.Out);
						Header_collectables_total = p_total;
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

		public void Set(DroneCamera p_camera)
		{
			if (!(backgroundField.texture != null))
			{
				backgroundField.enabled = false;
				if ((bool)p_camera)
				{
					backgroundField.texture = p_camera.Capture();
				}
			}
		}

		public void SetRestartEnabled(bool p_flag)
		{
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
			playerCrash = collectable.model.crashes;
			playerTopSpeed = collectable.model.topSpeed;
			Debug.Log($"<color=green>Collectable COMPLETE ANALYTICS:</color> PLAYER TIME:{collectable.model.time} , PLAYER TOPSPEED:{collectable.model.topSpeed} ");
			playerTime = collectable.model.time;
			playerDistance = collectable.model.distanceTraveled;
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
	}
}
