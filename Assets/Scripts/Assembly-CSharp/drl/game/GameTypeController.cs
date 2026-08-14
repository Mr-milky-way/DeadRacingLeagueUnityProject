using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PostProcessing;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameTypeController : Controller<DRLApp>
	{
		private GameController m_game;

		private UIGame m_ui;

		public GameFlag type;

		public GameFlag mode;

		public bool introComplete;

		public bool podiumAnimationEnabled;

		public DronePodium podiumAnimationFocus;

		public static bool ignoreCount;

		public string cheatLog;

		private bool m_need_restore;

		private static bool m_camera_restore_values_saved;

		private static float m_camera_fov;

		protected Activity m_drone_slowmotion_complete;

		protected Activity m_drone_slowmotion_stop;

		protected MonoActivity m_replay_stop_timer;

		protected float flightTime;

		protected bool m_raceSlowmotionStarted;

		protected bool resetInProgress;

		private Thread m_process_rpl_thd;

		private bool m_process_rpl_enabled;

		public GameController game
		{
			get
			{
				if (!m_game)
				{
					return m_game = (base.app ? base.app.controller.game : null);
				}
				return m_game;
			}
		}

		public UIGame ui
		{
			get
			{
				if (!m_ui)
				{
					return m_ui = base.app.view.ui.game;
				}
				return m_ui;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "scene.start":
				CheckActivation();
				break;
			case "game.boot":
			{
				DRLApp.LogMemStats("GameTypeController> Game Boot", p_show_delta: true);
				DroneSimulation droneSimulation = null;
				droneSimulation = GetSimulation();
				string podium = base.app.arguments.game.podium;
				string text = (base.app.arguments.game.track ? base.app.arguments.game.track.podium : "");
				bool isCustomMap = base.app.arguments.game.isCustomMap;
				MapData mapData = (isCustomMap ? base.app.arguments.game.map.data : null);
				podium = ((isCustomMap && mapData.mapCategoryFlag == GameFlag.MapMultiGP) ? "PD-db5" : text);
				if (base.app.arguments.game.type == GameFlag.Campaign)
				{
					DRLCampaign campaign = base.app.arguments.game.campaign;
					if ((bool)campaign && !string.IsNullOrEmpty(campaign.podium))
					{
						podium = campaign.podium;
					}
				}
				if (string.IsNullOrEmpty(podium))
				{
					podium = "PD-a6d";
				}
				if (isCustomMap && !string.IsNullOrEmpty(mapData.podiumId))
				{
					podium = mapData.podiumId;
					UnityEngine.Debug.Log("GameTypeController> Boot / MapData contains podium " + podium);
				}
				DronePodium dronePodium = base.app.model.storage.library.FindByGUID<DronePodium>(podium);
				if (!dronePodium)
				{
					dronePodium = base.app.model.storage.library.FindByGUID<DronePodium>(podium = "PD-a6d");
				}
				if ((bool)dronePodium)
				{
					droneSimulation.podiums.template = dronePodium;
				}
				UnityEngine.Debug.Log("GameTypeController> Boot / podium[" + podium + "]");
				game.SetSimulation(droneSimulation);
				if (!droneSimulation)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> Boot / Failed to locate a simulation!");
					base.app.args.Clear();
					base.app.scene.LoadMain();
					return;
				}
				DRLApp.LogMemStats("GameTypeController> Simulation Set", p_show_delta: true);
				Physics.autoSyncTransforms = true;
				LoadPodiums(podium);
				LoadCameras();
				LoadDrones();
				DRLApp.LogMemStats("GameTypeController> Simulation Load Complete", p_show_delta: true);
				Notify(0.01f, "game.simulation.load@complete", droneSimulation);
				break;
			}
			case "game.ready":
				introComplete = false;
				PlayIntroAnimation();
				Notify("game.intro.animation@start");
				UnityEngine.Debug.Log("GameTypeController> IntroAnimationStart");
				break;
			case "game.simulation.drone.all@ready":
				if (!introComplete)
				{
					introComplete = true;
					LoadPlayerTuning();
					LoadCameraSettings();
					game.model.simulation.Run(p_arm: false);
					Run((float t) => t <= 1f || !CompleteIntroAnimation());
					flightTime = 0f;
				}
				break;
			case "game.simulation.drone@ready":
			{
				RefreshPodiumAnimationInfo();
				FCMode p_mode = FCMode.Pro;
				if (base.app.arguments.game.mission != null)
				{
					p_mode = base.app.arguments.game.mission.flightModes[0];
				}
				if (base.app.inOnboarding && base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
				{
					base.app.model.storage.state.player.activeFCMode = FCMode.Pro;
				}
				SetDroneFCMode(game.model.playerDrone, p_mode);
				break;
			}
			case "game.ui.dashboard@show":
				base.app.view.ui.game.hud.dashboard.Show(p_data);
				break;
			case "game.ui.dashboard@hide":
				base.app.view.ui.game.hud.dashboard.Hide();
				break;
			case "game.simulation.drone.flight-time@update":
			{
				DroneRigData droneRigData = ((p_data.Length == 0) ? null : ((DroneRigData)p_data[0]));
				if (!base.validContext || !(droneRigData != null))
				{
					flightTime = 0f;
					break;
				}
				ServiceModel service = base.app.model.service;
				DRLMap map = base.app.scene.map;
				DRLMapTrack dRLMapTrack = ((!map.custom) ? base.app.scene.track : null);
				service.SetCommunityDroneTime(p_gameType: game.model.type.ToString() + ((game.model.mode == GameFlag.NetworkMultiplayer) ? "-multiplayer" : ""), p_map: map ? map.guid : null, p_track: dRLMapTrack ? dRLMapTrack.guid : null, p_communityMap: (!map) ? null : (map.custom ? base.app.scene.map.data.guid : null), p_guid: droneRigData.guid, p_time: flightTime / 60f, p_callback: null);
				flightTime = 0f;
				break;
			}
			case "game.unpause":
			{
				bool hotkeys = base.app.model.storage.state.player.settings.game.hotkeys;
				base.app.view.ui.game.hud.SetHotkeysEnabled(hotkeys);
				Notify("game.simulation.drone@armed", game.model.playerDrone);
				break;
			}
			case "game.change-game@click":
				if (base.app.arguments.game.type == GameFlag.Collectable)
				{
					UIMapsSDCategoryView uIMapsSDCategoryView = base.app.view.ui.screens.Open<UIMapsSDCategoryView>("collectables-category-screen");
					uIMapsSDCategoryView.screen.title = base.app.model.storage.locale.Get("maps.choose-map", "Choose Map");
					uIMapsSDCategoryView.caller = this;
				}
				else
				{
					UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
					uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("maps.choose-map", "Choose Map");
					uIMapsCategoryView.caller = this;
				}
				break;
			case "maps.selection-complete":
				if (base.app.controller.AssertMapSelection(p_target, this))
				{
					base.app.controller.LoadTrackOverview(this, p_target, p_data);
				}
				break;
			}
			OnSettingsNotification(p_event, p_target, p_data);
		}

		protected bool CheckActivation()
		{
			if (!base.validContext)
			{
				return false;
			}
			bool flag = true;
			if (base.app.arguments == null || base.app.arguments.game == null)
			{
				flag = false;
			}
			else
			{
				if (type != GameFlag.None && base.app.arguments.game.type != type)
				{
					flag = false;
				}
				if (mode != GameFlag.None && base.app.arguments.game.mode != mode)
				{
					flag = false;
				}
			}
			if (flag)
			{
				UnityEngine.Debug.Log(GetType().Name + "> Activate");
			}
			base.gameObject.SetActive(flag);
			return flag;
		}

		protected virtual DroneSimulation GetSimulation()
		{
			return AssertFind<DroneSimulation>("simulation");
		}

		protected virtual void LoadPodiums(string p_guid = "")
		{
			string text = (string.IsNullOrEmpty(p_guid) ? base.app.arguments.game.track.podium : p_guid);
			if (base.app.arguments.game.type == GameFlag.Campaign)
			{
				DRLCampaign campaign = base.app.arguments.game.campaign;
				if (!string.IsNullOrEmpty(campaign.podium))
				{
					text = campaign.podium;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				for (int i = 0; i < game.model.players.Count; i++)
				{
					game.model.players[i].podiumId = text;
				}
			}
			Transform starts = game.model.level.track.starts;
			game.model.simulation.podiums.root = starts;
			_ = game.model.players;
			List<Transform> podiums = game.model.level.track.podiums;
			int count = podiums.Count;
			for (int j = 0; j < count; j++)
			{
				GamePlayerData racerDataByOrder = game.model.GetRacerDataByOrder(j);
				DronePodium dronePodium = ((racerDataByOrder == null) ? null : base.app.model.storage.library.FindByGUID<DronePodium>(racerDataByOrder.podiumId));
				if (dronePodium != null && dronePodium.guid == "PD-a6d")
				{
					dronePodium = null;
				}
				Transform p_anchor = podiums[j];
				game.model.simulation.podiums.Push(dronePodium, p_anchor);
			}
		}

		protected virtual void OnGameReady()
		{
		}

		public bool PlayTrackAnimation()
		{
			if (game.model.level.track == null || game.model.level == null)
			{
				return false;
			}
			TrackModel track = base.app.model.game.level.track;
			if (!track.hasTrackAnimation)
			{
				return false;
			}
			DroneCamera p_target = game.model.simulation.cameras.Get(0);
			track.PlayTrackAnimation(p_target);
			return true;
		}

		public bool PlayPodiumAnimation(int p_defaultPodium = 0)
		{
			TrackModel track_model = base.app.model.game.level.track;
			m_need_restore = false;
			if (!track_model.podiumAnimation)
			{
				return false;
			}
			DroneSimulation sim = game.model.simulation;
			DroneCamera c = sim.cameras.Get(0);
			m_need_restore = true;
			if (!m_camera_restore_values_saved)
			{
				m_camera_fov = c.fov;
				m_camera_restore_values_saved = true;
			}
			game.ui.hud.userInfo.fade.alpha = -0.1f;
			podiumAnimationEnabled = true;
			sim.drones.SetCustomReflections(base.app.model.storage.state.player.settings.graphics.advancedRendering && base.app.model.storage.state.player.settings.graphics.eyeAdaptation);
			float speed = 1f;
			if (game.model.mode == GameFlag.NetworkMultiplayer)
			{
				speed = 3f;
			}
			new List<Drone>();
			this.TimerRunOnce(delegate
			{
				c.SetPodiumAnimation(p_defaultPodium, sim, track_model.podiumAnimation, speed, game.model.players, delegate(int p_podium_id)
				{
					DronePodium dronePodium = sim.podiums.Get(p_podium_id);
					if ((bool)dronePodium)
					{
						podiumAnimationFocus = dronePodium;
						RefreshPodiumAnimationInfo();
					}
				});
			}, 1f);
			return true;
		}

		protected void RefreshPodiumAnimationInfo()
		{
			if (!podiumAnimationEnabled)
			{
				return;
			}
			DroneSimulation simulation = game.model.simulation;
			DronePodium dronePodium = podiumAnimationFocus;
			if (!dronePodium)
			{
				return;
			}
			Drone closest = simulation.drones.GetClosest(dronePodium.spawn.position);
			if ((bool)closest)
			{
				if (game.model.racerCount > 1 && !base.app.model.storage.state.player.profile.isObserver)
				{
					game.ui.hud.alpha = 1f;
					game.ui.hud.userInfo.fade.alpha = 1f;
				}
				GamePlayerData playerData = game.model.GetPlayerData(closest);
				game.ui.hud.userInfo.Set(playerData);
			}
		}

		public void StopIntroAnimations()
		{
			game.ui.hud.userInfo.fade.alpha = -0.1f;
			game.ui.hud.alpha = -0.1f;
			podiumAnimationEnabled = false;
			DroneSimulation simulation = game.model.simulation;
			if (!simulation)
			{
				return;
			}
			DroneCamera droneCamera = simulation.cameras.Get(0);
			simulation.drones.SetCustomReflections(p_flag: false);
			TrackModel track = base.app.model.game.level.track;
			if (!track)
			{
				return;
			}
			if ((bool)track.podiumAnimation)
			{
				droneCamera.StopPodiumAnimation(track.podiumAnimation);
				if (m_need_restore && m_camera_restore_values_saved)
				{
					droneCamera.fov = m_camera_fov;
					float p_rad = 0f;
					float p_int = 0f;
					base.app.controller.settings.GetAmbientOcclusionIntensityAndRadius(out p_int, out p_rad);
					droneCamera.fx.aoIntensity = p_int;
					droneCamera.fx.aoRadius = p_rad;
				}
			}
			track.StopTrackAnimation();
		}

		protected virtual void PlayIntroAnimation()
		{
		}

		protected virtual bool CompleteIntroAnimation()
		{
			if (!RequestIntroAnimationSkip())
			{
				return false;
			}
			OnIntroAnimationComplete();
			Drone playerDrone = game.model.playerDrone;
			if (playerDrone != null)
			{
				int num = -1;
				int num2 = -1;
				if (playerDrone.fc != null)
				{
					num2 = (int)playerDrone.fc.mode;
				}
				if (playerDrone.rig != null)
				{
					num = playerDrone.rig.diameter;
				}
				if (game.model.type == GameFlag.Mission)
				{
					num2 = -1;
					num = -1;
				}
				Notify("game.intro.animation@complete", num2, num);
			}
			else
			{
				Notify("game.intro.animation@complete");
			}
			UnityEngine.Debug.Log("GameTypeController> IntroAnimationComplete");
			SetGameReady();
			return true;
		}

		public void SetGameReady()
		{
			game.input.SetController(this);
			game.input.listening = true;
			bool flag = game.model.type != GameFlag.Mission && base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot;
			if (base.app.inOnboarding)
			{
				flag = flag && base.app.model.onboarding.activeOnboarding.mode == OnboardingCampaignMode.Pro;
			}
			base.app.view.ui.game.hud.damage.Show(flag);
			Physics.autoSyncTransforms = false;
			OnGameReady();
		}

		protected virtual void OnIntroAnimationComplete()
		{
		}

		protected virtual bool RequestIntroAnimationSkip()
		{
			bool result = Input.anyKeyDown || RCI.GetAnyButtonDown() || RCI.GetAxisToggle(RawAxis.ToggleA, p_positiveDirection: true) || RCI.GetAxisToggle(RawAxis.ToggleB, p_positiveDirection: true);
			if (Input.GetKey(KeyCode.F12))
			{
				result = false;
			}
			return result;
		}

		protected virtual void OnSettingsNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "game.simulation.drone@ready":
			{
				Drone drone = (Drone)p_data[0];
				if ((bool)drone && drone == game.model.playerDrone)
				{
					LoadPlayerTuning();
				}
				break;
			}
			case "settings.graphics.apply":
				LoadCameraSettings();
				break;
			case "settings.tuning.profile.save":
				LoadPlayerTuning();
				break;
			case "settings.game.form.tilt":
			{
				Drone playerDrone2 = game.model.playerDrone;
				if ((bool)playerDrone2)
				{
					float num2 = Reflection<object>.Get((IList)p_data, 0, playerDrone2.body.frame.camera.tilt);
					playerDrone2.body.frame.camera.tilt = num2;
					base.app.model.storage.state.player.settings.tuning.UpdateCameraDelayed(num2);
				}
				break;
			}
			case "settings.game.form.fov":
			{
				Drone playerDrone = game.model.playerDrone;
				DroneCamera camera = game.model.camera;
				if ((bool)playerDrone)
				{
					float num = CameraLens.H2VFov(Reflection<object>.Get((IList)p_data, 0, playerDrone.body.frame.camera.fov));
					if (camera.mode == DroneCameraModeType.FPV)
					{
						camera.fov = num;
					}
					else
					{
						camera.fov = 45f;
					}
					playerDrone.body.frame.camera.fov = num;
					base.app.model.storage.state.player.settings.tuning.UpdateCameraDelayed(-1f, num);
				}
				break;
			}
			}
		}

		protected virtual void LoadPlayerTuning(FCProfileData p_data = null)
		{
			Drone playerDrone = game.model.playerDrone;
			if (!playerDrone)
			{
				UnityEngine.Debug.LogWarning("GameTypeController> Player Drone not found!");
				return;
			}
			FCProfileData fCProfileData = ((p_data == null) ? base.app.model.storage.state.player.settings.tuning.GetActive() : p_data);
			playerDrone.fc.profile = fCProfileData;
			UnityEngine.Debug.Log("GameTypeController> LoadPlayerTuning - drone[" + playerDrone.name + "] fcp[" + fCProfileData.guid + "]");
		}

		protected virtual void LoadCameraSettings()
		{
			UnityEngine.Debug.Log("GameTypeController> LoadCameraSettings");
		}

		protected virtual void LoadCameras()
		{
			DroneCamera droneCamera = ((game.model.camera == null) ? game.model.simulation.cameras.Push() : game.model.camera);
			game.model.camera = droneCamera;
			CameraFadeLights cameraFadeLights = droneCamera.main.gameObject.GetComponent<CameraFadeLights>();
			if (cameraFadeLights == null)
			{
				cameraFadeLights = droneCamera.main.gameObject.AddComponent<CameraFadeLights>();
			}
			float distanceDecay = cameraFadeLights.distanceDecay;
			switch (OS.context)
			{
			case "xb":
			case "xbs":
			case "xbx":
			case "xbss":
			case "xbsx":
			case "ps4base":
			case "ps4pro":
			case "ps5":
				distanceDecay = 35f;
				break;
			}
			cameraFadeLights.distanceDecay = distanceDecay;
			cameraFadeLights.targetLevel = base.app.model.game.level.root;
			cameraFadeLights.targetTracks = base.app.model.game.level.track.root;
			cameraFadeLights.gameCamera = droneCamera.main;
			cameraFadeLights.Initialize();
			bool fadeReflections = base.app.model.game.level.settings.reflection.fadeReflections;
			float defaultRangePadding = base.app.model.game.level.settings.reflection.defaultRangePadding;
			CameraFadeReflectionProbes cameraFadeReflectionProbes = droneCamera.main.gameObject.GetComponent<CameraFadeReflectionProbes>();
			if (cameraFadeReflectionProbes == null)
			{
				cameraFadeReflectionProbes = droneCamera.main.gameObject.AddComponent<CameraFadeReflectionProbes>();
			}
			cameraFadeReflectionProbes.targetLevel = base.app.model.game.level.root;
			cameraFadeReflectionProbes.targetTracks = base.app.model.game.level.track.root;
			cameraFadeReflectionProbes.gameCamera = droneCamera.main;
			cameraFadeReflectionProbes.rangePadding = new Vector3(defaultRangePadding, defaultRangePadding, defaultRangePadding);
			cameraFadeReflectionProbes.enabled = fadeReflections;
			cameraFadeReflectionProbes.Initialize();
			Transform transform = game.model.level.root.transform.Find("orbit-camera");
			if (transform != null)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			OrbitTransform orbitTransform = base.app.model.storage.library.Find<OrbitTransform>("orbit-camera");
			if ((bool)orbitTransform)
			{
				orbitTransform = UnityEngine.Object.Instantiate(orbitTransform, game.model.level.root.transform);
				orbitTransform.name = "orbit-camera";
				orbitTransform.gameObject.SetActive(value: false);
				game.model.orbit = orbitTransform;
				UnityEngine.Object.Destroy(orbitTransform.GetComponentInChildren<Camera>().gameObject);
				Camera camera = UnityEngine.Object.Instantiate(droneCamera.main, orbitTransform.transform);
				camera.name = "main";
				camera.transform.localPosition = Vector3.zero;
				camera.transform.localEulerAngles = Vector3.zero;
				camera.transform.localScale = Vector3.one;
				PostProcessingBehaviour component = camera.gameObject.GetComponent<PostProcessingBehaviour>();
				if ((bool)component)
				{
					component.profile = UnityEngine.Object.Instantiate(component.profile);
					component.profile.name = "fx-orbit-camera";
					DepthOfFieldModel depthOfField = component.profile.depthOfField;
					depthOfField.enabled = false;
					component.profile.depthOfField = depthOfField;
				}
			}
		}

		public void SetCameraStart(DroneCamera p_camera)
		{
			if ((bool)p_camera)
			{
				UnityEngine.Debug.Log("FreeCameraController> SetCamera - camera[" + p_camera?.ToString() + "]");
				base.app.model.game.level.track.SetStartsFrontTransform(p_camera.transform, new Vector3(0f, 0f, 0.1f), p_lookat_center: true);
				p_camera.orbit.Snap();
			}
		}

		protected virtual void LoadDrones()
		{
		}

		protected virtual void FreezeDroneOnPodium(Drone p_drone, float p_delay = 0.25f)
		{
			if (!base.validContext || p_drone == null)
			{
				return;
			}
			if (p_drone.ready)
			{
				FreezeDrone(p_drone, p_delay);
				return;
			}
			p_drone.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					FreezeDrone(p_drone, p_delay);
				}
			});
		}

		private void FreezeDrone(Drone p_drone, float p_delay = 0.25f)
		{
			this.TimerRunOnce(delegate
			{
				if (base.validContext && !(p_drone == null))
				{
					game.PodiumReset(p_drone, -1, p_force_podium: true);
					p_drone.receiver.ClearSignal();
					p_drone.rigidbody.isKinematic = true;
					p_drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeAll;
				}
			}, p_delay);
		}

		protected virtual void UnfreezeDrones()
		{
			if (!base.validContext || game.model.simulation == null || game.model.simulation.drones.list.Count == 0)
			{
				return;
			}
			List<Drone> list = game.model.simulation.drones.list;
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] == null) && !(list[i].rigidbody == null))
				{
					list[i].rigidbody.isKinematic = false;
					list[i].rigidbody.rb.constraints = RigidbodyConstraints.None;
					if (list[i].receiver != null)
					{
						list[i].receiver.ClearSignal();
					}
				}
			}
			Notify(1f / 12f, "game.drones.unfrozen");
		}

		public virtual void OnDroneScrape(DroneEvent p_event)
		{
		}

		public Drone CreateDrone(DroneRigData p_rig, int p_id, Color[] p_color, string p_player_id, bool p_enable_receiver = false)
		{
			DroneSimulation simulation = game.model.simulation;
			if (!simulation)
			{
				return null;
			}
			bool num = base.app.model.storage.state.player.garage.IsOriginal(p_rig);
			bool inTournament = base.app.inTournament;
			DRLTournamentMatchData dRLTournamentMatchData = (inTournament ? base.app.arguments.game.tournamentMatchData : null);
			bool num2 = (num || inTournament) && p_rig.allowDynamicColor;
			Color color = p_color[0];
			_ = ref p_color[1];
			if (dRLTournamentMatchData != null)
			{
				DRLTournamentPlayerData playerById = base.app.arguments.game.tournamentMatchData.GetPlayerById(p_player_id);
				if (playerById != null)
				{
					color = playerById.profileColor;
				}
				_ = playerById?.profileColor2;
			}
			if (num2)
			{
				p_rig.color0 = color;
				p_rig.color2 = color;
			}
			Drone d = base.app.model.storage.factory.Instantiate(p_rig, p_async: true, game.model.playerId == p_id);
			base.app.controller.game.ApplyCommunityDroneToDrone(d);
			simulation.RegisterDrone(d);
			d.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					d.receiver.enabled = p_enable_receiver;
					d.receiver.channel = p_id;
					if (p_color.Length > 1)
					{
						Color color2 = p_color[0];
						string text = color2.ToString();
						color2 = p_color[1];
						UnityEngine.Debug.Log("GameTypeController> Create Drone: Trail Colors: " + text + " " + color2.ToString());
					}
					else
					{
						Color color2 = p_color[0];
						UnityEngine.Debug.Log("GameTypeController> Create Drone: Trail Colors: " + color2.ToString());
					}
					d.renderer.playerColor = p_color[0];
					bool trailsEnabled = true;
					if (game.model.IsPlayer(d))
					{
						trailsEnabled = false;
					}
					if (!base.app.model.storage.state.player.settings.game.trails)
					{
						trailsEnabled = false;
					}
					d.SetPropwash(base.app.model.storage.state.player.settings.game.propwash);
					d.renderer.SetTrailsEnabled(trailsEnabled);
					game.PodiumReset(d, -1, p_force_podium: true);
				}
			});
			d.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.NanRecover && !(d == null) && !(game == null))
				{
					game.PodiumReset(d, -1, p_force_podium: true);
				}
			});
			if (base.app.model.game.type != GameFlag.Freestyle)
			{
				FreezeDroneOnPodium(d);
			}
			return d;
		}

		public Drone CreatePlayer(GamePlayerData p_player, DroneRigData p_default_rig, Action<Drone> p_on_ready)
		{
			DroneSimulation simulation = game.model.simulation;
			if (!simulation)
			{
				UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - Simulation is null!");
				return null;
			}
			GamePlayerData pd = p_player;
			if (pd == null)
			{
				UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - Player Data is null!");
				return null;
			}
			Drone d = null;
			DroneRigData droneRigData = base.app.model.storage.state.player.garage.currentRigData;
			if (droneRigData == null || string.IsNullOrEmpty(droneRigData.guid))
			{
				droneRigData = p_default_rig;
			}
			switch (pd.type)
			{
			case GamePlayerType.Human:
			{
				pd.Initialize();
				if (pd.rig == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - DroneRig not available on player data!");
					pd.rig = droneRigData;
				}
				DroneRigData droneRigData3 = ((pd.rig == null) ? p_default_rig : pd.rig);
				if (droneRigData3 == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreateDrone - Invalid RigData!");
					break;
				}
				pd.rig = droneRigData3;
				Color[] p_color3 = new Color[2] { pd.color, pd.color2 };
				d = CreateDrone(droneRigData3, pd.id, p_color3, p_player.playerId);
				simulation.transmitters.Add<DroneRCTransmitter>().channel = pd.id;
				break;
			}
			case GamePlayerType.Ghost:
			{
				pd.Initialize();
				DroneRigData droneRigData2 = pd.rig;
				if (ReplayFile.EnableVersion2)
				{
					droneRigData2 = ((pd.replayV2 == null) ? null : pd.replayV2.header.GetDroneRig());
				}
				if (droneRigData2 == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - DroneRig not available on player data!");
				}
				droneRigData2 = ((droneRigData2 == null) ? p_default_rig : droneRigData2);
				if (droneRigData2 == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreateDrone - Invalid RigData!");
					break;
				}
				pd.rig = droneRigData2;
				Color[] p_color2 = new Color[2] { pd.color, pd.color2 };
				d = CreateDrone(droneRigData2, pd.id, p_color2, p_player.playerId);
				DroneGhostTransmitter droneGhostTransmitter = simulation.transmitters.Add<DroneGhostTransmitter>();
				droneGhostTransmitter.channel = pd.id;
				DroneGhostTransmitter droneGhostTransmitter2 = droneGhostTransmitter;
				d.isGhost = true;
				droneGhostTransmitter2.drone = d;
				droneGhostTransmitter2.order = pd.order;
				if (ReplayFile.EnableVersion2)
				{
					droneGhostTransmitter2.dataV2 = p_player.replayV2;
					if (droneGhostTransmitter2.dataV2 == null)
					{
						UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - ReplayFile not available on player data!");
					}
				}
				else
				{
					droneGhostTransmitter2.data = p_player.replay;
					if (droneGhostTransmitter2.data == null)
					{
						UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - BlackboxData not available on player data!");
					}
				}
				break;
			}
			case GamePlayerType.Data:
			{
				pd.Initialize();
				if (pd.rig == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreatePlayer - DroneRig not available on player data!");
				}
				DroneRigData droneRigData4 = ((pd.rig == null) ? p_default_rig : pd.rig);
				pd.rig = droneRigData4;
				if (droneRigData4 == null)
				{
					UnityEngine.Debug.LogWarning("GameTypeController> CreateDrone - Invalid RigData!");
					break;
				}
				Color[] p_color4 = new Color[2] { pd.color, pd.color2 };
				UnityEngine.Debug.Log("Color: " + pd.color2.ToString());
				d = CreateDrone(droneRigData4, pd.id, p_color4, p_player.playerId);
				break;
			}
			case GamePlayerType.Network:
			{
				pd.Initialize();
				Color[] p_color = new Color[2] { pd.color, pd.color2 };
				UnityEngine.Debug.Log("Color: " + pd.color2.ToString());
				d = CreateDrone(pd.rig ?? droneRigData, pd.id, p_color, p_player.playerId, p_enable_receiver: true);
				simulation.transmitters.Add<DroneNetworkTransmitter>().channel = pd.id;
				d.isRemote = true;
				break;
			}
			case GamePlayerType.Spectator:
				pd.Initialize();
				game.model.camera.SetFreeCamera();
				break;
			}
			pd.drone = d;
			if (d != null)
			{
				simulation.PlaceDrone(d, pd.order);
				UnityAction<DroneEvent> cb = null;
				cb = delegate(DroneEvent p_event)
				{
					if (p_event.type == DroneEventType.Ready)
					{
						if ((bool)p_event.target && p_on_ready != null)
						{
							p_on_ready(p_event.target);
						}
						if ((bool)d)
						{
							d.OnEvent.RemoveListener(cb);
						}
						if (pd.type == GamePlayerType.Network)
						{
							p_event.target.fc.enabled = false;
							p_event.target.fc.external = true;
							d.body.frame.crash.Link();
						}
					}
				};
				d.OnEvent.AddListener(cb);
			}
			UnityEngine.Debug.Log("GameTypeController> CreatePlayer - type[" + pd.type.ToString() + "] sid[" + pd.playerId + "] name[" + pd.name + "] order[" + pd.order + "] id[" + pd.id + "] drone[" + d?.ToString() + "]\n" + ((pd.rig == null) ? "" : pd.rig.ToJson(p_indented: true)));
			return d;
		}

		public Drone CreatePlayer(GamePlayerData p_player, Action<Drone> p_on_ready)
		{
			return CreatePlayer(p_player, null, p_on_ready);
		}

		public Drone CreatePlayer(GamePlayerData p_player, DroneRigData p_default_rig)
		{
			return CreatePlayer(p_player, p_default_rig, null);
		}

		public Drone CreatePlayer(GamePlayerData p_player)
		{
			return CreatePlayer(p_player, null, null);
		}

		public void SetDroneFCMode(Drone p_drone, FCMode p_mode)
		{
			if (!p_drone || !p_drone.fc)
			{
				return;
			}
			if (base.app.inTournament)
			{
				if (!base.app.tournament.drlPilotMode && base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
				{
					base.app.model.storage.state.player.activeFCMode = FCMode.Pro;
					p_mode = FCMode.Pro;
				}
				else if (base.app.tournament.drlPilotMode)
				{
					base.app.model.storage.state.player.activeFCMode = FCMode.DRLPilot;
					p_mode = FCMode.DRLPilot;
				}
			}
			switch (p_mode)
			{
			case FCMode.Beginner:
				_ = p_drone.fc.mode;
				p_drone.fc.SetMode(FlightControllerMode.Beginner);
				p_drone.fc.Reset();
				p_drone.crashEnabled = false;
				base.app.model.storage.state.player.activeFCMode = FCMode.Beginner;
				break;
			case FCMode.Intermediate:
				p_drone.fc.SetMode(FlightControllerMode.Intermediate);
				p_drone.crashEnabled = false;
				break;
			case FCMode.Pro:
				p_drone.fc.SetMode(FlightControllerMode.Pro);
				p_drone.crashEnabled = false;
				break;
			case FCMode.DRLPilot:
				p_drone.fc.SetMode(FlightControllerMode.Pro);
				p_drone.invulnerable = -1f;
				p_drone.crashEnabled = true;
				p_drone.UseCrashDelay(type, mode == GameFlag.NetworkMultiplayer);
				break;
			case FCMode.Stabilized:
			case FCMode.Horizon:
				break;
			}
		}

		public void EnableRaceEndSlowmotion(Drone p_drone, float p_duration, bool p_centered, RaceStatusType p_status)
		{
			DroneCamera c = game.model.camera;
			if (!base.validContext || p_drone == null || c == null)
			{
				return;
			}
			float t = Mathf.Max(0f, p_duration);
			game.model.level.track.pathTrace.rendererEnabled = false;
			m_raceSlowmotionStarted = true;
			c.unscaledTime = true;
			p_drone.receiver.enabled = false;
			float num = 0f;
			float num2 = 0f;
			switch (p_status)
			{
			case RaceStatusType.Success:
			case RaceStatusType.Timeout:
			case RaceStatusType.Quit:
			case RaceStatusType.Forfeit:
				num2 = 0f;
				game.FadeTimescale(0.005f, num2);
				game.SetCameraMode(p_drone, c, GameCameraMode.Finish);
				c.orbit.distance = 0.55f;
				break;
			case RaceStatusType.Crash:
				num2 = 0.5f;
				game.SetCameraMode(p_drone, c, GameCameraMode.Finish);
				c.orbit.angle = new Vector2(0f, 30f);
				c.orbit.anchorRotation = Quaternion.LookRotation(p_drone.fc.sensor.inertial.velocity, Vector3.up);
				game.FadeTimescale(0.005f, num2);
				c.orbit.distance = 0.85f;
				t = 1f;
				break;
			}
			if (!p_centered)
			{
				Quaternion p_to = Quaternion.LookRotation(c.main.ViewportPointToRay(new Vector3(0.72f, 0.52f, 0f)).direction, c.main.transform.up);
				Tween.Add(c.main.transform, "rotation", p_to, t, 0.8f, Cubic.InOut);
			}
			c.fx.SetDOF(p_drone.transform, 9f, 35f);
			if (m_drone_slowmotion_stop != null)
			{
				m_drone_slowmotion_stop.Stop();
			}
			if (m_drone_slowmotion_complete != null)
			{
				m_drone_slowmotion_complete.Stop();
			}
			num = 0.8f + num2;
			c.npsnap.enabled = false;
			m_drone_slowmotion_complete = this.TimerRunOnce(delegate
			{
				if (base.validContext && !resetInProgress)
				{
					c.unscaledTime = false;
					game.FadeTimescale(1E-05f, t);
					m_drone_slowmotion_stop = this.TimerRunOnce(OnRaceSlowmotionStop, t);
					RunOnce(delegate
					{
						if (!(this == null) && base.validContext && !resetInProgress && !(p_drone == null))
						{
							UnityEngine.Debug.Log("GameTypeController> EnableRaceEndSlowmotion - Disable local drone.");
							p_drone.fc.armed = false;
							p_drone.SetMotorSpinSpeed(0f);
						}
					}, 2f, unscaledTime: true);
				}
			}, t + num);
			DroneSimulation simulation = game.model.simulation;
			if ((bool)simulation)
			{
				simulation.drones.SetCustomReflections(base.app.model.storage.state.player.settings.graphics.advancedRendering && base.app.model.storage.state.player.settings.graphics.eyeAdaptation);
			}
			OnRaceSlowmotionStart(t);
		}

		public void DisableRaceEndSlowmotion(Drone p_drone, bool p_use_anchor = false)
		{
			m_raceSlowmotionStarted = false;
			SplineTracerComponent pathTrace = game.model.level.track.pathTrace;
			if ((bool)pathTrace)
			{
				pathTrace.rendererEnabled = base.app.model.storage.state.player.settings.game.raceGuide;
			}
			if (m_drone_slowmotion_complete != null)
			{
				m_drone_slowmotion_complete.Stop();
			}
			Tween.Kill(game);
			game.timescale = 1f;
			DroneCamera camera = game.model.camera;
			Tween.Kill(camera.main.transform);
			camera.main.transform.localRotation = Quaternion.identity;
			camera.unscaledTime = true;
			camera.orbit.anchorRotation = Quaternion.identity;
			camera.orbit.angle = Vector2.zero;
			camera.npsnap.enabled = true;
			camera.fx.ClearDOF();
			if (p_use_anchor)
			{
				Transform closestFinishAnchor = game.model.level.track.GetClosestFinishAnchor(p_drone.position);
				camera.SetLOS(p_drone, closestFinishAnchor, 0f);
				camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
			}
			DroneSimulation simulation = game.model.simulation;
			if ((bool)simulation)
			{
				simulation.drones.SetCustomReflections(p_flag: false);
			}
			OnRaceSlowmotionComplete();
		}

		protected virtual void OnRaceSlowmotionStart(float p_duration)
		{
		}

		protected virtual void OnRaceSlowmotionStop()
		{
		}

		protected virtual void OnRaceSlowmotionComplete()
		{
		}

		public void Pause(bool p_flag, bool p_pause_physics, bool p_open_pause_screen = true)
		{
			DroneSimulation simulation = game.model.simulation;
			if ((bool)simulation)
			{
				DroneSimulationPauseMode pause = DroneSimulationPauseMode.Unpause;
				if (p_flag)
				{
					pause = (p_pause_physics ? DroneSimulationPauseMode.Pause : DroneSimulationPauseMode.PauseKeepPhysics);
				}
				simulation.pause = pause;
				DroneFlightController droneFlightController = (game.model.playerDrone ? game.model.playerDrone.fc : null);
				if (!p_flag)
				{
					simulation.drones.LoadArmed();
				}
				else if ((bool)droneFlightController && droneFlightController.armed)
				{
					simulation.drones.SaveArmed();
					simulation.drones.SetArmed(!p_pause_physics);
				}
				if ((bool)droneFlightController && mode == GameFlag.NetworkMultiplayer)
				{
					if (!p_flag)
					{
						droneFlightController.armed = true;
						droneFlightController.enabled = true;
					}
					else
					{
						droneFlightController.armed = false;
					}
				}
			}
			if (p_flag)
			{
				base.app.view.ui.game.hud.lowFPSWarning.enabled = false;
				base.app.view.ui.game.hud.Hide();
				if (p_open_pause_screen)
				{
					base.app.view.ui.screens.Open<UIPauseView>("game-pause-screen").ignoreReturn = true;
				}
			}
			else
			{
				base.app.view.ui.game.hud.lowFPSWarning.enabled = true;
				base.app.view.ui.game.hud.lowFPSWarning.GetComponent<FPSLowWarning>().Restart();
				base.app.view.ui.game.hud.Show();
				if (p_open_pause_screen)
				{
					base.app.view.ui.screens.Close("game-pause-screen");
				}
			}
			OnPause(p_flag, p_pause_physics);
		}

		public virtual void Pause(bool p_flag)
		{
			Pause(p_flag, p_pause_physics: true);
		}

		protected virtual void OnPause(bool p_flag, bool p_pause_physics)
		{
		}

		public virtual bool OnGameCommandChange(GameCommand p_from, GameCommand p_to)
		{
			return true;
		}

		public virtual bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			switch (p_command.type)
			{
			case GameCommandType.SwitchPhysicsDashboard:
				Notify("game.ui.dashboard@toggle");
				return false;
			case GameCommandType.Pause:
			{
				UIDroneDashboardController dashboard = base.app.view.ui.game.hud.dashboard;
				bool isShowing = dashboard.isShowing;
				bool openedFromPause = dashboard.openedFromPause;
				if (isShowing)
				{
					Notify("game.ui.dashboard@hide");
					if (openedFromPause)
					{
						Activity.RunOnce(delegate
						{
							base.app.controller.game.input.Post(GameCommandType.Pause);
						}, 0.05f);
					}
					return false;
				}
				UIScreen current = base.app.view.ui.screens.current;
				bool flag = (bool)current && current.name == "game-pause-screen";
				if (current != null && !flag)
				{
					return false;
				}
				return true;
			}
			default:
				return true;
			}
		}

		protected void ProcessReplays()
		{
			Drone d = game.model.playerDrone;
			if (m_process_rpl_thd != null)
			{
				m_process_rpl_thd.Abort();
			}
			m_process_rpl_thd = null;
			m_replay_stop_timer = RunOnce(delegate
			{
				if (!(this == null) && base.validContext)
				{
					UnityEngine.Debug.Log("GameTypeController> ProcessReplays / Replay Stop!");
					ReplayRecorderController recorder = game.replay.recorder;
					recorder.Pause();
					OnReplayComplete();
					if (ReplayFile.EnableVersion2)
					{
						ReplayFile rpl = recorder.model.GetReplay(d);
						if (rpl != null)
						{
							Stopwatch sw = new Stopwatch();
							sw.Start();
							int rpl_process_state = 0;
							string rpl_file_name = "";
							bool is_crash = rpl.header.GetEventCount(ReplayEventType.Crash) > 0;
							ThreadStart start = delegate
							{
								while (m_process_rpl_enabled)
								{
									Thread.Sleep(1);
									switch (rpl_process_state)
									{
									case 0:
										if (is_crash)
										{
											rpl.header.compressed = false;
										}
										if (m_process_rpl_enabled)
										{
											try
											{
												rpl.Serialize();
												if (rpl.file == null)
												{
													m_process_rpl_enabled = false;
													break;
												}
												rpl_file_name = rpl.file.Name;
											}
											catch (Exception ex2)
											{
												UnityEngine.Debug.LogWarning("GameTypeController> ProcessReplays / Replay Serialize Failed!\n" + ex2.Message);
												m_process_rpl_enabled = false;
												break;
											}
											rpl_process_state = 1;
										}
										break;
									case 1:
										if (m_process_rpl_enabled)
										{
											try
											{
												rpl.Deserialize(rpl_file_name);
											}
											catch (Exception ex)
											{
												UnityEngine.Debug.LogWarning("GameTypeController> ProcessReplays / Replay Deserialize Failed!\n" + ex.Message);
												m_process_rpl_enabled = false;
												break;
											}
											if (m_process_rpl_enabled)
											{
												UnityEngine.Debug.Log($"GameTypeController> ProcessReplays / Replay Processing Complete [{sw.ElapsedMilliseconds}ms]");
												sw.Stop();
												Activity.RunOnce(delegate
												{
													game.model.replayProcessActive = false;
													Notify("game.race.process-replay.complete");
													OnReplayWrite();
												}, 0.05f);
												m_process_rpl_enabled = false;
											}
										}
										break;
									}
								}
								m_process_rpl_enabled = false;
								m_process_rpl_thd = null;
							};
							m_process_rpl_thd = new Thread(start);
							m_process_rpl_enabled = true;
							m_process_rpl_thd.Start();
						}
					}
					else
					{
						recorder.model.record.Trim();
						recorder.model.record.ClearTrackTables();
						OnReplayWrite();
					}
				}
			}, 0.8f, unscaledTime: true);
		}

		protected void ReplayInit()
		{
			DroneSimulation simulation = game.model.simulation;
			if (!simulation)
			{
				UnityEngine.Debug.LogWarning("GameTypeController> ReplayInit / Simulation is <null>");
				return;
			}
			bool inTournament = base.app.inTournament;
			ReplayRecorderController recorder = game.replay.recorder;
			recorder.model.Clear();
			for (int i = 0; i < simulation.drones.list.Count; i++)
			{
				Drone drone = simulation.drones.list[i];
				if (drone != game.model.playerDrone)
				{
					continue;
				}
				bool flag = false;
				if (drone.rig.hasCustomPhysics && !inTournament)
				{
					flag = true;
					ui.hud.dashboard.Init();
					ui.hud.dashboard.Hide(p_all: true);
				}
				if (ReplayFile.EnableVersion2)
				{
					recorder.model.AddReplay(drone, p_is_player: true).header.isCustomPhysics = flag;
					continue;
				}
				BlackboxData blackboxData = recorder.model.Add(drone, p_is_player: true);
				if (flag)
				{
					blackboxData.SetPhysicsFlag(p_flag: true);
				}
			}
		}

		protected virtual void OnReplayHeaderWrite()
		{
			UnityEngine.Debug.Log("GameTypeController> OnReplayHeaderWrite");
			game.model.replayProcessActive = true;
			Notify("game.race.process-replay.start");
		}

		protected virtual void OnReplayComplete()
		{
			ReplayRecorderController recorder = game.replay.recorder;
			if (!ReplayFile.EnableVersion2)
			{
				recorder.Pause();
				recorder.model.record.Trim();
				recorder.model.record.ClearTrackTables();
			}
		}

		protected virtual void OnReplayWrite()
		{
			bool num = base.app.model.game.fromEditor || base.app.model.storage.state.player.profile.isDeveloper;
			string folder = (base.app.model.game.fromEditor ? DRLPaths.Storage.replaysMapEditorRoot : DRLPaths.Storage.replaysRoot);
			string hash = base.app.hash;
			if (game.model.type == GameFlag.Campaign)
			{
				CampaignController campaignController = this as CampaignController;
				if ((bool)campaignController && (bool)campaignController.model.campaign)
				{
					hash = hash + "_" + campaignController.model.campaign.guid;
				}
			}
			_ = base.app.model.service;
			if (num)
			{
				UnityEngine.Debug.Log("GameTypeController> OnReplayWrite [" + folder + hash + "]");
				game.replay.recorder.model.ToBytesAsync(delegate(byte[] fd)
				{
					string text = (ReplayFile.EnableVersion2 ? "rpl2" : "replay");
					File.WriteAllBytes(folder + hash + "." + text + ".bytes", fd);
				});
			}
		}

		protected virtual void OnReplayWriteData(byte[] p_replay_data)
		{
		}

		protected void CheatAssert()
		{
			bool flag = (bool)base.app.acs && base.app.acs.cheatEver;
			float num = (base.app.acs ? base.app.acs.avgRatio : 1f);
			int num2 = (base.app.acs ? base.app.acs.cheatCount : 0);
			string text = (base.app.acs ? base.app.acs.GetSamplesString() : "");
			cheatLog = "";
			string text2 = "GameTypeController> CheatAssert";
			text2 = text2 + "\n  cheat[" + flag + "] ratio[" + num + "] samples[" + text + "] count[" + num2 + "]";
			UnityEngine.Debug.Log(text2);
			if (!Application.isEditor && flag)
			{
				SlackDebugTool slackDebugTool = UnityEngine.Object.FindObjectOfType<SlackDebugTool>();
				if ((bool)slackDebugTool)
				{
					slackDebugTool.ReportToSlack($"<Cheat Warning>\nRatio: {num}\nCount: {num2}\nSamples: {text}", "");
				}
				cheatLog = $"{num};{text};{num2}";
			}
		}

		public virtual string GetRaceTitle()
		{
			return "Finished!";
		}

		public virtual void SetLeaderboard(Action<DRLLeaderboardData> p_callback, DroneRigData p_rig = null)
		{
			p_callback?.Invoke(null);
		}

		protected virtual void Update()
		{
			if (introComplete && (bool)game && (bool)game.model && (bool)game.model.playerDrone && game.model.playerDrone.hasFc && game.model.playerDrone.fc.armed)
			{
				flightTime += Time.deltaTime;
			}
		}

		protected virtual void OnDestroy()
		{
			if (m_process_rpl_thd != null)
			{
				UnityEngine.Debug.Log("GameTypeController> OnDestroy / Aborting Replay Serialization");
				m_process_rpl_thd.Abort();
			}
			m_process_rpl_enabled = false;
			m_process_rpl_thd = null;
		}

		protected virtual void StartCount(bool p_fast = false)
		{
			Notify("game.count@start");
			float num = (p_fast ? 0.4f : 1f);
			float num2 = 0.8f * num;
			ui.hud.gameTitle.Show(num2);
			num2 += 1.5f * num;
			Activity.RunOnce(delegate
			{
				SetCount(1, 3, p_play_audio: true, p_hide_title: false);
			}, num2);
			num2 += 1f * num;
			Activity.RunOnce(delegate
			{
				SetCount(2, 3, p_play_audio: true, p_hide_title: true);
			}, num2);
			num2 += 1f * num;
			Activity.RunOnce(delegate
			{
				SetCount(3, 3, p_play_audio: true, p_hide_title: false);
			}, num2);
		}

		protected virtual void SetCount(int p_current, int p_max, bool p_play_audio, bool p_hide_title)
		{
			Notify("game.count@step", p_current, p_max, p_play_audio, p_hide_title);
			if (p_current >= p_max)
			{
				Notify("game.count@complete");
			}
		}

		protected virtual void ApplyCount(int p_current, int p_max, bool p_play_audio = true, bool p_hide_title = false)
		{
			if (p_current >= p_max)
			{
				if (p_play_audio)
				{
					base.app.view.audio.PlayGameCountdownFinish();
				}
			}
			else if (p_play_audio)
			{
				base.app.view.audio.PlayGameCountdownTick();
			}
			ui.hud.counter.FadeLamp(p_current - 1, p_on: true);
			if (game.model.mode != GameFlag.NetworkMultiplayer && p_hide_title)
			{
				ui.hud.gameTitle.Hide(0.6f);
			}
		}

		protected virtual void OnCountComplete()
		{
			float num = 0.8f;
			bool controllerOverlay = base.app.model.storage.state.player.settings.game.controllerOverlay;
			ui.hud.controller.fade.Fade(controllerOverlay ? 1f : 0f, 0.25f);
			game.SetGCEnabled(p_flag: false);
			bool hotkeys = base.app.model.storage.state.player.settings.game.hotkeys;
			base.app.view.ui.game.hud.SetHotkeysEnabled(hotkeys);
			this.TimerRunOnce(ui.hud.counter.Clear, 0.3f + num);
			ui.hud.counter.fade.FadeOut(0.25f, num);
			base.app.model.game.simulation.drones.SetArmed(p_flag: true);
		}

		protected virtual void SetTitle()
		{
			UIHUDTitle gameTitle = ui.hud.gameTitle;
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			string p_caption_left = (track ? track.label : "");
			if ((bool)map && map.data != null)
			{
				p_caption_left = map.data.mapTitle.ToUpper();
			}
			gameTitle.Set(map.label, p_caption_left, base.app.model.storage.locale.Get("race-hud.title.get", "GET"), base.app.model.storage.locale.Get("race-hud.title.ready", "READY!"));
		}
	}
}
