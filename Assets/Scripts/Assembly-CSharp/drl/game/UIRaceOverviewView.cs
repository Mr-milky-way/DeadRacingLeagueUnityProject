using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIRaceOverviewView : UIScreenView
	{
		[Header("Screen")]
		public DRLStandingsView standings;

		public RaceController race;

		public GameCollectableController collectable;

		public Text title;

		public GameObject promo;

		public Text headerTitle;

		public Text tryoutsQualification;

		public Text tryoutsHeats;

		public UIElementView roomButtonView;

		public RenderTexture backgroundCapture;

		[Header("Stats")]
		public Text playerTimeField;

		public Text playerCrashField;

		public Text playerTopSpeedField;

		public Text playerTimeInFirstField;

		public Text playerPercentileField;

		public Text playerDistanceField;

		public ListComponent listField;

		[Header("Leaderboard")]
		public UILeaderboardCardView leaderCard;

		public UILeaderboardItemView[] rivals;

		public GameObject[] rivalsPlaceholders;

		[Header("Drone Info")]
		public RawImage droneThumb;

		public Text droneTitle;

		[Header("Nav")]
		public RectTransform restartButton;

		public RectTransform mapsButton;

		public RectTransform nextButton;

		public RectTransform campaignButton;

		public RectTransform replayButton;

		public RectTransform roomButton;

		public RectTransform exitButton;

		public RectTransform circuitsButton;

		public GameObject nextTryoutsButton;

		public GameObject completeTryoutsButton;

		public UINavigationLinkList restartProxyList;

		private bool _savingComplete;

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
				playerCrashField.text = value.ToString();
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
				playerPercentileField.text = value.ToString("0") + " %";
			}
		}

		public float playerDistance
		{
			set
			{
				playerDistanceField.text = ((value > 1000f) ? ((value / 1000f).ToString("0.0") + "KM / ") : (value.ToString("0") + "M / "));
			}
		}

		public StorageModel storage => base.app.model.storage;

		public bool exitEnabled { get; set; }

		public bool isSpectator { get; set; }

		public bool savingComplete
		{
			get
			{
				if (!_savingComplete && !storage.saveComplete)
				{
					return DRLApp.offline;
				}
				return true;
			}
			set
			{
				_savingComplete = (storage.saveComplete = value);
			}
		}

		protected void Awake()
		{
		}

		public void SetPromoEnabled(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
		}

		public void Clear()
		{
			FadeComponent component = leaderCard.GetComponent<FadeComponent>();
			if ((bool)component)
			{
				component.alpha = 0.1f;
			}
			leaderCard.Set((DRLLeaderboardData)null, 0.4f);
			SetRival(0, null);
			SetRival(1, null);
			SetRival(2, null);
			listField.Clear();
		}

		public void SetTitle()
		{
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			string text = (track ? track.label : "");
			if ((bool)map && map.data != null)
			{
				text = map.data.mapTitle;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = " / " + text.ToUpper();
			}
			headerTitle.text = (map ? (map.label + text) : "");
		}

		public void SetGameType(GameFlag p_type, bool p_multiplayer, bool p_from_editor, bool p_tryouts)
		{
			restartButton.gameObject.SetActive(value: false);
			mapsButton.gameObject.SetActive(value: false);
			nextButton.gameObject.SetActive(value: false);
			campaignButton.gameObject.SetActive(value: false);
			roomButton.gameObject.SetActive(value: false);
			UINavigation uINavigation = null;
			switch (p_type)
			{
			case GameFlag.Race:
				if (p_multiplayer)
				{
					roomButton.gameObject.SetActive(value: true);
					uINavigation = roomButton.GetComponent<UINavigation>();
					break;
				}
				restartButton.gameObject.SetActive(savingComplete && !p_tryouts && !base.app.inCircuits);
				mapsButton.gameObject.SetActive(!p_from_editor && !p_tryouts && !base.app.inCircuits);
				uINavigation = (p_tryouts ? nextTryoutsButton.GetComponent<UINavigation>() : restartButton.GetComponent<UINavigation>());
				if (base.app.inCircuits)
				{
					uINavigation = nextButton.GetComponent<UINavigation>();
				}
				break;
			case GameFlag.Campaign:
				restartButton.gameObject.SetActive(savingComplete);
				break;
			}
			if (base.app.inCircuits)
			{
				uINavigation = ((!(race != null) || race.model.status != RaceStatusType.Success) ? restartButton.GetComponent<UINavigation>() : circuitsButton.GetComponent<UINavigation>());
			}
			base.leftNavigation.right = uINavigation;
			if (uINavigation == null || !uINavigation.gameObject.activeInHierarchy)
			{
				base.leftNavigation.right = restartProxyList;
			}
		}

		public void SetReplayEnabled(bool p_flag)
		{
			if (base.app.model.network.room != null && base.app.model.network.room.State == NetworkRoom.StateCode.MatchLocked)
			{
				p_flag = false;
			}
			replayButton.GetComponent<FadeComponent>().alpha = (p_flag ? 1f : 0.2f);
			UIElementView component = replayButton.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_flag;
			}
		}

		public void LoadRaceData()
		{
			RaceController rc = race;
			if (!rc)
			{
				return;
			}
			if (isSpectator)
			{
				this.TimerRunOnce(delegate
				{
					rc.model.ForceSortRankings();
					standings.Refresh(rc.model.Rankings);
					standings.Fade(p_flag: true, 0.6f);
					Hierarchy.RefreshLayout(standings, standings.transform, p_disable_csf: true);
				}, 1f);
			}
			else
			{
				standings.Refresh(rc.model.Rankings);
				standings.Fade(p_flag: true, 0.6f);
				Hierarchy.RefreshLayout(standings, standings.transform, p_disable_csf: true);
			}
		}

		private void SetRaceAnalytics()
		{
			RaceController raceController = race;
			if (!raceController)
			{
				return;
			}
			RaceModel model = raceController.model;
			playerTime = model.time;
			playerCrash = model.crashes;
			playerTopSpeed = model.topSpeed;
			playerTimeInFirst = model.timeInFirstPlace;
			playerPercentile = model.playerPercentile;
			playerDistance = model.distanceTraveled;
			Debug.Log($"<color=green>RACE OVERVIEW ANALYTICS:</color> PLAYER TIME:{model.time} , PLAYER TOPSPEED:{model.topSpeed}  , PLAYER TIME IN FIRST:{model.timeInFirstPlace}  , PLAYER PERCENTILE:{model.playerPercentile}  , PLAYER DISTANCE:{model.distanceTraveled} ");
			for (int i = 0; i < model.lapTimes.Count; i++)
			{
				bool p_slowestLap = false;
				bool p_fastestLap = false;
				if (model.slowestLapIndex == i)
				{
					p_slowestLap = true;
				}
				if (model.fastestLapIndex == i)
				{
					p_fastestLap = true;
				}
				AddCard(i, model.lapTimes[i], p_fastestLap, p_slowestLap);
			}
		}

		public void SetLeader(DRLLeaderboardData p_data)
		{
			FadeComponent component = leaderCard.GetComponent<FadeComponent>();
			leaderCard.Set(p_data);
			float p_alpha = ((p_data == null) ? 0.1f : 1f);
			if ((bool)component)
			{
				component.Fade(p_alpha, 0.5f);
			}
		}

		public void SetRival(int p_id, DRLLeaderboardData p_data)
		{
			if (p_id >= 0 && p_id < rivals.Length)
			{
				rivalsPlaceholders[p_id].SetActive(p_data == null);
				rivals[p_id].gameObject.SetActive(p_data != null);
				rivals[p_id].Set(p_data);
				if (p_data != null)
				{
					string text = "#" + p_data.position + " " + rivals[p_id].profileNameField.text;
					int num = 15;
					int length = Mathf.Min(text.Length, num);
					rivals[p_id].profileNameField.text = text.Substring(0, length) + ((text.Length > num) ? "..." : "");
				}
			}
		}

		public void SetUserQualified()
		{
			if (title != null)
			{
				title.gameObject.SetActive(value: false);
			}
			if (tryoutsHeats != null)
			{
				tryoutsHeats.gameObject.SetActive(value: false);
			}
			if (tryoutsQualification != null)
			{
				tryoutsQualification.gameObject.SetActive(value: true);
			}
			completeTryoutsButton.SetActive(value: true);
		}

		internal void SetHeatsFeedback(int heats)
		{
			if (title != null)
			{
				title.gameObject.SetActive(value: false);
			}
			if (tryoutsQualification != null)
			{
				tryoutsQualification.gameObject.SetActive(value: false);
			}
			if (tryoutsHeats != null)
			{
				tryoutsHeats.text = "HEAT " + heats + "/3";
				tryoutsHeats.gameObject.SetActive(value: true);
			}
			nextTryoutsButton.SetActive(value: true);
			exitButton.gameObject.SetActive(savingComplete);
		}

		public void SetDroneCard(DroneRigData p_data)
		{
			DroneRigData d = p_data;
			GarageStateModel model = base.app.model.storage.state.player.garage;
			droneThumb.gameObject.SetActive(value: true);
			model.GetRigThumbnail(d, 320, 0, delegate(Texture2D p_result)
			{
				if (p_result != null)
				{
					droneThumb.texture = p_result;
				}
				else
				{
					model.TryGetBaseDrone(d, out var p_base_drone);
					if (p_base_drone != null)
					{
						model.GetRigThumbnail(p_base_drone, 320, 0, delegate(Texture2D p_tex)
						{
							if (p_tex == null)
							{
								droneThumb.gameObject.SetActive(value: false);
							}
							else
							{
								droneThumb.texture = p_tex;
							}
						});
					}
				}
			});
			droneTitle.text = "";
			int num = 17;
			for (int num2 = 0; num2 < p_data.rigName.Length; num2++)
			{
				if (num2 < num)
				{
					droneTitle.text += p_data.rigName[num2];
					continue;
				}
				droneTitle.text += "...";
				break;
			}
			droneTitle.text = droneTitle.text.ToUpper();
		}

		private void AddCard(int lap_index, float p_lapTime, bool p_fastestLap = false, bool p_slowestLap = false)
		{
			listField.Push<UIRaceOverviewItemView>().Set(lap_index, p_lapTime, p_fastestLap, p_slowestLap);
		}

		public void SetRestartButton()
		{
			bool flag = race.model.status == RaceStatusType.Success;
			bool inProgress = base.app.model.storage.state.player.circuits.inProgress;
			bool tryouts = base.app.arguments.game.tryouts;
			bool flag2 = !(base.app.arguments.game.tournamentData != null || tryouts);
			if (inProgress && flag)
			{
				flag2 = false;
			}
			if (base.app.inMultiplayer)
			{
				flag2 = false;
			}
			restartButton.gameObject.SetActive(flag2 && savingComplete);
		}

		public void SetExitButton()
		{
			exitButton.gameObject.SetActive(savingComplete);
		}

		public void SetMapButton()
		{
			bool fromEditor = base.app.controller.game.model.fromEditor;
			bool tryouts = base.app.arguments.game.tryouts;
			mapsButton.gameObject.SetActive(!fromEditor && !tryouts && !base.app.inCircuits && savingComplete);
		}
	}
}
