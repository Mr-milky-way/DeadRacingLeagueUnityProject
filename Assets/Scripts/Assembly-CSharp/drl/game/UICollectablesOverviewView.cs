using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UICollectablesOverviewView : UIScreenView
	{
		[Header("Screen")]
		public DRLStandingsView standings;

		public GameController race;

		public GameCollectableController collectable;

		public Text title;

		public GameObject promo;

		public Text headerTitle;

		public Text tryoutsQualification;

		public Text tryoutsHeats;

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

		public UILeaderboardItemView[] leaders;

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

		public GameObject nextTryoutsButton;

		public GameObject completeTryoutsButton;

		public GameTypeController gameMode
		{
			get
			{
				if (!collectable)
				{
					return null;
				}
				return collectable;
			}
		}

		public float playerTime
		{
			set
			{
				playerTimeField.text = Format.SecondsToTime(value, 2, p_use_ms: true);
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
				playerTimeInFirstField.text = Format.SecondsToTime(value, 2, p_use_ms: true);
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

		public bool exitEnabled { get; set; }

		public bool isSpectator { get; set; }

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
			switch (p_type)
			{
			case GameFlag.Collectable:
				restartButton.gameObject.SetActive(value: true);
				mapsButton.gameObject.SetActive(!p_from_editor && !p_tryouts);
				exitEnabled = true;
				break;
			case GameFlag.Race:
			{
				restartButton.gameObject.SetActive(!p_tryouts);
				mapsButton.gameObject.SetActive(!p_from_editor && !p_tryouts);
				UINavigation uINavigation = null;
				uINavigation = (p_tryouts ? nextTryoutsButton.GetComponent<UINavigation>() : restartButton.GetComponent<UINavigation>());
				base.leftNavigation.right = uINavigation;
				break;
			}
			case GameFlag.Campaign:
				restartButton.gameObject.SetActive(value: true);
				break;
			}
		}

		public void SetReplayEnabled(bool p_flag)
		{
			if (base.app.model.network.room != null && base.app.model.network.room.State == NetworkRoom.StateCode.MatchLocked)
			{
				p_flag = false;
			}
			replayButton.GetComponent<FadeComponent>().alpha = (p_flag ? 1f : 0.2f);
		}

		public void LoadRaceData()
		{
			_ = race;
			GameCollectableController gameCollectableController = collectable;
			List<GamePlayerData> p_players = new List<GamePlayerData> { base.app.controller.game.model.playerData };
			SetRaceAnalytics();
			bool p_dnf = false;
			if (gameCollectableController.model.status != RaceStatusType.Success)
			{
				p_dnf = true;
			}
			standings.Refresh(p_players, p_clear: false, p_dnf);
			standings.Fade(p_flag: true, 0.6f);
			Hierarchy.RefreshLayout(standings, standings.transform, p_disable_csf: true);
		}

		private void SetRaceAnalytics()
		{
			GameCollectableModel model = collectable.model;
			_ = base.app.controller.game.model.playerData.rig;
			playerTime = model.time;
			playerCrash = model.crashes;
			playerTopSpeed = model.topSpeed;
			playerDistance = model.distanceTraveled;
			Debug.Log($"<color=green>RACE OVERVIEW ANALYTICS:</color> PLAYER TIME:{model.time} , PLAYER TOPSPEED:{model.topSpeed} ");
			AddCard(model.time, model.topSpeed, 0);
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
			exitButton.gameObject.SetActive(value: true);
		}

		public void SetDroneCard(DroneRigData p_data)
		{
			base.app.model.storage.state.player.garage.GetRigThumbnail(p_data, 320, 0, delegate(Texture2D p_result)
			{
				if (p_result != null)
				{
					droneThumb.texture = p_result;
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

		private void AddCard(float p_time, float p_topSpeed, int p_collected)
		{
			listField.Push<UIRaceOverviewItemView>().Set(p_time, p_topSpeed, p_collected);
		}
	}
}
