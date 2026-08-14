using System;
using System.Collections.Generic;
using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISpectateModel : Model<DRLApp>
	{
		public List<MEInfoHelpData> helpDataList;

		[SerializeField]
		private GameTypeController m_game;

		public SpectateCameraModeType cameraMode;

		public bool cameraCourseActive;

		public SpectateDroneTrailModeType trailMode = SpectateDroneTrailModeType.Auto;

		public SpectateDroneTrailWidthModeType trailWidthMode = SpectateDroneTrailWidthModeType.Auto;

		public bool isReplay;

		public List<Transform> targets = new List<Transform>();

		public List<Drone> drones = new List<Drone>();

		public List<GamePlayerData> players = new List<GamePlayerData>();

		public List<DroneInputTransmitter> transmitters = new List<DroneInputTransmitter>();

		public List<ReplayClipPlayerModel> replays = new List<ReplayClipPlayerModel>();

		public List<string> names = new List<string>();

		public List<MACameraTool> cameraTools;

		public List<MACameraTool> cameraToolTargetFocus;

		private List<Ray> m_targets_rays;

		public int focus;

		public int cameraToolFocus;

		public SplineActor courseCameraActorTemplate;

		public int courseCameraFocus;

		public List<MASpline> courseCameras;

		public List<SplineActor> courseCameraActors;

		public bool changeFocusUponFinish;

		public bool keepFocusOnLeader;

		public UISpectateVideoFlags videoCaptureSizeMode = UISpectateVideoFlags.Size1080;

		public UISpectateVideoFlags videoCaptureApectMode = UISpectateVideoFlags.AspectWH;

		public UISpectateVideoFlags videoCaptureFPSMode = UISpectateVideoFlags.FPS60;

		public UISpectateVideoFlags videoCaptureQualityMode = UISpectateVideoFlags.Quality1;

		public float videoCaptureRangeStart;

		public float videoCaptureRangeEnd;

		public bool videoCaptureCropEnabled;

		[SerializeField]
		private string m_vc_output_folder_path;

		public int collectCount;

		[Header("Focus")]
		[SerializeField]
		protected Drone m_focus_drone;

		[SerializeField]
		protected GamePlayerData m_focus_player;

		[SerializeField]
		protected string m_focus_name;

		[SerializeField]
		protected Transform m_focus_target;

		[SerializeField]
		protected DroneInputTransmitter m_focus_transmitter;

		[SerializeField]
		protected ReplayClipPlayerModel m_focus_replay;

		protected List<ReplayEvent> m_focus_replay_events;

		private float defaultSmoothing = 0.015f;

		private Activity m_drone_rendering_call;

		private List<MACameraTool> m_find_cameratool_results;

		private float m_next_ratio;

		private Vector3 m_next_position;

		private float[] trail_durations = new float[4] { 0f, 0.3f, 2f, 6f };

		private float[] trail_width_multipliers = new float[4] { 1f, 1f, 1.5f, 2f };

		public GameTypeController game
		{
			get
			{
				if ((bool)m_game)
				{
					return m_game;
				}
				GameController gameController = base.app.controller.game;
				if (!gameController)
				{
					return null;
				}
				return m_game = gameController.GetMode<GameTypeController>();
			}
		}

		public List<Ray> targetRays
		{
			get
			{
				if (m_targets_rays != null)
				{
					return m_targets_rays;
				}
				return m_targets_rays = new List<Ray>();
			}
			set
			{
				m_targets_rays = value;
			}
		}

		public string videoCaptureOutputFolderPath
		{
			get
			{
				string text = PlayerPrefs.GetString("game-spectate.video-capture.ouput-folder-path", DRLPaths.Storage.videoRecordRoot);
				if (string.IsNullOrEmpty(text))
				{
					text = DRLPaths.Storage.videoRecordRoot;
				}
				return text;
			}
			set
			{
				PlayerPrefs.SetString("game-spectate.video-capture.ouput-folder-path", m_vc_output_folder_path = value);
			}
		}

		private float default_fov => base.app.model.storage.state.player.settings.tuning.GetActive().fov;

		private float default_tilt => base.app.model.storage.state.player.settings.tuning.GetActive().tilt;

		public void AddTargets(List<GamePlayerData> p_data)
		{
			if (p_data == null)
			{
				return;
			}
			foreach (GamePlayerData p_datum in p_data)
			{
				AddTarget(p_datum);
			}
		}

		public void AddTarget(GamePlayerData p_data)
		{
			if (!base.validContext)
			{
				Debug.LogWarning("UISpectateModel> AddTarget / InvalidContext");
				return;
			}
			if (p_data == null)
			{
				Debug.LogWarning("UISpectateModel> AddTarget / Invalid PlayerData");
				return;
			}
			GamePlayerData playerById = GetPlayerById(p_data.playerId);
			if (!base.app.inVirtualSeason || !base.app.inTournament || !isReplay)
			{
				if (playerById != null)
				{
					Notify(1f / 60f, "spectate.targets@change");
					return;
				}
				if (!p_data.isRacer)
				{
					return;
				}
			}
			DroneTransmitterManager dtm = base.app.model.game.simulation.transmitters;
			playerById = p_data;
			players.Add(playerById);
			drones.Add(playerById.drone);
			targets.Add(playerById.drone ? playerById.drone.transform : null);
			names.Add(playerById.name);
			transmitters.Add(dtm ? dtm.GetByDrone<DroneInputTransmitter>(playerById.drone) : null);
			replays.Add(null);
			targetRays.Add(default(Ray));
			RefreshCameraToolTargetFocus();
			string player_id = playerById.playerId;
			Activity.Run((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				for (int i = 0; i < players.Count; i++)
				{
					GamePlayerData gamePlayerData = players[i];
					if (gamePlayerData != null && !(gamePlayerData.playerId != player_id))
					{
						if ((bool)drones[i])
						{
							return false;
						}
						Drone drone = gamePlayerData.drone;
						if (!drone)
						{
							break;
						}
						drones[i] = drone;
						targets[i] = drone.transform;
						transmitters[i] = (dtm ? dtm.GetByDrone<DroneInputTransmitter>(drone) : null);
						return false;
					}
				}
				Notify(1f / 60f, "spectate.targets@change");
				return true;
			}, 0f, false);
			Notify(1f / 60f, "spectate.targets@change");
		}

		public void RemoveTarget(GamePlayerData p_data)
		{
			RemoveTargetById((p_data == null) ? "" : p_data.playerId);
		}

		public void RemoveTargetById(string p_steam_id)
		{
			if (!string.IsNullOrEmpty(p_steam_id))
			{
				GamePlayerData playerById = GetPlayerById(p_steam_id);
				if (playerById != null)
				{
					int idx = players.IndexOf(playerById);
					RemovePlayerByIndex(idx);
				}
			}
		}

		public void RemoveTargetByNetworkId(int p_network_id)
		{
			GamePlayerData playerByNetworkId = GetPlayerByNetworkId(p_network_id);
			if (playerByNetworkId != null)
			{
				int idx = players.IndexOf(playerByNetworkId);
				RemovePlayerByIndex(idx);
			}
		}

		private void RemovePlayerByIndex(int idx)
		{
			if ((!isReplay || !base.app.inVirtualSeason || !base.app.inTournament) && idx >= 0 && players.Count != 0 && idx <= players.Count - 1)
			{
				if (focus == idx)
				{
					focus = -1;
					SetFocusAvailable();
				}
				players.RemoveAt(idx);
				drones.RemoveAt(idx);
				targets.RemoveAt(idx);
				names.RemoveAt(idx);
				transmitters.RemoveAt(idx);
				replays.RemoveAt(idx);
				targetRays.RemoveAt(idx);
				RefreshCameraToolTargetFocus();
				Notify(1f / 60f, "spectate.targets@change");
			}
		}

		public void SetTargets(List<GamePlayerData> p_list)
		{
			focus = -1;
			targets.Clear();
			drones.Clear();
			players.Clear();
			transmitters.Clear();
			replays.Clear();
			names.Clear();
			targetRays.Clear();
			for (int i = 0; i < p_list.Count; i++)
			{
				GamePlayerData gamePlayerData = p_list[i];
				if (gamePlayerData != null && gamePlayerData.isRacer && !(gamePlayerData.drone == null))
				{
					players.Add(gamePlayerData);
					drones.Add(gamePlayerData.drone);
					targets.Add(gamePlayerData.drone ? gamePlayerData.drone.transform : null);
					names.Add(gamePlayerData.name);
					transmitters.Add(base.app.model.game.simulation.transmitters.GetByDrone<DroneInputTransmitter>(gamePlayerData.drone));
					replays.Add(null);
					targetRays.Add(default(Ray));
				}
			}
			RefreshCameraToolTargetFocus();
			Debug.Log($"UISpectateModel> SetTargets / Waiting for {p_list.Count} Players to Spectate!");
			float time_out = 0.5f;
			Activity.Run((Func<bool>)delegate
			{
				if (!CheckAllDroneReady() && !(time_out <= 0f))
				{
					time_out -= Time.deltaTime;
					return true;
				}
				Debug.Log($"UISpectateModel> SetTargets / {p_list.Count} Players are Ready to Spectate - timeout[{time_out}]!");
				Notify(1f / 60f, "spectate.targets@change");
				Notify(1f / 15f, "spectate.targets.ready");
				return false;
			}, 0f, false);
		}

		protected bool CheckAllDroneReady()
		{
			int num = 0;
			for (int i = 0; i < drones.Count; i++)
			{
				Drone drone = drones[i];
				if (drone != null && drone.ready)
				{
					num++;
				}
			}
			return num >= drones.Count;
		}

		public void SetTargets(List<ReplayClipPlayerModel> p_list)
		{
			if (p_list == null || p_list.Count == 0)
			{
				return;
			}
			DroneSimulation simulation = base.app.model.game.simulation;
			if (!simulation)
			{
				Debug.LogWarning("UISpectateModel> Initialize - Failed to find the simulation");
				return;
			}
			targets.Clear();
			drones.Clear();
			players.Clear();
			transmitters.Clear();
			replays.Clear();
			names.Clear();
			targetRays.Clear();
			List<ReplayClipPlayerModel> list = new List<ReplayClipPlayerModel>(p_list);
			list.RemoveAll(PruneReplayClips);
			list.Sort(SortReplayClips);
			DroneTransmitterManager droneTransmitterManager = base.app.model.game.simulation.transmitters;
			for (int i = 0; i < list.Count; i++)
			{
				ReplayClipPlayerModel replayClipPlayerModel = list[i];
				replays.Add(replayClipPlayerModel);
				players.Add(replayClipPlayerModel.player);
				drones.Add(replayClipPlayerModel.drone);
				targets.Add(replayClipPlayerModel.drone ? replayClipPlayerModel.drone.transform : null);
				names.Add(replayClipPlayerModel.player.name);
				DroneInputTransmitter byDrone = droneTransmitterManager.GetByDrone<DroneInputTransmitter>(replayClipPlayerModel.drone);
				transmitters.Add(byDrone);
				int p_index = ((simulation.podiums.list.Count > 0) ? (replayClipPlayerModel.player.order % simulation.podiums.list.Count) : 0);
				DronePodium dronePodium = simulation.podiums.Get(p_index);
				if (!dronePodium)
				{
					Debug.LogWarning($"UISpectateController> SetTargets / Failed to find podium -  index[{i}] count[{simulation.podiums.list.Count}]");
					continue;
				}
				replayClipPlayerModel.podiumBlendDuration = (dronePodium.spawn ? replayClipPlayerModel.podiumBlendDuration : 0f);
				replayClipPlayerModel.podium = (dronePodium.spawn ? dronePodium.spawn.position : Vector3.zero);
				replayClipPlayerModel.podiumRotation = (dronePodium.spawn ? dronePodium.spawn.rotation : Quaternion.identity);
				replayClipPlayerModel.usePodium = simulation.podiums.list.Count > 1;
				targetRays.Add(new Ray(dronePodium.spawn.position, dronePodium.spawn.forward));
			}
			RefreshCameraToolTargetFocus();
			Activity.Run((Func<bool>)delegate
			{
				int num = 0;
				for (int j = 0; j < drones.Count; j++)
				{
					if (drones[j].ready)
					{
						num++;
					}
				}
				if (num >= drones.Count)
				{
					Notify(1f / 30f, "spectate.targets.ready");
					return false;
				}
				return true;
			}, 0f, false);
			Notify(1f / 60f, "spectate.targets@change");
		}

		public void ResetDamageData()
		{
		}

		public T GetFocus<T>()
		{
			if (typeof(T) == typeof(Transform))
			{
				return (T)(object)m_focus_target;
			}
			if (typeof(T) == typeof(string))
			{
				return (T)(object)m_focus_name;
			}
			if (typeof(T) == typeof(int))
			{
				return (T)(object)focus;
			}
			if (typeof(T) == typeof(DroneInputTransmitter))
			{
				return (T)(object)m_focus_transmitter;
			}
			if (typeof(T) == typeof(GamePlayerData))
			{
				return (T)(object)m_focus_player;
			}
			if (typeof(T) == typeof(Drone))
			{
				return (T)(object)m_focus_drone;
			}
			if (typeof(T) == typeof(ReplayClipPlayerModel))
			{
				return (T)(object)m_focus_replay;
			}
			if (typeof(T) == typeof(List<ReplayEvent>))
			{
				return (T)(object)m_focus_replay_events;
			}
			return default(T);
		}

		public void SetFocus(int p_index)
		{
			focus = p_index;
			m_focus_drone = null;
			m_focus_player = null;
			m_focus_target = null;
			m_focus_transmitter = null;
			m_focus_replay = null;
			m_focus_name = "none";
			if (focus >= 0 && focus < players.Count)
			{
				m_focus_target = targets[focus];
				m_focus_name = names[focus];
				m_focus_player = players[focus];
				m_focus_drone = drones[focus];
				m_focus_replay = replays[focus];
				m_focus_transmitter = base.app.model.game.simulation.transmitters.GetByDrone<DroneInputTransmitter>(m_focus_drone);
				if (!transmitters.Contains(m_focus_transmitter))
				{
					transmitters.Add(m_focus_transmitter);
				}
				collectCount = ((m_focus_replay != null) ? m_focus_replay.GetCollectCount() : 0);
				m_focus_replay_events = ((!m_focus_replay) ? new List<ReplayEvent>() : ((m_focus_replay.clipV2 == null) ? new List<ReplayEvent>() : new List<ReplayEvent>(m_focus_replay.clipV2.header.events)));
				m_focus_replay_events.RemoveAll(ReplayEventListRemoveNonRace);
			}
			RefreshDroneRendering();
			Debug.Log($"UISpectateController> SetFocus - focus[{focus} / {m_focus_name}]");
			Notify("spectate.focus@change");
		}

		private static bool ReplayEventListRemoveNonRace(ReplayEvent v)
		{
			return v.typeFlag switch
			{
				ReplayEventType.Reset => true, 
				ReplayEventType.Action => true, 
				ReplayEventType.None => true, 
				_ => false, 
			};
		}

		public bool SetFocusAvailable()
		{
			Debug.Log("UISpectateModel> SetFocusAvailable()");
			int num = focus;
			int num2 = -1;
			List<int> list = new List<int>();
			if (cameraMode == SpectateCameraModeType.FreeCamera)
			{
				return false;
			}
			if (isReplay)
			{
				for (int i = 0; i < replays.Count; i++)
				{
					ReplayClipPlayerModel replayClipPlayerModel = replays[i];
					if (replayClipPlayerModel.elapsed < replayClipPlayerModel.duration)
					{
						list.Add(drones.IndexOf(replayClipPlayerModel.drone));
					}
				}
			}
			else
			{
				for (int j = 0; j < transmitters.Count; j++)
				{
					DroneInputTransmitter droneInputTransmitter = transmitters[j];
					if ((bool)droneInputTransmitter)
					{
						bool flag = false;
						if (droneInputTransmitter is DroneNetworkTransmitter)
						{
							flag = ((DroneNetworkTransmitter)droneInputTransmitter).Actor.RaceState == NetworkActor.RacerState.Running;
						}
						if (droneInputTransmitter is DroneGhostTransmitter)
						{
							DroneGhostTransmitter droneGhostTransmitter = (DroneGhostTransmitter)droneInputTransmitter;
							flag = droneGhostTransmitter.elapsed < droneGhostTransmitter.raceTime;
						}
						if (flag)
						{
							list.Add(drones.IndexOf(droneInputTransmitter.drone));
							num2 = drones.IndexOf(droneInputTransmitter.drone);
						}
					}
				}
			}
			if (game.type == GameFlag.Race)
			{
				List<GamePlayerData> rankings = ((RaceController)game).model.Rankings;
				for (int k = 0; k < rankings.Count; k++)
				{
					if (rankings[k].raceStatus == RaceStatusType.Running)
					{
						num2 = players.IndexOf(rankings[k]);
						break;
					}
				}
			}
			if (list.Count > 0)
			{
				num2 = list[0];
			}
			Debug.Log($"UISpectateController> SetFocusAvailable - focus[{num} -> {num2}]");
			if (num2 < 0)
			{
				return false;
			}
			SetFocus(num2);
			return true;
		}

		public void SetFocus(Drone p_drone)
		{
			SetFocus(drones.IndexOf(p_drone));
		}

		public void SetFocus(GamePlayerData p_player)
		{
			GamePlayerData playerById = GetPlayerById(p_player.playerId);
			SetFocus(players.IndexOf(playerById));
		}

		public void SetFocus(ReplayClipPlayerModel p_replay)
		{
			SetFocus(p_replay.player);
		}

		public List<string> GetUpperCaseNames()
		{
			return names.ConvertAll((string it) => it.ToUpper());
		}

		public GamePlayerData GetPlayerById(string p_id)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].playerId == p_id)
				{
					return players[i];
				}
			}
			return null;
		}

		public GamePlayerData GetPlayerByNetworkId(int p_id)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].id == p_id)
				{
					return players[i];
				}
			}
			return null;
		}

		public bool Exists(GamePlayerData p_player)
		{
			return GetPlayerById(p_player.playerId) != null;
		}

		public float GetGameTime()
		{
			if (!game)
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 0f;
			if (game is RaceController)
			{
				RaceController obj = game as RaceController;
				num = obj.model.timeStart;
				num2 = obj.GetGlobalTime();
			}
			if (game is NetworkRaceController)
			{
				NetworkRaceController obj2 = game as NetworkRaceController;
				num = 0f;
				num2 = obj2.GetGlobalTime();
			}
			return num2 - num;
		}

		public float GetGameTime(DroneInputTransmitter p_target)
		{
			if (!game)
			{
				return 0f;
			}
			if (game is RaceController)
			{
				return (game as RaceController).model.GetRaceTime(p_target ? p_target.drone : null);
			}
			if (game is NetworkRaceController)
			{
				return (game as NetworkRaceController).model.GetRaceTime(p_target ? p_target.drone : null);
			}
			return 0f;
		}

		public int GetFocusDroneGate()
		{
			Drone drone = GetFocus<Drone>();
			if (!drone)
			{
				return 0;
			}
			if (!game)
			{
				return 0;
			}
			if (game is RaceController)
			{
				return (game as RaceController).model.GetProgress(drone);
			}
			if (game is NetworkRaceController)
			{
				return (game as NetworkRaceController).model.GetProgress(drone);
			}
			return 0;
		}

		public ControllerStateType GetFocusController()
		{
			ControllerStateType result = ControllerStateType.Taranis;
			if (isReplay)
			{
				ReplayClipPlayerModel replayClipPlayerModel = GetFocus<ReplayClipPlayerModel>();
				if (replayClipPlayerModel == null)
				{
					return result;
				}
				if (ReplayFile.EnableVersion2)
				{
					result = replayClipPlayerModel.clipV2.header.controllerTypeFlag;
				}
				else
				{
					int value = replayClipPlayerModel.clip.header.Get<int>("controller-type");
					result = (ControllerStateType)Enum.ToObject(typeof(ControllerStateType), value);
				}
			}
			else
			{
				if (!game)
				{
					return result;
				}
				DroneInputTransmitter droneInputTransmitter = GetFocus<DroneInputTransmitter>();
				if (droneInputTransmitter == null)
				{
					return result;
				}
				if (droneInputTransmitter is DroneNetworkTransmitter)
				{
					result = (droneInputTransmitter as DroneNetworkTransmitter).GetControllerType();
				}
				if (droneInputTransmitter is DroneGhostTransmitter)
				{
					result = (droneInputTransmitter as DroneGhostTransmitter).GetControllerType();
				}
			}
			return result;
		}

		protected int GetClosestEvent(float p_time)
		{
			List<ReplayEvent> focus_replay_events = m_focus_replay_events;
			if (focus_replay_events == null)
			{
				return -1;
			}
			if (focus_replay_events.Count <= 0)
			{
				return -1;
			}
			if (focus_replay_events.Count <= 1)
			{
				return 0;
			}
			int result = 0;
			int count = focus_replay_events.Count;
			if (p_time <= focus_replay_events[0].time)
			{
				return 0;
			}
			if (p_time >= focus_replay_events[count - 1].time)
			{
				return count - 1;
			}
			for (int i = 1; i < count; i++)
			{
				ReplayEvent replayEvent = focus_replay_events[i - 1];
				ReplayEvent replayEvent2 = focus_replay_events[i];
				if (p_time >= replayEvent.time && p_time < replayEvent2.time)
				{
					float num = (replayEvent.time + replayEvent2.time) * 0.5f;
					result = ((p_time <= num) ? (i - 1) : i);
					break;
				}
			}
			return result;
		}

		public float GetNextEventTime(float p_time)
		{
			int closestEvent = GetClosestEvent(p_time);
			if (closestEvent < 0)
			{
				return p_time;
			}
			int num = ((m_focus_replay_events != null) ? m_focus_replay_events.Count : 0);
			closestEvent++;
			if (closestEvent >= num)
			{
				closestEvent = 0;
			}
			return m_focus_replay_events[closestEvent].time;
		}

		public float GetPrevEventTime(float p_time)
		{
			int closestEvent = GetClosestEvent(p_time);
			if (closestEvent < 0)
			{
				return p_time;
			}
			int num = ((m_focus_replay_events != null) ? m_focus_replay_events.Count : 0);
			closestEvent--;
			if (closestEvent < 0)
			{
				closestEvent = num - 1;
			}
			return m_focus_replay_events[closestEvent].time;
		}

		public void ClearDroneTrails()
		{
			List<Drone> list = drones;
			if (list == null)
			{
				list = new List<Drone>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				Drone drone = list[i];
				if ((bool)drone && drone.ready)
				{
					drone.renderer.ClearTrails();
				}
			}
		}

		public void SetDroneTrailDuration(float p_duration)
		{
			List<Drone> list = drones;
			if (list == null)
			{
				list = new List<Drone>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				Drone drone = list[i];
				if ((bool)drone && drone.ready)
				{
					drone.renderer.SetTrailsDuration(p_duration);
				}
			}
		}

		public void SetDroneTrailWidthMultiplier(float p_multiplier)
		{
			List<Drone> list = drones;
			if (list == null)
			{
				list = new List<Drone>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				Drone drone = list[i];
				if ((bool)drone && drone.ready)
				{
					drone.renderer.SetTrailsWidth(p_multiplier);
				}
			}
		}

		public void SetDroneTrailScale(float p_value)
		{
			List<Drone> list = drones;
			if (list == null)
			{
				list = new List<Drone>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				Drone drone = list[i];
				if ((bool)drone && drone.ready)
				{
					drone.renderer.trailScale = p_value;
				}
			}
		}

		public void SetDroneTrailEnabled(bool p_flag)
		{
			List<Drone> list = drones;
			if (list == null)
			{
				list = new List<Drone>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				Drone drone = list[i];
				if ((bool)drone && drone.ready)
				{
					drone.renderer.SetTrailsEnabled(p_flag);
				}
			}
		}

		public void SetDroneMotorRPM(float p_value)
		{
			List<ReplayClipPlayerModel> list = replays;
			if (list == null)
			{
				list = new List<ReplayClipPlayerModel>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetDroneMotorRpm(p_value);
			}
		}

		public int GetTargetStandingsIndex(int p_index)
		{
			int result = p_index;
			switch (game.type)
			{
			case GameFlag.Freestyle:
			case GameFlag.Replay:
				result = p_index;
				break;
			case GameFlag.Race:
			{
				RaceController raceController = (RaceController)game;
				if (p_index < 0)
				{
					result = 0;
					break;
				}
				if (p_index >= raceController.model.Rankings.Count)
				{
					result = 0;
					break;
				}
				GamePlayerData gamePlayerData = raceController.model.Rankings[p_index];
				GamePlayerData playerById = GetPlayerById(gamePlayerData.playerId);
				result = players.IndexOf(playerById);
				break;
			}
			}
			return result;
		}

		public float GetDroneCameraFOV(Drone p_drone, float p_default)
		{
			int num = drones.IndexOf(p_drone);
			if (num < 0)
			{
				return p_default;
			}
			if (isReplay)
			{
				ReplayClipPlayerModel replayClipPlayerModel = replays[num];
				if (ReplayFile.EnableVersion2)
				{
					return replayClipPlayerModel.clipV2.header.cameraFOV;
				}
				return replayClipPlayerModel.clip.header.Get("camera-fov", p_default);
			}
			return p_default;
		}

		public float GetDroneCameraTilt(Drone p_drone, float p_default)
		{
			int num = drones.IndexOf(p_drone);
			if (num < 0)
			{
				return p_default;
			}
			if (isReplay)
			{
				ReplayClipPlayerModel replayClipPlayerModel = replays[num];
				if (ReplayFile.EnableVersion2)
				{
					return replayClipPlayerModel.clipV2.header.cameraTilt;
				}
				return replayClipPlayerModel.clip.header.Get("camera-tilt", p_default);
			}
			DroneInputTransmitter droneInputTransmitter = transmitters[num];
			if (droneInputTransmitter == null)
			{
				return p_default;
			}
			if (droneInputTransmitter is DroneNetworkTransmitter)
			{
				DroneNetworkTransmitter droneNetworkTransmitter = (DroneNetworkTransmitter)droneInputTransmitter;
				if (droneNetworkTransmitter == null || droneNetworkTransmitter.Actor == null)
				{
					return p_default;
				}
				return droneNetworkTransmitter.Actor.CameraTilt;
			}
			if (droneInputTransmitter is DroneGhostTransmitter)
			{
				DroneGhostTransmitter droneGhostTransmitter = (DroneGhostTransmitter)droneInputTransmitter;
				if (droneGhostTransmitter == null || droneGhostTransmitter.data == null || droneGhostTransmitter.data.header == null)
				{
					return p_default;
				}
				return droneGhostTransmitter.data.header.Get("camera-tilt", p_default);
			}
			return p_default;
		}

		public void SetCameraMode(SpectateCameraModeType p_mode)
		{
			switch (p_mode)
			{
			case SpectateCameraModeType.None:
				return;
			case SpectateCameraModeType.Auto:
			case SpectateCameraModeType.Manual:
				if (cameraTools.Count <= 0)
				{
					return;
				}
				break;
			}
			if (p_mode != SpectateCameraModeType.None)
			{
				cameraMode = p_mode;
				Notify("spectate.camera-mode@change");
			}
		}

		public void SetCourseCameraActive(bool p_flag)
		{
			cameraCourseActive = p_flag;
			Notify("spectate.course-camera-mode@change");
		}

		public void ApplyCameraCourse(DroneCamera p_camera)
		{
			for (int i = 0; i < courseCameraActors.Count; i++)
			{
				courseCameraActors[i].auto = false;
			}
			if (cameraCourseActive)
			{
				DroneCamera obj = (p_camera ? p_camera : base.app.model.game.camera);
				obj.SetNone();
				obj.follow.target = null;
				SplineActor splineActor = courseCameraActors[courseCameraFocus];
				splineActor.auto = true;
				splineActor.snap = SplineActor.SnapMode.Start;
				splineActor.Snap();
				splineActor.auto = true;
			}
		}

		public void ApplyCameraMode(SpectateCameraModeType p_mode, DroneCamera p_camera, Drone p_drone)
		{
			DroneCamera c = (p_camera ? p_camera : base.app.model.game.camera);
			Drone d = p_drone;
			if (!c)
			{
				return;
			}
			Debug.Log("UISpectateModel> SetCameraMode - mode[" + p_mode.ToString() + "] drone[" + p_drone?.ToString() + "]");
			cameraCourseActive = false;
			ApplyCameraCourse(c);
			c.drone = null;
			c.wasd.joystickSensitivityMultiplier = 1f;
			switch (p_mode)
			{
			case SpectateCameraModeType.FPV:
				if ((bool)d)
				{
					float droneCameraFOV = GetDroneCameraFOV(d, default_fov);
					float droneCameraTilt = GetDroneCameraTilt(d, default_tilt);
					if (d.ready && (bool)d.body.frame.camera)
					{
						d.body.frame.camera.fov = droneCameraFOV;
						d.body.frame.camera.tilt = droneCameraTilt;
					}
					c.SetFPV(d);
					c.fov = droneCameraFOV;
					c.wasd.useJoystick = false;
					c.wasd.snapOnRelease = false;
					c.wasd.scrollStep = 0.5f;
					c.wasd.enabled = false;
					Debug.Log("UISpectateModel> SetCameraMode / FPV tilt[" + droneCameraTilt + "] fov[" + droneCameraFOV + "]");
				}
				break;
			case SpectateCameraModeType.Orbit:
				if ((bool)d)
				{
					c.SetTPVFree(d, 2f, 0.4f, 35f);
					c.orbit.angle = new Vector2(0f, 0f);
					c.orbit.speed.angle = 0.5f;
					c.follow.offset = new Vector3(0f, 0.025f, 0f);
					c.fov = 45f;
					c.wasd.useJoystick = false;
					c.wasd.snapOnRelease = true;
					c.wasd.scrollStep = 4f;
					c.wasd.orbitDragKey = KeyCode.Mouse0;
					c.wasd.sensitivity = 0.35f;
					c.wasd.joystickSensitivityMultiplier = 5f;
					Vector3 forward = d.transform.forward;
					forward.y = 0f;
					forward.Normalize();
					c.orbit.anchorRotation = Quaternion.LookRotation(forward, Vector3.up);
					c.orbit.StopTransition(OrbitTransform.TransitionMask.AnchorRotationMask);
					c.orbit.Snap(p_position: true, p_angle: false);
					this.TimerRunOnce(delegate
					{
						c.follow.target = d.transform;
						c.fov = 45f;
						c.orbit.Snap(p_position: true, p_angle: false);
					}, 1f / 30f);
				}
				break;
			case SpectateCameraModeType.FreeCamera:
				c.SetFreeCamera(p_reset_y: true);
				c.wasd.useJoystick = false;
				c.wasd.snapOnRelease = true;
				c.wasd.orbitDragKey = KeyCode.Mouse1;
				break;
			case SpectateCameraModeType.Auto:
			case SpectateCameraModeType.Manual:
				c.SetNone();
				c.follow.target = null;
				UpdateCameraTool(c, p_smooth: false);
				break;
			}
			RefreshDroneRendering();
			RefreshEffects();
		}

		protected void RefreshDroneRendering()
		{
			SpectateCameraModeType cmode = cameraMode;
			bool prop_visible = base.app.model.storage.state.player.settings.game.propsVisible;
			if (m_drone_rendering_call != null)
			{
				m_drone_rendering_call.Stop();
			}
			m_drone_rendering_call = this.TimerRunOnce(delegate
			{
				Drone drone = GetFocus<Drone>();
				bool shadowsOnly = cmode == SpectateCameraModeType.FPV;
				for (int i = 0; i < drones.Count; i++)
				{
					Drone drone2 = drones[i];
					if ((bool)drone2)
					{
						drone2.renderer.visible = true;
						if (drone2 == drone)
						{
							SpectateCameraModeType spectateCameraModeType = cmode;
							if ((uint)(spectateCameraModeType - 3) > 1u)
							{
								drone2.renderer.shadowsOnly = shadowsOnly;
							}
							drone2.renderer.propsVisible = prop_visible;
						}
						else
						{
							drone2.renderer.shadowsOnly = false;
							drone2.renderer.propsVisible = true;
						}
					}
				}
			}, 1f / 30f);
		}

		public void SetCourseCameras(List<MASpline> p_list)
		{
			courseCameras = new List<MASpline>();
			for (int i = 0; i < p_list.Count; i++)
			{
				MASpline mASpline = p_list[i];
				courseCameras.Add(mASpline);
				GameObject gameObject = (courseCameraActorTemplate ? UnityEngine.Object.Instantiate(courseCameraActorTemplate).gameObject : new GameObject("new-actor"));
				gameObject.name = $"spl-actor-{i}";
				gameObject.transform.SetParent(base.app.model.game.level.track.root.transform);
				SplineActor splineActor = gameObject.GetComponent<SplineActor>();
				if (!splineActor)
				{
					splineActor = gameObject.AddComponent<SplineActor>();
				}
				splineActor.spline = mASpline.spline;
				splineActor.smoothPosition = (splineActor.smoothRotation = 0f);
				splineActor.speed = mASpline.splineCourseCameraSpeed;
				splineActor.angularSpeed = 5f;
				splineActor.mode = SplineActor.Mode.PositionRotation;
				splineActor.orient = false;
				splineActor.wrap = WrapMode.Once;
				courseCameraActors.Add(splineActor);
			}
			Notify(1f / 60f, "spectate.course-cameras@change");
		}

		public void SetCameraTools(List<MACameraTool> p_list)
		{
			cameraTools = new List<MACameraTool>();
			for (int i = 0; i < p_list.Count; i++)
			{
				MACameraTool mACameraTool = p_list[i];
				if ((bool)mACameraTool.collider)
				{
					mACameraTool.SetIngame(p_flag: true);
					cameraTools.Add(mACameraTool);
				}
			}
			RefreshCameraToolTargetFocus();
			Notify(1f / 60f, "spectate.camera-tools@change");
		}

		public void SetCameraToolFocus(int p_index, MACameraTool p_tool)
		{
			if (p_index >= 0 && p_index < cameraToolTargetFocus.Count && (bool)p_tool)
			{
				cameraToolTargetFocus[p_index] = p_tool;
			}
		}

		public MACameraTool GetCameraToolFocus(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= cameraToolTargetFocus.Count)
			{
				return null;
			}
			return cameraToolTargetFocus[p_index];
		}

		public MACameraTool GetCameraToolFocus()
		{
			int num = cameraToolFocus;
			if (num < 0)
			{
				return null;
			}
			if (num >= cameraTools.Count)
			{
				return null;
			}
			return cameraTools[cameraToolFocus];
		}

		public int GetCameraToolIndex(MACameraTool p_target)
		{
			return cameraTools.IndexOf(p_target);
		}

		public void SetCameraToolFocus(Transform p_target, MACameraTool p_tool)
		{
			SetCameraToolFocus(targets.IndexOf(p_target), p_tool);
		}

		public void SetCameraToolFocus(Drone p_target, MACameraTool p_tool)
		{
			SetCameraToolFocus(drones.IndexOf(p_target), p_tool);
		}

		public void ResetCameraToolFocus(int p_index = -1)
		{
			int index = Mathf.Clamp(p_index, 0, cameraTools.Count - 1);
			for (int i = 0; i < targets.Count; i++)
			{
				MACameraTool p_tool = ((p_index < 0 || cameraTools.Count == 0) ? GetClosestCameraTool(targets[i]) : cameraTools[index]);
				SetCameraToolFocus(i, p_tool);
			}
			Notify("spectate.camera-tool.focus-list@change");
			SetCameraToolFocus(0);
		}

		public MACameraTool GetClosestCameraTool(Vector3 p_position)
		{
			MACameraTool result = null;
			if (cameraTools.Count <= 0)
			{
				return result;
			}
			result = cameraTools[0];
			float num = result.GetDistance(p_position);
			for (int i = 1; i < cameraTools.Count; i++)
			{
				MACameraTool mACameraTool = cameraTools[i];
				float distance = mACameraTool.GetDistance(p_position);
				if (!(distance >= num))
				{
					result = mACameraTool;
					num = distance;
				}
			}
			return result;
		}

		public MACameraTool GetClosestCameraTool(Transform p_target)
		{
			if (!p_target)
			{
				return null;
			}
			return GetClosestCameraTool(p_target.position);
		}

		public void ResetTargetRays()
		{
			for (int i = 0; i < targetRays.Count; i++)
			{
				Transform transform = targets[i];
				if ((bool)transform)
				{
					Ray value = new Ray(transform.position, transform.forward);
					targetRays[i] = value;
				}
			}
		}

		public void UpdateTargetCameraToolCheck(bool p_notify_focus_change)
		{
			bool flag = false;
			bool flag2 = false;
			int num = -1;
			int num2 = Mathf.Min(targets.Count, targetRays.Count);
			for (int i = 0; i < num2; i++)
			{
				Transform transform = targets[i];
				if (!transform)
				{
					continue;
				}
				Vector3 origin = targetRays[i].origin;
				Vector3 position = transform.position;
				float magnitude = (origin - position).magnitude;
				Ray ray = new Ray(position, origin - position);
				targetRays[i] = ray;
				MACameraTool mACameraTool = FindCameraToolByRay(ray, Mathf.Max(magnitude, 0.1f), 0);
				MACameraTool mACameraTool2 = GetCameraToolFocus(i);
				if ((bool)mACameraTool && !(mACameraTool == mACameraTool2))
				{
					bool num3 = transform == m_focus_target;
					flag = true;
					SetCameraToolFocus(i, mACameraTool);
					if (num3)
					{
						flag2 = true;
						num = cameraTools.IndexOf(mACameraTool);
					}
				}
			}
			if (flag)
			{
				Notify("spectate.camera-tool.focus-list@change");
			}
			if (flag2 && p_notify_focus_change)
			{
				SetCameraToolFocus(num);
			}
		}

		public void SetCourseCameraFocus(int p_index)
		{
			courseCameraFocus = p_index;
			Notify("spectate.course-camera.focus@change", p_index);
		}

		public void SetCameraToolFocus(int p_index)
		{
			if (cameraToolFocus != p_index && p_index >= 0 && p_index < cameraTools.Count)
			{
				cameraToolFocus = p_index;
				SetCameraToolFocus(GetFocus<Transform>(), cameraTools[p_index]);
				Notify("spectate.camera-tool.focus@change", p_index);
			}
		}

		public void SetCameraToolFocus(MACameraTool p_camera_tool)
		{
			SetCameraToolFocus(cameraTools.IndexOf(p_camera_tool));
		}

		public MACameraTool FindCameraToolByRay(Ray p_ray, float p_distance, int p_variation_index)
		{
			if (m_find_cameratool_results == null)
			{
				m_find_cameratool_results = new List<MACameraTool>();
			}
			m_find_cameratool_results.Clear();
			Vector3 origin = p_ray.origin;
			Vector3 p_world_position = origin + p_ray.direction * p_distance;
			for (int i = 0; i < cameraTools.Count; i++)
			{
				MACameraTool mACameraTool = cameraTools[i];
				bool flag = false;
				if (mACameraTool.collider.IsInside(origin))
				{
					flag = true;
				}
				else if (mACameraTool.collider.IsInside(p_world_position))
				{
					flag = true;
				}
				else if (mACameraTool.collider.Raycast(p_ray, p_distance))
				{
					flag = true;
				}
				if (flag)
				{
					m_find_cameratool_results.Add(mACameraTool);
				}
			}
			if (m_find_cameratool_results.Count > 0)
			{
				return m_find_cameratool_results[p_variation_index % m_find_cameratool_results.Count];
			}
			return null;
		}

		public MACameraTool FindCameraToolByPosition(Vector3 p_point)
		{
			for (int i = 0; i < cameraTools.Count; i++)
			{
				MACameraTool mACameraTool = cameraTools[i];
				if (mACameraTool.collider.IsInside(p_point))
				{
					return mACameraTool;
				}
			}
			return null;
		}

		public void UpdateCameraTool(DroneCamera p_camera, bool p_smooth)
		{
			if (!p_camera || cameraTools.Count <= 0)
			{
				return;
			}
			MACameraTool mACameraTool = GetCameraToolFocus();
			if (!mACameraTool)
			{
				return;
			}
			Drone focus_drone = m_focus_drone;
			Transform focus_target = m_focus_target;
			PlayerStateModel player = base.app.model.storage.state.player;
			bool propsVisible = player.settings.game.propsVisible;
			bool flag = false;
			bool flag2 = player.settings.graphics.radioFx && player.settings.game.radioNoise;
			Component component = (focus_drone ? ((Component)focus_drone) : ((Component)focus_target));
			if (!component || cameraCourseActive)
			{
				return;
			}
			SpectateCameraModeType spectateCameraModeType = cameraMode;
			if ((uint)spectateCameraModeType > 2u && (uint)(spectateCameraModeType - 3) <= 1u)
			{
				CameraToolTrackingMode controlPointTrackingMode = mACameraTool.GetControlPointTrackingMode(0);
				Vector3 position = focus_target.transform.position;
				float p_ratio = Mathf.Clamp01(mACameraTool.GetProjectionRatio(position));
				float value = mACameraTool.GetControlPointTrackingDelayLerp(p_ratio) + defaultSmoothing;
				value = Mathf.Clamp(value, 0f, 1f);
				float t = ((value <= 0f || !p_smooth) ? 1f : (Time.deltaTime / value));
				position = (m_next_position = Vector3.Lerp(m_next_position, position, t));
				p_ratio = Mathf.Clamp01(mACameraTool.GetProjectionRatio(position));
				p_ratio = Mathf.Clamp01(mACameraTool.GetProjectionRatio(position, mACameraTool.GetControlPointOffsetLerp(p_ratio).z));
				p_ratio = mACameraTool.easing.curve.Evaluate(p_ratio);
				TransformVector controlPointSampleLerp = mACameraTool.GetControlPointSampleLerp(component, position, p_ratio);
				float fov = mACameraTool.GetControlPointFOVLerp(p_ratio);
				if (controlPointTrackingMode == CameraToolTrackingMode.FPV)
				{
					float droneCameraTilt = GetDroneCameraTilt(focus_drone, default_tilt);
					focus_drone.body.frame.camera.tilt = droneCameraTilt;
					fov = GetDroneCameraFOV(focus_drone, default_fov);
					focus_drone.renderer.shadowsOnly = true;
					focus_drone.renderer.propsVisible = propsVisible;
					flag = true;
				}
				else
				{
					focus_drone.renderer.shadowsOnly = false;
					focus_drone.renderer.propsVisible = true;
				}
				p_camera.fx.radioEnabled = flag && flag2;
				p_camera.transform.position = controlPointSampleLerp.position;
				p_camera.transform.localRotation = controlPointSampleLerp.rotation;
				p_camera.fov = fov;
			}
		}

		public void UpdateCourseCamera(DroneCamera p_camera)
		{
			if (cameraCourseActive && (bool)p_camera && courseCameraActors.Count > 0)
			{
				MASpline mASpline = courseCameras[courseCameraFocus];
				SplineActor splineActor = courseCameraActors[courseCameraFocus];
				p_camera.fx.radioEnabled = false;
				p_camera.transform.position = splineActor.transform.position;
				p_camera.transform.localRotation = splineActor.transform.localRotation;
				p_camera.fov = mASpline.splineCourseCameraFOV;
			}
		}

		protected void RefreshCameraToolTargetFocus()
		{
			List<MACameraTool> list = new List<MACameraTool>();
			for (int i = 0; i < targets.Count; i++)
			{
				if (i >= cameraToolTargetFocus.Count)
				{
					list.Add(null);
				}
				else
				{
					list.Add(cameraToolTargetFocus[i]);
				}
			}
			cameraToolTargetFocus = list;
		}

		public void SetDroneTrailMode(SpectateDroneTrailModeType p_mode)
		{
			trailMode = p_mode;
			RefreshEffects();
			Notify("spectate.drone-trail-mode@change");
		}

		public void SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType p_mode)
		{
			trailWidthMode = p_mode;
			RefreshEffects();
			Notify("spectate.drone-trail-width-mode@change");
		}

		public void RefreshEffects()
		{
			int num = 0;
			int num2 = 0;
			switch (cameraMode)
			{
			case SpectateCameraModeType.FPV:
				num = 1;
				break;
			case SpectateCameraModeType.Orbit:
				num = 1;
				break;
			case SpectateCameraModeType.FreeCamera:
				num = 1;
				break;
			case SpectateCameraModeType.Auto:
			case SpectateCameraModeType.Manual:
			{
				num = 1;
				MACameraTool mACameraTool = GetCameraToolFocus();
				if ((bool)mACameraTool && mACameraTool.GetControlPointTrackingMode(0) == CameraToolTrackingMode.FPV)
				{
					num = 1;
				}
				break;
			}
			}
			switch (trailMode)
			{
			case SpectateDroneTrailModeType.Off:
				num = 0;
				break;
			case SpectateDroneTrailModeType.Small:
				num = 1;
				break;
			case SpectateDroneTrailModeType.Medium:
				num = 2;
				break;
			case SpectateDroneTrailModeType.Large:
				num = 3;
				break;
			}
			switch (trailWidthMode)
			{
			case SpectateDroneTrailWidthModeType.Small:
				num2 = 1;
				break;
			case SpectateDroneTrailWidthModeType.Medium:
				num2 = 2;
				break;
			case SpectateDroneTrailWidthModeType.Large:
				num2 = 3;
				break;
			}
			float droneTrailDuration = trail_durations[num];
			float droneTrailWidthMultiplier = trail_width_multipliers[num2];
			SetDroneTrailDuration(droneTrailDuration);
			SetDroneTrailWidthMultiplier(droneTrailWidthMultiplier);
			GetFocus<Drone>();
			DroneCamera camera = base.app.model.game.camera;
			bool motionBlurEnabled = false;
			bool motionBlur = base.app.model.storage.state.player.settings.graphics.motionBlur;
			switch (cameraMode)
			{
			case SpectateCameraModeType.FPV:
				motionBlurEnabled = motionBlur;
				break;
			case SpectateCameraModeType.Orbit:
				motionBlurEnabled = false;
				break;
			case SpectateCameraModeType.FreeCamera:
				motionBlurEnabled = false;
				break;
			case SpectateCameraModeType.Auto:
			case SpectateCameraModeType.Manual:
			{
				MACameraTool mACameraTool2 = GetCameraToolFocus();
				if ((bool)mACameraTool2 && mACameraTool2.GetControlPointTrackingMode(0) == CameraToolTrackingMode.FPV)
				{
					motionBlurEnabled = motionBlur;
				}
				break;
			}
			}
			if (cameraCourseActive)
			{
				motionBlurEnabled = false;
			}
			if ((bool)camera && (bool)camera.fx)
			{
				camera.fx.SetMotionBlurEnabled(motionBlurEnabled);
			}
		}

		private int SortReplayClips(ReplayClipPlayerModel a, ReplayClipPlayerModel b)
		{
			if (a.player.order >= b.player.order)
			{
				return 1;
			}
			return -1;
		}

		private bool PruneReplayClips(ReplayClipPlayerModel a)
		{
			return a.player == null;
		}
	}
}
