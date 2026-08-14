using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameController : Controller<DRLApp>
	{
		private GameModel m_model;

		private GameReplayController m_replay;

		private List<string> m_ignoreCameraDisableScreens = new List<string> { "game-spectate-screen", "map-editor-screen" };

		[Range(0f, 1f)]
		[SerializeField]
		private float m_timescale;

		private Activity m_signal_lost_rearm;

		private GameCameraMode m_lastCameraMode = GameCameraMode._InGame__;

		private bool m_has_exited;

		private bool m_restart_lock;

		private Activity m_peek_timer;

		private bool m_delayResetting;

		private bool m_scrapeAudioPlaying;

		public GameModel model
		{
			get
			{
				if (!m_model)
				{
					return m_model = AssertLocal<GameModel>("model");
				}
				return m_model;
			}
		}

		public GameInputController input => AssertFind<GameInputController>("input");

		public LevelController level => AssertFind<LevelController>("level");

		public GameReplayController replay
		{
			get
			{
				if (!m_replay)
				{
					return m_replay = AssertFind<GameReplayController>("replay");
				}
				return m_replay;
			}
		}

		public RaceController race => AssertFind<RaceController>("modes/race");

		public RaceController networkRace => AssertFind<RaceController>("modes/network-race");

		public CircuitController circuit => AssertFind<CircuitController>("modes/circuits");

		public CampaignController campaign => AssertFind<CampaignController>("modes/campaign");

		public GameCollectableController collectable => AssertFind<GameCollectableController>("modes/collectables");

		public GameTypeController activeGameMode
		{
			get
			{
				if ((bool)networkRace)
				{
					return networkRace;
				}
				if ((bool)circuit)
				{
					return circuit;
				}
				if ((bool)campaign)
				{
					return campaign;
				}
				if ((bool)race)
				{
					return race;
				}
				if ((bool)collectable)
				{
					return collectable;
				}
				return null;
			}
		}

		public UIGame ui => base.app.view.ui.game;

		public GameAudioController audio => Assert<GameAudioController>("audio");

		public GameEffectView effects => Assert<GameEffectView>("effects");

		public float timescale
		{
			get
			{
				return m_timescale;
			}
			set
			{
				m_timescale = value;
				SetTimescale(m_timescale);
			}
		}

		public bool HasExited => m_has_exited;

		protected void Awake()
		{
			Transform transform = Find<Transform>("modes");
			foreach (GameTypeController mode3 in model.modes)
			{
				string text = mode3.name;
				GameFlag type = mode3.type;
				GameFlag mode = mode3.mode;
				GameFlag type2 = base.app.arguments.game.type;
				GameFlag mode2 = base.app.arguments.game.mode;
				bool num = type == GameFlag.None || type == type2;
				bool flag = mode == GameFlag.None || mode == mode2;
				Transform transform2 = transform.Find(text);
				if (!num || !flag)
				{
					if ((bool)transform2)
					{
						transform2.gameObject.SetActive(value: false);
					}
				}
				else if (!transform2)
				{
					Debug.Log("GameController> Awake / Create Game Mode - name[" + text + "]");
					GameObject obj = UnityEngine.Object.Instantiate(mode3.gameObject);
					obj.name = text;
					obj.transform.parent = transform;
				}
			}
		}

		public T GetMode<T>() where T : GameTypeController
		{
			Transform transform = base.transform.Find("modes");
			if (!transform)
			{
				return null;
			}
			return Hierarchy.Find<T>(transform);
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!base.validContext || !base.enabled || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.start":
				DRLApp.LogMemStats("GameController> Scene Start", p_show_delta: true);
				m_has_exited = false;
				base.app.view.ui.fade.transition = 0f;
				base.app.view.ui.game.hud.transition = -0.1f;
				base.app.view.ui.navigation.enabled = false;
				model.Set(base.app.arguments);
				base.app.view.ui.game.hud.gameTitle.Clear();
				base.app.view.ui.game.hud.transition = 0f;
				base.app.view.ui.footer.Hide(0f);
				base.app.controller.RefreshFooter();
				base.app.view.ui.footer.SetColors(p_ingame: true);
				base.app.view.ui.COXLogo.SetActive(base.app.arguments.game.mapGUID == "MP-409");
				DRLApp.LogMemStats("GameController> Scene Start Complete!", p_show_delta: true);
				break;
			case "game.ready":
				if ((bool)base.app.acs)
				{
					base.app.acs.guiAllowed = base.app.model.storage.state.player.profile.isDeveloper;
				}
				base.app.view.ui.fade.FadeOut(1.5f, 0.5f);
				if ((bool)model.camera && (bool)model.camera.main)
				{
					if (!model.camera.GetComponent<AudioListener>())
					{
						model.camera.gameObject.AddComponent<AudioListener>();
					}
					DroneCamera camera = model.camera;
					camera.CameraModeChanged = (Action<DroneCameraModeType>)Delegate.Remove(camera.CameraModeChanged, new Action<DroneCameraModeType>(OnCameraModeChanged));
					DroneCamera camera2 = model.camera;
					camera2.CameraModeChanged = (Action<DroneCameraModeType>)Delegate.Combine(camera2.CameraModeChanged, new Action<DroneCameraModeType>(OnCameraModeChanged));
				}
				base.app.view.audio.MuteFadeIn(1f, 2.5f);
				base.app.view.audio.PlayMusicGame();
				base.app.view.audio.PlayEnvWind();
				replay.recorder.model.Initialize();
				timescale = 1f;
				DRLApp.LogMemStats("GameController> Game Initialized", p_show_delta: true);
				break;
			case "missions.mission-overview.start@click":
			{
				Debug.Log("GameController> MissionOverview / Start");
				UIMissionOverviewView open = base.app.view.ui.screens.manager.GetOpen<UIMissionOverviewView>();
				if ((bool)open)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIStartGame();
					StartMission(open.quest, open.mission);
				}
				break;
			}
			case "fly.circuits-overview.ready":
			case "fly.map-track-overview.ready":
				if (p_data.Length == 0 || !(p_data[0] is MapLoadData mapLoadData) || mapLoadData.baseMap == null)
				{
					break;
				}
				if (mapLoadData.baseTrack == null)
				{
					mapLoadData.baseTrack = base.app.model.storage.GetMapTracks(mapLoadData.baseMap, GameFlag.Freestyle)[0];
				}
				if (model.playerDrone != null)
				{
					Notify("game.simulation.drone.flight-time@update", model.playerDrone.rig);
				}
				base.enabled = false;
				if (mapLoadData.opponentRecord == null)
				{
					base.app.model.service.opponent.ghostRecords = null;
				}
				if (mapLoadData.opponentRecordV2 == null)
				{
					if (base.app.model.service.opponent.ghostRecordsV2 != null)
					{
						base.app.model.service.opponent.ghostRecordsV2.Destroy();
					}
					base.app.model.service.opponent.ghostRecordsV2 = null;
				}
				base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
				base.app.arguments.game.allowCrash = false;
				base.app.arguments.game.opponentType = mapLoadData.opponentMode;
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				if (ReplayFile.EnableVersion2)
				{
					base.app.arguments.game.AddGhostPlayer(mapLoadData.opponentRecordV2);
				}
				else
				{
					base.app.arguments.game.AddGhostPlayer(mapLoadData.opponentRecord);
				}
				base.app.arguments.game.podium = mapLoadData.baseTrack.podium;
				if (base.app.arguments.game.type == GameFlag.Campaign)
				{
					DRLCampaign dRLCampaign = base.app.arguments.game.campaign;
					if ((bool)dRLCampaign && !string.IsNullOrEmpty(dRLCampaign.podium))
					{
						base.app.arguments.game.podium = dRLCampaign.podium;
					}
				}
				StartMap(mapLoadData.baseMap, mapLoadData.baseTrack, mapLoadData.customMap);
				break;
			case "game.drone.signal-full":
				ui.hud.lowSignalWarning.FadeOut(0.2f);
				ui.hud.lowSignalWarning.pulse = false;
				break;
			case "game.drone.signal-drop":
				if (!base.app.model.storage.state.player.settings.graphics.radioFx)
				{
					float num = (float)p_data[2];
					_ = (float)p_data[3];
					if (num < 0.5f)
					{
						ui.hud.lowSignalWarning.FadeIn(0.2f);
						ui.hud.lowSignalWarning.pulse = true;
					}
				}
				break;
			case "game.drone.signal-lost":
			{
				Drone d = model.playerDrone;
				DroneCamera c = model.camera;
				if (!d)
				{
					break;
				}
				Color ec = d.renderer.emissive;
				Color tc = d.renderer.trailsColor;
				d.fc.armed = false;
				d.renderer.emissive = Color.black;
				d.renderer.shadowsOnly = false;
				d.SetMotorSpinSpeed(0.2f, 1f);
				c.npsnap.enabled = false;
				float dist = c.orbit.distance;
				float min_dist = c.orbit.constraint.distanceMin;
				float max_dist = c.orbit.constraint.distanceMax;
				Vector2 max_angle = c.orbit.constraint.angleMax;
				Vector2 min_angle = c.orbit.constraint.angleMin;
				Vector2 angle = c.orbit.angle;
				float fov = c.fov;
				float np = c.main.nearClipPlane;
				Quaternion anchor_rotation = c.orbit.anchorRotation;
				c.orbit.constraint.Clear();
				float num2 = 2.45f;
				c.main.nearClipPlane = 0.01f;
				Tween.Add(c.orbit, "distance", 1.2f, num2 * 0.5f, 0f, Cubic.Out);
				Tween.Add(c.orbit, "angle", angle + new Vector2(-45f, 35f), num2, 0f, Cubic.Out);
				Tween.Add(c, "fov", 40f, num2, 0f, Cubic.Out);
				if (m_signal_lost_rearm != null)
				{
					m_signal_lost_rearm.Stop();
				}
				m_signal_lost_rearm = Timer.Set(d.fc, "armed", 3.5f, true);
				RunOnce(delegate
				{
					d.renderer.emissive = ec;
					d.renderer.trailsColor = tc;
					d.SetMotorSpinSpeed(1f);
					Tween.Kill(c.orbit);
					Tween.Kill(c.orbit, "angle");
					Tween.Kill(c);
					c.main.nearClipPlane = np;
					c.npsnap.enabled = true;
					c.orbit.angle = angle;
					c.orbit.constraint.distanceMax = max_dist;
					c.orbit.constraint.distanceMin = min_dist;
					c.orbit.constraint.distanceMax = max_dist;
					c.orbit.constraint.angleMax = max_angle;
					c.orbit.constraint.angleMin = min_angle;
					c.orbit.distance = dist;
					c.fov = fov;
					c.orbit.anchorRotation = anchor_rotation;
					if (Mathf.Abs(c.orbit.distance) < 0.2f)
					{
						d.renderer.shadowsOnly = true;
					}
					PodiumReset(d);
				}, 2.5f);
				break;
			}
			case "ui.screen@open":
			{
				if (!model.simulation)
				{
					break;
				}
				FadeBlur(1f, 0.8f, 1f / 12f);
				base.app.view.ui.navigation.keepFocus = true;
				if (!base.app.view.ui.game.preventFooter)
				{
					base.app.view.ui.footer.Show(0.5f, 0.5f);
				}
				else
				{
					base.app.view.ui.footer.Hide(0.5f, 0.25f);
				}
				base.app.view.ui.game.preventFooter = false;
				UIScreen uIScreen = p_data[0] as UIScreen;
				if (uIScreen == null || m_ignoreCameraDisableScreens.Contains(uIScreen.name))
				{
					break;
				}
				this.TimerRunOnce(delegate
				{
					if (base.validContext && !(base.app.view.ui.screens.current == null))
					{
						model.camera.SetGameCameraEnabled(p_flag: false);
					}
				}, 0.8f);
				break;
			}
			case "ui.screen@close":
			{
				if (!model.simulation)
				{
					break;
				}
				UIScreen scr = p_data[0] as UIScreen;
				Activity.RunOnce(delegate
				{
					int count = base.app.view.ui.screens.manager.count;
					float p_value = ((count <= 0) ? 0f : 1f);
					if (base.app.view.ui.game.preventFooter)
					{
						p_value = 0f;
					}
					FadeBlur(p_value, 0.8f);
					model.camera.SetGameCameraEnabled(p_flag: true);
					if ((bool)scr && scr.name == "game-pause-screen")
					{
						base.app.view.ui.footer.Hide(0.25f);
					}
					base.app.view.ui.navigation.keepFocus = count > 0;
					if (!base.app.view.ui.navigation.keepFocus)
					{
						base.app.view.ui.navigation.focus = null;
					}
				}, 1f / 30f);
				break;
			}
			case "leaderboards.replay.load@complete":
				base.app.view.audio.SceneMainToGame(1.6f);
				break;
			case "garage.edit.fly.ready":
				if (base.validContext)
				{
					GameModel game = base.app.model.game;
					if ((bool)game && game.playerData == null && p_data.Length != 0)
					{
						base.app.model.game.playerData.rig = (DroneRigData)p_data[0];
					}
				}
				break;
			}
		}

		public void Exit()
		{
			if (!m_has_exited)
			{
				m_has_exited = true;
				base.app.arguments.game.Clear();
				replay.recorder.model.Clear();
				model.players.Clear();
				base.app.model.service.opponent.ClearGhosts();
				if (model.simulation != null)
				{
					model.simulation.transmitters.RemoveGhostDrones();
				}
				audio.ClearDroneAudioList();
				SetGCEnabled(p_flag: true);
				base.app.scene.ExitGame();
				base.app.view.audio.SceneGameToMain();
			}
		}

		public void BackToEditor()
		{
			MapData data = model.level.data.data;
			DRLAppArguments arguments = base.app.arguments;
			arguments.game.editor = false;
			arguments.game.type = GameFlag.MapEditor;
			arguments.game.mode = GameFlag.SinglePlayer;
			arguments.game.map = model.level.data;
			arguments.game.map.data = data;
			arguments.game.track = model.level.track.data;
			base.app.view.audio.StopAllGameAudio();
			Activity.RunOnce(base.app.scene.Load, 0.2f);
		}

		public void Restart(DRLMap p_map, DRLMapTrack p_track)
		{
			if (m_restart_lock)
			{
				Debug.LogWarning("GameController> Restart / Multiple Calls Warning!");
				return;
			}
			m_restart_lock = true;
			DRLAppArguments arguments = base.app.arguments;
			arguments.game.type = model.type;
			arguments.game.mode = model.mode;
			arguments.game.map = (p_map ? p_map : model.level.data);
			arguments.game.track = (p_track ? p_track : model.level.track.data);
			arguments.game.fcMode = model.fcMode;
			arguments.game.allowCrash = model.allowCrash;
			arguments.game.promo = model.tournament;
			if (model.type == GameFlag.Mission)
			{
				MissionModel missionModel = Hierarchy.Find<MissionModel>(base.transform);
				arguments.game.mission = (missionModel ? missionModel.mission : null);
				arguments.game.quest = (missionModel ? missionModel.quest : null);
				arguments.game.track = ((!missionModel) ? null : (missionModel.mission.track ? missionModel.mission.track : null));
			}
			base.app.view.audio.StopAllGameAudio();
			Notify("game.restart");
			Activity.RunOnce(base.app.scene.Load, 0.2f);
		}

		public void RestartWithoutLoad()
		{
			DRLAppArguments arguments = base.app.arguments;
			base.app.view.audio.StopAllGameAudio();
			Activity.RunOnce(base.app.scene.ExitGame, 0.2f);
			arguments.game.type = model.type;
			arguments.game.mode = model.mode;
			arguments.game.map = model.level.data;
			arguments.game.track = model.level.track.data;
			arguments.game.fcMode = model.fcMode;
			arguments.game.allowCrash = model.allowCrash;
			arguments.game.promo = model.tournament;
			if (model.type == GameFlag.Mission)
			{
				MissionModel missionModel = Hierarchy.Find<MissionModel>(base.transform);
				arguments.game.mission = (missionModel ? missionModel.mission : null);
				arguments.game.quest = (missionModel ? missionModel.quest : null);
				arguments.game.track = ((!missionModel) ? null : (missionModel.mission.track ? missionModel.mission.track : null));
			}
		}

		public void Restart()
		{
			Restart(null, null);
		}

		public void StartMap(DRLMap p_map, DRLMapTrack p_track, MapData p_customData = null)
		{
			DRLAppArguments arguments = base.app.arguments;
			arguments.game.type = model.type;
			arguments.game.mode = model.mode;
			arguments.game.map = p_map;
			arguments.game.track = p_track;
			arguments.game.fcMode = model.fcMode;
			arguments.game.allowCrash = model.allowCrash;
			arguments.game.promo = model.tournament;
			base.app.view.audio.StopAllGameAudio();
			if (arguments.game.type == GameFlag.Race && arguments.game.opponentType == OpponentModeType.Off)
			{
				base.app.model?.service?.opponent?.ForceResetLoadedReplays();
				RunOnce(2f, delegate
				{
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				});
			}
			GameObject gameObject = GameObject.Find("dr-background");
			if (gameObject != null)
			{
				RawImage component = gameObject.GetComponent<RawImage>();
				if (component != null && component.texture != null && component.texture is RenderTexture renderTexture)
				{
					renderTexture.DiscardContents();
					RenderTexture.ReleaseTemporary(renderTexture);
					RenderTexture renderTexture2 = null;
				}
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			if (p_customData != null)
			{
				base.app.scene.LoadCommunityMap(p_customData.guid, 3f, delegate
				{
					base.app.view.audio.PlayUIStartGame();
					base.app.view.ui.fade.FadeIn(1.5f);
				}, p_customData.version);
			}
			else
			{
				base.app.view.audio.PlayUIStartGame();
				base.app.view.ui.fade.FadeIn(1.5f);
				Activity.RunOnce(base.app.scene.Load, 3f);
			}
			base.app.view.ui.navigation.enabled = false;
		}

		public void PodiumReset(Drone p_drone, int p_index = -1, bool p_force_podium = false, bool p_recover = true)
		{
			if (!p_drone)
			{
				return;
			}
			DroneSimulation simulation = model.simulation;
			if ((bool)simulation)
			{
				ToggleAutoSyncTransforms();
				GamePlayerData playerData = model.GetPlayerData(p_drone);
				int siblingIndex = p_drone.transform.GetSiblingIndex();
				int p_index2 = ((p_index >= 0) ? p_index : (playerData?.order ?? siblingIndex));
				simulation.PlaceDrone(p_drone, p_index2, p_force_podium, p_recover);
				p_drone.ClearForces();
				if ((bool)p_drone.fc)
				{
					p_drone.fc.Reset();
				}
				if (p_drone.physics != null && p_drone.physics.aerodynamics != null)
				{
					p_drone.physics.aerodynamics.Reset();
				}
				p_drone.renderer.ClearTrails();
				if (model.IsPlayer(p_drone) && !p_force_podium)
				{
					base.app.view.audio.PlayDroneRespawn(p_drone.gameObject);
				}
				base.app.view.ui.game.hud.damage.Show(p_drone.crashEnabled);
			}
		}

		public void PodiumResetAll()
		{
			foreach (Drone item in model.simulation.drones.list)
			{
				if ((bool)item)
				{
					PodiumReset(item, -1, p_force_podium: true);
				}
			}
		}

		public void DroneArmDisarm(Drone p_drone)
		{
			if ((bool)p_drone)
			{
				p_drone.fc.armed = !p_drone.fc.armed;
				if (p_drone.fc.armed)
				{
					Notify("game.simulation.arm-and-turtle@armed");
				}
				else
				{
					Notify("game.simulation.arm-and-turtle@disarmed");
				}
			}
		}

		public void DroneTurtle(Drone p_drone)
		{
			if ((bool)p_drone && !p_drone.fc.armed)
			{
				p_drone.fc.turtle = !p_drone.fc.turtle;
			}
		}

		public void DroneResetArmAndTurtle(Drone p_drone)
		{
			if ((bool)p_drone)
			{
				p_drone.fc.armed = true;
				p_drone.fc.turtle = false;
			}
		}

		public void ToggleAutoSyncTransforms()
		{
			Physics.autoSyncTransforms = true;
			Physics.SyncTransforms();
			Activity.RunOnce(delegate
			{
				Physics.autoSyncTransforms = false;
			}, 0.1f);
		}

		public bool DroneReset(Drone p_drone, bool p_snapToPath = false)
		{
			Drone drone = p_drone;
			if (!drone)
			{
				return false;
			}
			if (Time.time < drone.lastResetTime + 2f)
			{
				return false;
			}
			if (drone.fc.sensor.inertial.actualSpeed > 13.89f)
			{
				return false;
			}
			ToggleAutoSyncTransforms();
			drone.renderer.ClearTrails();
			SpawnPoint spawnPoint = new SpawnPoint(drone.position, drone.transform.rotation);
			RaceController raceController = ((race != null && race.isActiveAndEnabled) ? race : ((networkRace != null && networkRace.isActiveAndEnabled) ? networkRace : campaign));
			int num = (raceController ? raceController.model.GetProgress(drone) : (-1));
			SplineTracerComponent pathTrace = model.level.track.pathTrace;
			bool flag = (bool)pathTrace && (bool)pathTrace.spline;
			if (p_snapToPath)
			{
				if ((race != null && race.isActiveAndEnabled) || (campaign != null && campaign.isActiveAndEnabled) || (networkRace != null && networkRace.isActiveAndEnabled))
				{
					float droneResetDelay = base.app.model.storage.state.player.profile.droneResetDelay;
					bool flag2 = droneResetDelay > 0f;
					if (flag2 && m_delayResetting)
					{
						return true;
					}
					if (flag2)
					{
						m_delayResetting = true;
						base.app.model.game.simulation.SetDroneTransmitter(p_drone, p_active: false);
						this.TimerRunOnce(delegate
						{
							base.app.model.game.simulation.SetDroneTransmitter(p_drone);
							m_delayResetting = false;
						}, droneResetDelay);
					}
					if (num < 1)
					{
						if ((bool)model.simulation.podiums.Get(drone.transform.GetSiblingIndex()))
						{
							PodiumReset(drone, -1, p_force_podium: false, p_recover: false);
							return true;
						}
					}
					else if (num - 1 < raceController.model.gates.Count)
					{
						ColliderEventComponent colliderEventComponent = raceController.model.gates[num - 1];
						MAGate mAGate = Hierarchy.FindReverse<MAGate>(colliderEventComponent.transform);
						MAGuide mAGuide = null;
						if (mAGate != null)
						{
							mAGuide = mAGate.GetRespawnGuide();
						}
						if ((bool)mAGuide)
						{
							Vector3 vector = mAGuide.transform.position;
							if (Physics.Raycast(new Ray(vector + Vector3.up * 0.2f, Vector3.down), out var hitInfo, 5f, DRLPhysics.Layers.Raycast_IgnoreDrone))
							{
								vector = hitInfo.point;
							}
							spawnPoint = new SpawnPoint(vector, mAGuide.transform.rotation);
							if (num < raceController.model.gates.Count && raceController.model.gates[num] != null)
							{
								spawnPoint.rotation = Quaternion.LookRotation(raceController.model.gates[num].transform.position - spawnPoint.position, Vector3.up);
							}
						}
						else
						{
							Transform transform = raceController.model.gates[num].transform;
							Vector3 position = (colliderEventComponent ? colliderEventComponent.transform : drone.transform).position;
							TransformVector transformVector = ((!pathTrace) ? new TransformVector(position) : (pathTrace.spline ? pathTrace.GetClosestSample(position, 4) : new TransformVector(position)));
							Vector3 p_towards = (transform ? transform.position : (transformVector.position + transformVector.forward));
							spawnPoint = SpawnPoint.FindSafeSpot(transformVector.position, p_towards, DRLPhysics.Layers.Raycast_IgnoreDrone);
						}
					}
					else
					{
						spawnPoint = SpawnPoint.FindSafeSpot(drone.position, drone.transform, DRLPhysics.Layers.Raycast_IgnoreDrone);
					}
				}
				else if (flag)
				{
					ColliderEventComponent colliderEventComponent2 = ((num < 0) ? null : ((num >= raceController.model.gates.Count) ? null : raceController.model.gates[num]));
					Vector3 position2 = (colliderEventComponent2 ? colliderEventComponent2.transform : drone.transform).position;
					TransformVector transformVector2 = ((!pathTrace) ? new TransformVector(position2) : (pathTrace.spline ? pathTrace.GetClosestSample(position2, -4) : new TransformVector(position2)));
					Vector3 p_towards2 = (colliderEventComponent2 ? position2 : (transformVector2.position + transformVector2.forward));
					spawnPoint = SpawnPoint.FindSafeSpot(transformVector2.position, p_towards2, DRLPhysics.Layers.Raycast_IgnoreDrone);
				}
				else
				{
					spawnPoint = SpawnPoint.FindSafeSpot(drone.position, drone.transform, DRLPhysics.Layers.Raycast_IgnoreDrone);
				}
			}
			else
			{
				spawnPoint = SpawnPoint.FindSafeSpot(drone.position, drone.transform, DRLPhysics.Layers.Raycast_IgnoreDrone);
			}
			base.app.view.audio.PlayDroneFlipped(drone.gameObject);
			DronePodium dronePodium = model.simulation.podiums.Get(drone.transform.GetSiblingIndex());
			if ((bool)dronePodium && (dronePodium.spawn.position - spawnPoint.position).magnitude < 2f)
			{
				PodiumReset(drone, -1, p_force_podium: false, p_recover: false);
				return true;
			}
			if (p_snapToPath)
			{
				drone.transform.localRotation = spawnPoint.rotation;
			}
			drone.ResetPosition(spawnPoint.position);
			return true;
		}

		public void ClearAllActivities()
		{
			if (m_signal_lost_rearm != null)
			{
				m_signal_lost_rearm.Stop();
			}
		}

		public void StartMission(DRLQuest p_quest, DRLMission p_mission)
		{
			Debug.Log("GameController> StartMission");
			if (!p_mission)
			{
				Debug.LogWarning("GameController> Invalid Mission data!");
				base.app.scene.LoadMain();
				return;
			}
			if (!p_quest)
			{
				Debug.LogWarning("GameController> Invalid Quest data!");
				base.app.scene.LoadMain();
				return;
			}
			DRLAppArguments arguments = base.app.arguments;
			arguments.game.type = GameFlag.Mission;
			arguments.game.mode = GameFlag.SinglePlayer;
			arguments.game.mission = p_mission;
			arguments.game.quest = p_quest;
			arguments.game.fcMode = FCMode.None;
			arguments.game.allowCrash = false;
			base.app.view.audio.StopAllGameAudio();
			Activity.RunOnce(base.app.scene.Load, 0.2f);
		}

		public void NextMission(DRLQuest p_quest, DRLMission p_mission)
		{
			Debug.Log("GameController> NextMission");
			if (!p_mission)
			{
				Debug.LogWarning("GameController> Invalid Mission data! ");
				base.app.scene.LoadMain();
				return;
			}
			if (!p_quest)
			{
				Debug.LogWarning("GameController> Invalid Quest data! ");
				base.app.scene.LoadMain();
				return;
			}
			if (model.type != GameFlag.Mission)
			{
				Debug.LogWarning("GameController> Tried to load next mission, but in Mission game type.");
				return;
			}
			int count = p_quest.missions.Count;
			int num = p_quest.missions.IndexOf(p_mission);
			if (num < 0)
			{
				Debug.LogWarning("GameController> Invalid Next Mission ");
				base.app.scene.LoadMain();
				return;
			}
			num++;
			for (int i = num; i < count; i++)
			{
				num = i;
				if ((bool)p_quest.missions[num])
				{
					break;
				}
			}
			Debug.Log("GameController> NextMision - next[" + num + "] total[" + count + "]");
			if (num >= count)
			{
				base.app.view.ui.screens.Open<UIQuestsView>("train-menu-screen");
				return;
			}
			DRLMission dRLMission = p_quest.missions[num];
			UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>(p_quest.tags.Contains(GameFlag.DMVQuest) ? "lesson-overview-screen" : "mission-overview-screen");
			uIMissionOverviewView.screen.title = dRLMission.title;
			uIMissionOverviewView.quest = p_quest;
			uIMissionOverviewView.mission = dRLMission;
		}

		public void NextQuest(DRLQuest p_quest)
		{
			DRLMission dRLMission = p_quest.missions[0];
			Debug.Log("GameController> NextMission");
			if (!dRLMission)
			{
				Debug.LogWarning("GameController> Invalid Mission data! ");
				base.app.scene.LoadMain();
				return;
			}
			if (!p_quest)
			{
				Debug.LogWarning("GameController> Invalid Quest data! ");
				base.app.scene.LoadMain();
				return;
			}
			if (model.type != GameFlag.Mission)
			{
				Debug.LogWarning("GameController> Tried to load next mission, but in Mission game type.");
				return;
			}
			UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>(p_quest.tags.Contains(GameFlag.DMVQuest) ? "lesson-overview-screen" : "mission-overview-screen");
			uIMissionOverviewView.screen.title = dRLMission.title;
			uIMissionOverviewView.quest = p_quest;
			uIMissionOverviewView.mission = dRLMission;
		}

		public void NextCampaignRace(DRLCampaign p_campaign, int p_offset = 0)
		{
			Debug.Log("GameController> NextCampaignRace");
			if (!p_campaign)
			{
				Debug.LogWarning("GameController> Invalid Campaign data! ");
				base.app.scene.LoadMain();
				return;
			}
			int count = base.app.model.storage.state.player.results.campaign.FindAll(p_campaign).Count;
			if (p_campaign.IsComplete(count))
			{
				Debug.LogWarning("GameController> Campaign [" + p_campaign.label + "] is complete!");
				base.app.scene.LoadMain();
				return;
			}
			count += p_offset;
			DRLCampaignRace dRLCampaignRace = p_campaign.GetRace(count);
			if (dRLCampaignRace == null)
			{
				Debug.LogWarning("GameController> Campaign [" + p_campaign.label + "] Invalid Race");
				base.app.scene.LoadMain();
				return;
			}
			base.app.view.audio.StopAllGameAudio();
			base.app.arguments.game.type = GameFlag.Campaign;
			if (dRLCampaignRace.isCustomMap)
			{
				base.app.scene.LoadCommunityMap(dRLCampaignRace.customMap.guid);
				return;
			}
			base.app.arguments.game.map = dRLCampaignRace.track.map;
			base.app.arguments.game.track = dRLCampaignRace.track;
			Activity.RunOnce(base.app.scene.Load, 0.2f);
		}

		public void FadeBlur(float p_value, float p_duration, float p_delay = 0f)
		{
			List<DroneCamera> list = model.simulation.cameras.list;
			for (int i = 0; i < list.Count; i++)
			{
				CameraFX cameraFX = list[i].GetComponent<CameraFX>();
				if (!cameraFX)
				{
					cameraFX = list[i].gameObject.AddComponent<CameraFX>();
				}
				cameraFX.FadeBlur(p_value, p_duration, p_delay);
			}
		}

		public void UIPeek(float p_duration = 0.7f, Transform p_target = null, Transform p_parent = null, int p_index = -1)
		{
			if (m_peek_timer != null)
			{
				m_peek_timer.Stop();
			}
			base.app.view.ui.screens.ClearStaticBackground();
			base.app.view.ui.screens.fade.Fade(0.05f, 0.5f, 0f, Cubic.Out);
			base.app.view.ui.dark.Fade(0f, 0.5f);
			FadeBlur(0f, 0.5f);
			Transform target_parent = p_parent;
			if ((bool)p_target)
			{
				p_target.SetParent(base.app.view.ui.transform, worldPositionStays: true);
			}
			m_peek_timer = Activity.RunOnce(delegate
			{
				base.app.view.ui.screens.fade.Fade(1f, 0.5f, 0f, Cubic.Out);
				int count = base.app.view.ui.screens.manager.count;
				float p_value = ((count <= 0) ? 0f : 1f);
				if ((bool)p_target)
				{
					p_target.SetParent(target_parent, worldPositionStays: true);
					if (p_index >= 0)
					{
						p_target.transform.SetSiblingIndex(p_index);
					}
				}
				if (count > 0)
				{
					base.app.view.ui.dark.Fade(1f, 0.5f);
				}
				FadeBlur(p_value, 0.8f);
				this.TimerRunOnce(delegate
				{
					if (base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "game-pause-screen")
					{
						base.app.view.ui.screens.SetStaticBackground();
					}
					else
					{
						base.app.view.ui.screens.ClearStaticBackground();
					}
				}, 0.81f);
			}, p_duration);
		}

		public void SetTabScreenEnabled(bool p_flag)
		{
			if (!p_flag)
			{
				ui.hud.standingsFade.FadeOut(0.12f);
			}
			else
			{
				ui.hud.standingsFade.FadeIn(0.12f);
			}
			ui.hud.physics.view.raceStandingsVisible = p_flag;
		}

		public void SwitchTabScreen()
		{
			SetTabScreenEnabled((ui.hud.standingsFade.alpha < 0.5f) ? true : false);
		}

		public void RefreshCrosshairVisibility(DroneCameraModeType p_mode)
		{
			if (base.app.model.game == null)
			{
				return;
			}
			UIHUD hud = base.app.view.ui.game.hud;
			if (!(hud == null))
			{
				UIScreen current = base.app.view.ui.screens.current;
				string text = (current ? current.name : "");
				if (text != null && text == "game-spectate-screen")
				{
					hud.crosshair.SetActive(value: false);
				}
				else if (p_mode == DroneCameraModeType.FPV || p_mode == DroneCameraModeType.FPVSmooth)
				{
					hud.crosshair.SetActive(base.app.model.storage.state.player.settings.game.crosshair);
				}
				else
				{
					hud.crosshair.SetActive(value: false);
				}
			}
		}

		public void RefreshCrosshairVisibility()
		{
			if ((bool)model.camera)
			{
				RefreshCrosshairVisibility(model.camera.mode);
			}
		}

		public GameCameraMode GetCameraMode(DroneCamera p_camera)
		{
			if (!p_camera)
			{
				return GameCameraMode.FPV;
			}
			switch (p_camera.mode)
			{
			case DroneCameraModeType.FPV:
			case DroneCameraModeType.FPVSmooth:
				return GameCameraMode.FPV;
			case DroneCameraModeType.TPVBack:
			case DroneCameraModeType.TPVSmooth:
				return GameCameraMode.TPV;
			case DroneCameraModeType.TPVFree:
				return GameCameraMode.Orbit;
			default:
				return GameCameraMode.FPV;
			}
		}

		public void SetCameraMode(Drone p_drone, DroneCamera p_camera, GameCameraMode p_mode)
		{
			if (!p_drone)
			{
				return;
			}
			if ((bool)p_camera)
			{
				switch (p_mode)
				{
				case GameCameraMode.FPV:
					p_camera.SetFPV(p_drone);
					break;
				case GameCameraMode.TPV:
					p_camera.SetGameExternal(p_drone);
					break;
				case GameCameraMode.Orbit:
					p_camera.SetTPVFree(p_drone, 0.8f, 0.4f);
					p_camera.orbit.angle = new Vector2(45f, 20f);
					p_camera.follow.offset = new Vector3(0f, 0.025f, 0f);
					p_camera.fov = 45f;
					break;
				case GameCameraMode.Finish:
					p_camera.SetTPVFree(p_drone, 0f, 0.1f, 2f);
					p_camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
					p_camera.orbit.angle = new Vector2(135f, 45f);
					p_camera.follow.offset = new Vector3(0f, 0f, 0f);
					p_camera.orbit.anchorRotation = p_drone.transform.rotation;
					p_camera.wasd.enabled = false;
					p_camera.fov = 45f;
					break;
				}
			}
		}

		private void OnCameraModeChanged(DroneCameraModeType p_mode)
		{
			Notify(N.Game.CameraModeChanged, p_mode);
			RefreshCrosshairVisibility(p_mode);
			Debug.Log("GameController>Camera mode changed to " + p_mode);
		}

		public void FadeTimescale(float p_timescale, float p_duration = 0.5f)
		{
			Tween.Kill(this);
			if (p_duration <= 0f)
			{
				timescale = p_timescale;
			}
			else
			{
				Tween.Add(this, "timescale", p_timescale, p_duration, 0f, Cubic.In).unscaledTime = true;
			}
		}

		public void SetTimescale(float p_timescale)
		{
			Time.timeScale = p_timescale;
			if ((bool)model.simulation)
			{
				model.simulation.speed = p_timescale;
			}
			base.app.view.audio.UpdateTimescale(p_timescale);
		}

		public void SetSimulation(DroneSimulation p_simulation)
		{
			DroneSimulation simulation = model.simulation;
			if ((bool)simulation)
			{
				simulation.OnEvent.RemoveAllListeners();
			}
			simulation = (model.simulation = p_simulation);
			ui.hud.training.requirements = simulation.UIRequirements;
			if (!simulation)
			{
				Debug.LogWarning("GameController> Invalid Simulation Set!");
			}
			if ((bool)simulation)
			{
				simulation.OnEvent.AddListener(OnDroneSimulationEvent);
			}
		}

		private void OnDroneSimulationEvent(DroneSimulationEvent p_event)
		{
			switch (p_event.type)
			{
			case DroneSimulationEventType.DroneAdd:
			{
				Drone drone3 = Reflection<object>.Get<Drone>(p_event.args, 0);
				drone3.OnEvent.AddListener(OnDroneEvent);
				Notify("game.simulation.drone@add", drone3);
				break;
			}
			case DroneSimulationEventType.DroneRemove:
			{
				Drone drone2 = Reflection<object>.Get<Drone>(p_event.args, 0);
				drone2.OnEvent.RemoveAllListeners();
				Notify("game.simulation.drone@remove", drone2);
				break;
			}
			case DroneSimulationEventType.DroneReady:
			{
				Drone drone = Reflection<object>.Get<Drone>(p_event.args, 0);
				Notify("game.simulation.drone@ready", drone);
				break;
			}
			case DroneSimulationEventType.AllDronesReady:
				Notify("game.simulation.drone.all@ready");
				break;
			case DroneSimulationEventType.CameraAdd:
			{
				DroneCamera droneCamera = Reflection<object>.Get<DroneCamera>(p_event.args, 0);
				Notify("game.simulation.camera@add", droneCamera);
				break;
			}
			case DroneSimulationEventType.DroneNanRecover:
			{
				Drone p_drone = Reflection<object>.Get<Drone>(p_event.args, 0);
				PodiumReset(p_drone);
				break;
			}
			}
		}

		private void OnDroneEvent(DroneEvent p_event)
		{
			Drone d = p_event.target;
			switch (p_event.type)
			{
			case DroneEventType.Armed:
				Notify("game.simulation.drone@armed", d);
				break;
			case DroneEventType.Disarmed:
				Notify("game.simulation.drone@disarmed", d);
				break;
			case DroneEventType.TurtleOn:
				Notify("game.simulation.drone.turtle@on", d);
				break;
			case DroneEventType.TurtleOff:
				Notify("game.simulation.drone.turtle@off", d);
				break;
			case DroneEventType.Crash:
				Notify("game.simulation.drone@crash", d);
				if (!(d != model.playerDrone))
				{
					if (model.type == GameFlag.Freestyle)
					{
						DroneCamera camera = model.camera;
						m_lastCameraMode = GetCameraMode(camera);
						SetCameraMode(d, camera, GameCameraMode.Orbit);
						camera.orbit.angle = new Vector2(0f, 30f);
						camera.orbit.anchorRotation = Quaternion.LookRotation(d.fc.sensor.inertial.velocity, Vector3.up);
						camera.orbit.distance = 0.95f;
						d.fc.armed = false;
					}
					base.app.view.ui.game.hud.damage.SetCrash();
				}
				break;
			case DroneEventType.Recover:
				Notify("game.simulation.drone@recover", d);
				if (!(d != model.playerDrone))
				{
					DroneCamera camera2 = model.camera;
					if (m_lastCameraMode != GameCameraMode._InGame__)
					{
						SetCameraMode(d, camera2, m_lastCameraMode);
					}
					m_lastCameraMode = GameCameraMode._InGame__;
					d.damageReduction = 0f;
					base.app.view.ui.game.hud.damage.Reset();
				}
				break;
			case DroneEventType.WaterImpact:
			{
				if (d != model.playerDrone)
				{
					break;
				}
				DroneCamera c = model.camera;
				m_lastCameraMode = GetCameraMode(c);
				SetCameraMode(d, c, GameCameraMode.Orbit);
				RunOnce(delegate
				{
					DroneReset(d);
					d.Fix();
					if (m_lastCameraMode != GameCameraMode._InGame__)
					{
						SetCameraMode(d, c, m_lastCameraMode);
					}
					m_lastCameraMode = GameCameraMode._InGame__;
					Notify("game.simulation.drone@flip");
				}, 2f);
				break;
			}
			case DroneEventType.Scrape:
			{
				if (d != model.playerDrone || d.damage <= 0f)
				{
					break;
				}
				Notify("game.simulation.drone@scrape", d);
				float[] damagePenalty = SettingsController.GetDamagePenalty(d.damage);
				_ = damagePenalty[0];
				float p_deviationFactor = damagePenalty[1];
				float num = d.crashData.crashEnergy / Drone.CrashEnergy;
				d.damageReduction += damagePenalty[0] * num;
				d.ApplySpinout(p_deviationFactor);
				D.Log("GameController> On Drone Scrape Event - energy multiplier:" + num + " reducing motor strength: " + d.damageReduction + "\nApllied spinout with factor: " + p_deviationFactor);
				base.app.view.ui.game.hud.damage.SetDamage(d.crashData.bodyDamage, d.crashData.propsDamage);
				if (d.damageReduction >= SettingsController.damageCrashThreshold)
				{
					base.app.view.ui.game.hud.damage.SetCrash();
					d.Crash();
					d.crashData.crashEnergy = 0f;
					break;
				}
				model.camera.fx.shake.Shake(1f, 0f);
				if ((bool)activeGameMode)
				{
					activeGameMode.OnDroneScrape(p_event);
				}
				break;
			}
			case DroneEventType.ScrapeAudio:
			{
				if (m_scrapeAudioPlaying || d != model.playerDrone || d.crashData == null || d.crashData.crashEnergy <= 0f || d.crashData.type != DroneEventType.ScrapeAudio)
				{
					break;
				}
				float value = d.crashData.crashEnergy / Drone.CrashEnergy;
				value = Mathf.Clamp01(value);
				value = value * (float)Math.E + 1f;
				value = Mathf.Log(value) + 0.05f;
				value = Mathf.Clamp01(value);
				audio.PlayDroneDamage(d, value);
				m_scrapeAudioPlaying = true;
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						m_scrapeAudioPlaying = false;
					}
				}, 1f / 30f);
				break;
			}
			case DroneEventType.Collision:
			case DroneEventType.PropScrape:
			case DroneEventType.NanRecover:
				break;
			}
		}

		public void ApplyCommunityDroneToDrone(Drone p_drone)
		{
			if (p_drone == null || p_drone.hasRig)
			{
				return;
			}
			if (p_drone.rig.hasCustomPhysics)
			{
				DronePhysicsData dronePhysicsData = DronePhysicsData.FromJson(p_drone.rig.tune);
				if (dronePhysicsData != null)
				{
					p_drone.physics = dronePhysicsData;
					p_drone.defaultphysics = dronePhysicsData;
				}
			}
			if (PlayerPrefs.HasKey("drone-profile-" + p_drone.rig.guid))
			{
				p_drone.rig.profile = PlayerPrefs.GetString("drone-profile-" + p_drone.rig.guid);
			}
			DroneProfileData droneProfileData = DroneProfileData.FromJson(p_drone.rig.profile);
			if (droneProfileData != null)
			{
				p_drone.profile = droneProfileData;
				p_drone.defaultprofile = droneProfileData;
			}
		}

		public UIMultiplayerRoomView OpenNetworkRoomScreen()
		{
			NetworkModel network = base.app.model.network;
			if (network.room == null)
			{
				base.app.view.audio.PlayUIGenericError();
				Debug.LogWarning("GameController> OpenNetworkRoomScreen - No room available.");
				return null;
			}
			Debug.Log("GameController> OpenNetworkRoomScreen");
			ui.hud.Fade(0f);
			UIMultiplayerRoomView uIMultiplayerRoomView = base.app.view.ui.screens.Open<UIMultiplayerRoomView>("multiplayer-room-screen", 0.5f);
			string title = string.Concat(str2: network.room.RoomTitle.ToUpper(), str0: network.room.GameMode.ToString().ToUpper(), str1: "/");
			uIMultiplayerRoomView.screen.title = title;
			uIMultiplayerRoomView.leaveRoomOnExit = false;
			uIMultiplayerRoomView.Clear();
			uIMultiplayerRoomView.SetGameType(network.room.GameMode);
			uIMultiplayerRoomView.SetAvailableOptions(network.room.IsMaster, network.room.IsTournamentMatch);
			uIMultiplayerRoomView.SetExitButtonEnabled(p_enabled: true);
			return uIMultiplayerRoomView;
		}

		public void SetGCEnabled(bool p_flag)
		{
			bool flag = false;
			bool flag2 = false;
			switch (GarbageCollector.GCMode)
			{
			case GarbageCollector.Mode.Disabled:
				if (!p_flag)
				{
					return;
				}
				flag2 = true;
				flag = true;
				break;
			case GarbageCollector.Mode.Enabled:
				if (p_flag)
				{
					return;
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				Debug.LogWarning($"GameController> SetGCEnabled / flag[{p_flag}] - Not Supported in {Application.platform}!");
				return;
			}
			Debug.LogWarning($"GameController> SetGCEnabled / flag[{p_flag}] flushMemory[{flag2}]");
			GarbageCollector.GCMode = (p_flag ? GarbageCollector.Mode.Enabled : GarbageCollector.Mode.Disabled);
			if (flag2)
			{
				GC.Collect();
			}
		}
	}
}
