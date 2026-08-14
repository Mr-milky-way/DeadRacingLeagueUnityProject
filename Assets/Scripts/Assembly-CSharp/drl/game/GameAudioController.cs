using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameAudioController : Controller<DRLApp>
	{
		private AudioView m_view;

		public bool ready;

		public List<Drone> actives;

		public float droneMotorVolume;

		public int playerId;

		protected Activity m_drone_slowmotion;

		private List<Component> m_drone_audio_list;

		private float[] m_drone_audio_rpms;

		private float[] m_rpms;

		private bool m_is_paused;

		public AudioView view
		{
			get
			{
				if (!m_view)
				{
					return m_view = base.app.view.audio;
				}
				return m_view;
			}
		}

		public GameController game => base.app.controller.game;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.ready":
				ready = true;
				view.ResetGameRadioSignal();
				view.StopUICounterLoop();
				break;
			case "scene.start":
			case "scene.game.scenes@complete":
			case "scene.game.scenes@start":
				view.StopUICounterLoop();
				break;
			case "game.simulation.drone@ready":
			{
				Drone drone5 = Reflection<object>.Get<Drone>(p_data, 0);
				if (drone5.fc.armed)
				{
					PlayDroneMotor(drone5);
				}
				break;
			}
			case "game.simulation.drone@armed":
			{
				Drone d = Reflection<object>.Get<Drone>(p_data, 0);
				if (!d)
				{
					break;
				}
				if (!actives.Contains(d))
				{
					actives.Add(d);
				}
				UpdatePlayerId();
				if (d.ready)
				{
					PlayDroneMotor(d);
				}
				RunOnce(0.1f, delegate
				{
					if ((bool)d && d.ready)
					{
						if (actives.Contains(d))
						{
							actives.Remove(d);
						}
						StopDroneMotor(d);
					}
				});
				RunOnce(0.2f, delegate
				{
					if ((bool)d && d.ready)
					{
						if (!actives.Contains(d))
						{
							actives.Add(d);
						}
						PlayDroneMotor(d);
					}
				});
				break;
			}
			case "game.simulation.drone@remove":
			case "game.simulation.drone@disarmed":
			{
				Drone drone4 = Reflection<object>.Get<Drone>(p_data, 0);
				if ((bool)drone4)
				{
					if (actives.Contains(drone4))
					{
						actives.Remove(drone4);
					}
					UpdatePlayerId();
					StopDroneMotor(drone4);
				}
				break;
			}
			case "map-editor.camera.signal-full":
			case "game.drone.signal-full":
				view.StopGameRadioSignal();
				break;
			case "map-editor.camera.signal-recover":
			case "game.drone.signal-recover":
				view.StopGameRadioSignal();
				break;
			case "map-editor.camera.signal-drop":
			case "game.drone.signal-drop":
				view.PlayGameRadioSignal();
				break;
			case "map-editor.camera.signal-update":
			case "game.drone.signal-update":
			{
				Drone drone2 = Reflection<object>.Get<Drone>(p_data, 0);
				if ((bool)drone2)
				{
					float p_signal_strength = Reflection<object>.Get((IList)p_data, 2, 1f);
					view.UpdateGameRadioSignal(p_signal_strength, drone2.gameObject);
				}
				break;
			}
			case "fn.mission.drone.spawn":
				view.PlayDroneSpawn(base.gameObject);
				break;
			case "fn.mission.drone.rescue":
				view.PlayDroneRespawn(base.gameObject);
				break;
			case "fn.mission.target@hit":
			{
				ColliderEventComponent p_target2 = Reflection<object>.Get<ColliderEventComponent>(p_data, 0);
				PlayMissionTargetHit(p_target2);
				break;
			}
			case "fn.mission.precision@start":
				base.app.view.audio.PlayGameRadar();
				break;
			case "fn.mission.precision@update":
			{
				if (base.app.model.game.paused)
				{
					view.StopGameRadar();
					base.app.view.audio.UpdateGameRadar(0f);
					break;
				}
				float p_proximity = Reflection<object>.Get<float>(p_data, 1);
				if (!view.IsPlaying("game-radar"))
				{
					view.PlayGameRadar();
				}
				base.app.view.audio.UpdateGameRadar(p_proximity);
				break;
			}
			case "fn.mission.precision@stop":
				base.app.view.audio.StopGameRadar();
				break;
			case "fn.mission.balloonradar@start":
			{
				Balloon balloon2 = Reflection<object>.Get<Balloon>(p_data, 0);
				base.app.view.audio.PlayGameBalloonRadar(balloon2.gameObject);
				break;
			}
			case "fn.mission.balloonradar@stop":
			{
				Balloon balloon = Reflection<object>.Get<Balloon>(p_data, 0);
				base.app.view.audio.StopGameBalloonRadar(balloon.gameObject);
				break;
			}
			case "game.pause":
				view.PlayUIPause();
				SetPauseStatus(p_flag: true);
				break;
			case "game.unpause":
				view.PlayUIPause();
				SetPauseStatus(p_flag: false);
				break;
			case "game.ui.dashboard@show":
				view.PlayUIScreenForward();
				SetPauseStatus(p_flag: true);
				break;
			case "game.ui.dashboard@hide":
				view.PlayUIScreenBackward();
				SetPauseStatus(p_flag: false);
				break;
			case "game.race.gate@step":
			{
				int num = Reflection<object>.Get<int>(p_data, 0);
				int num2 = Reflection<object>.Get<int>(p_data, 1);
				Drone drone3 = Reflection<object>.Get<Drone>(p_data, 3);
				if (base.app.model.game.playerDrone == drone3)
				{
					float num3 = ((num2 <= 0) ? 0f : ((float)num / (float)(num2 - 1)));
					base.app.view.audio.UpdateRaceGatesPercentage(num3 * 100f);
				}
				break;
			}
			case "game.race-complete.time.animation@start":
				if (!base.validContext)
				{
					view.StopUICounterLoop();
				}
				else
				{
					view.PlayUICounterLoop();
				}
				break;
			case "game.race-complete.time.animation@complete":
				view.StopUICounterLoop();
				break;
			case "game.race.slowmo@start":
			{
				Drone p_drone = Reflection<object>.Get<Drone>(p_data, 0);
				float p_length = Reflection<object>.Get<float>(p_data, 1);
				OnSlowmotionStart(p_drone, p_length);
				break;
			}
			case "network.room.first-racer-finshed":
				view.PlayUINewResultLine();
				break;
			case "network.race.end":
				StopDroneMotors();
				break;
			case "network.remote.drone.finished":
			{
				Drone drone = Reflection<object>.Get<Drone>(p_data, 0);
				if (drone.hasBody && !(drone.body.frame == null))
				{
					view.StopDroneMotor(drone.body.frame.gameObject);
				}
				break;
			}
			}
		}

		protected void Update()
		{
			UpdateDroneMotors();
		}

		public void ClearDroneAudioList()
		{
			m_drone_audio_list = null;
		}

		private void UpdatePlayerId()
		{
			playerId = (game.model.playerDrone ? actives.IndexOf(game.model.playerDrone) : (-1));
		}

		protected void UpdateDroneMotors()
		{
			if (!ready)
			{
				return;
			}
			DroneSimulation simulation = game.model.simulation;
			if (!simulation)
			{
				return;
			}
			List<Drone> list = simulation.drones.list;
			List<DroneInputTransmitter> list2 = simulation.transmitters.list;
			List<ReplayClipPlayerModel> clips = game.model.replay.player.clips;
			if (m_drone_audio_list == null)
			{
				m_drone_audio_list = new List<Component>();
				for (int i = 0; i < 40; i++)
				{
					m_drone_audio_list.Add(null);
				}
			}
			if (m_drone_audio_rpms == null)
			{
				m_drone_audio_rpms = new float[4];
			}
			List<Component> drone_audio_list = m_drone_audio_list;
			int num = 0;
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				m_drone_audio_list[num++] = list[j];
			}
			count = list2.Count;
			for (int k = 0; k < count; k++)
			{
				m_drone_audio_list[num++] = list2[k];
			}
			count = clips.Count;
			for (int l = 0; l < count; l++)
			{
				m_drone_audio_list[num++] = clips[l];
			}
			float[] drone_audio_rpms = m_drone_audio_rpms;
			float[] array = null;
			for (int m = 0; m < num; m++)
			{
				Component component = drone_audio_list[m];
				Drone drone = null;
				int num2 = -1;
				drone_audio_rpms[0] = (drone_audio_rpms[1] = (drone_audio_rpms[2] = (drone_audio_rpms[3] = 0f)));
				if (component is ReplayClipPlayerModel)
				{
					num2 = 0;
				}
				if (component is DroneInputTransmitter)
				{
					num2 = 1;
				}
				if (component is DroneRCTransmitter)
				{
					num2 = 2;
				}
				if (component is DroneGhostTransmitter)
				{
					num2 = 3;
				}
				if (component is DroneNetworkTransmitter)
				{
					num2 = 4;
				}
				bool flag = false;
				bool flag2 = false;
				bool flag3 = true;
				if (component is DroneInputTransmitter)
				{
					drone = (component as DroneInputTransmitter).drone;
					flag = (bool)drone && (!drone.hasFc || !drone.fc.armed);
				}
				switch (num2)
				{
				case 0:
				{
					ReplayClipPlayerModel replayClipPlayerModel = component as ReplayClipPlayerModel;
					drone = replayClipPlayerModel.drone;
					if (replayClipPlayerModel.IsPlaying() && !replayClipPlayerModel.IsPaused())
					{
						array = replayClipPlayerModel.rpm;
					}
					break;
				}
				case 1:
					flag3 = false;
					break;
				case 2:
					flag2 = true;
					flag3 = false;
					break;
				case 3:
					array = (component as DroneGhostTransmitter).rpm;
					break;
				case 4:
					array = (component as DroneNetworkTransmitter).NetworkRPMs;
					break;
				}
				if (array != null && flag3)
				{
					int num3 = array.Length;
					for (int n = 0; n < 4; n++)
					{
						drone_audio_rpms[n] = ((n >= num3) ? ((num3 <= 0) ? 0f : array[0]) : array[n]);
					}
				}
				if (flag)
				{
					StopDroneMotor(drone);
					continue;
				}
				if (flag3)
				{
					UpdateDroneSounds(drone, drone_audio_rpms);
				}
				else
				{
					UpdateDroneSounds(drone);
				}
				if (flag2)
				{
					UpdateDroneWindSounds(drone);
				}
			}
		}

		protected void StopDroneMotors()
		{
			if (!ready)
			{
				return;
			}
			if (m_drone_slowmotion != null)
			{
				m_drone_slowmotion.Stop();
			}
			DroneSimulation simulation = base.app.model.game.simulation;
			if ((bool)simulation)
			{
				List<Drone> list = simulation.drones.list;
				for (int i = 0; i < list.Count; i++)
				{
					Drone p_drone = list[i];
					StopDroneMotor(p_drone);
				}
				view.StopEnvWind();
			}
		}

		protected void UpdateDroneSounds(Drone p_drone)
		{
			if (m_drone_slowmotion != null)
			{
				return;
			}
			if (!p_drone || !p_drone.ready)
			{
				return;
			}
			int count = p_drone.body.frame.escs.Count;
			if (m_rpms == null)
			{
				m_rpms = new float[count];
			}
			if (m_rpms.Length != count)
			{
				m_rpms = new float[count];
			}
			float[] rpms = m_rpms;
			if (p_drone.fc.mode == FlightControllerMode.DJI || p_drone.fc.mode == FlightControllerMode.Beginner)
			{
				_ = 1;
			}
			else
				_ = p_drone.fc.mode == FlightControllerMode.Stabilized;
			bool armed = p_drone.fc.armed;
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = p_drone.body.frame.escs[i];
				if ((bool)droneESC)
				{
					DroneMotor motor = droneESC.motor;
					if (droneESC.hasMotor)
					{
						rpms[i] = (armed ? motor.rpmAudioRatio : 0f);
					}
				}
			}
			UpdateDroneSounds(p_drone, rpms);
		}

		protected void UpdateDroneSounds(Drone p_drone, float[] p_rpms)
		{
			if (!p_drone || !p_drone.ready || p_drone.body.frame.escs == null || p_rpms == null || p_rpms.Length == 0)
			{
				return;
			}
			int num = Mathf.Min(p_drone.body.frame.escs.Count, p_rpms.Length);
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < num; i++)
			{
				DroneESC droneESC = p_drone.body.frame.escs[i];
				if ((bool)droneESC && droneESC.hasMotor)
				{
					_ = droneESC.motor;
					float num4 = p_rpms[i] * 1f;
					if (num4 * 100f > num2)
					{
						num2 = num4 * 100f;
					}
					num3 += num4 * 100f;
				}
			}
			num3 = (num3 + num2) / (float)(num + 1);
			DroneESC droneESC2 = p_drone.body.frame.escs[0];
			if ((bool)droneESC2 && droneESC2.hasMotor)
			{
				if (p_drone.hasFc && (p_drone.fc.mode == FlightControllerMode.DJI || p_drone.fc.mode == FlightControllerMode.Beginner || p_drone.fc.mode == FlightControllerMode.Stabilized))
				{
					view.UpdateDroneMotor(droneESC2.motor.gameObject, num2, num3);
				}
				else
				{
					view.UpdateDroneMotor(droneESC2.motor.gameObject, num2, num3, 0f);
				}
			}
		}

		protected void UpdateDroneWindSounds(Drone p_drone)
		{
			if ((bool)p_drone && p_drone.ready)
			{
				float num = 1f;
				bool flag = true;
				if (p_drone.fc.sensor.inertial == null)
				{
					flag = false;
				}
				if (p_drone.fc.sensor.gyro == null)
				{
					flag = false;
				}
				float speed_kph = 0f;
				float rotation_speed_dps_m = 0f;
				float rotation_speed_dps_z = 0f;
				if (flag)
				{
					speed_kph = p_drone.fc.sensor.inertial.speedKph * num;
				}
				if (flag)
				{
					rotation_speed_dps_m = p_drone.fc.sensor.gyro.averageVelocity.magnitude * num;
				}
				if (flag)
				{
					rotation_speed_dps_z = p_drone.fc.sensor.gyro.averageVelocity.z * num;
				}
				DroneESC droneESC = p_drone.body.frame.escs[0];
				if ((bool)droneESC && droneESC.hasMotor)
				{
					view.UpdateDroneSpeed(speed_kph, rotation_speed_dps_m, rotation_speed_dps_z, droneESC.motor.gameObject);
				}
			}
		}

		public void PlayDroneMotor(List<Drone> p_drones)
		{
			for (int i = 0; i < p_drones.Count; i++)
			{
				PlayDroneMotor(p_drones[i]);
			}
		}

		public void PlayDroneMotor(Drone p_drone)
		{
			if (!base.validContext)
			{
				return;
			}
			if (!p_drone)
			{
				return;
			}
			bool flag = false;
			int num = 0;
			float num2 = 0f;
			float num3 = 0f;
			string text = "";
			List<DroneESC> list = ((!p_drone.hasBody) ? null : (p_drone.body.hasFrame ? p_drone.body.frame.escs : null));
			if (list == null)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				DroneESC droneESC = p_drone.body.frame.escs[i];
				if (!droneESC || !droneESC.hasMotor || flag)
				{
					continue;
				}
				DroneMotor motor = droneESC.motor;
				if ((bool)motor)
				{
					flag = true;
					DroneProp prop = motor.prop;
					if ((bool)prop)
					{
						text = (motor.hasProp ? DronePropTypePrefix.FromEnum(prop.type) : "PN");
						num = (motor.hasProp ? prop.blades : 3);
						num2 = (motor.hasProp ? prop.diameter : 5f);
						num3 = (motor.hasProp ? prop.pitch : 4f);
						view.SetDroneMotorPropState(motor.gameObject, text, num, num2, num3);
						view.PlayDroneMotor(motor.gameObject);
					}
				}
			}
		}

		public void StopDroneMotor(List<Drone> p_drones)
		{
			for (int i = 0; i < p_drones.Count; i++)
			{
				StopDroneMotor(p_drones[i]);
			}
		}

		public void StopDroneMotor(Drone p_drone)
		{
			if (!p_drone || !p_drone.ready)
			{
				return;
			}
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = p_drone.body.frame.escs[i];
				if ((bool)droneESC)
				{
					DroneMotor motor = droneESC.motor;
					if (droneESC.hasMotor && !(motor == null))
					{
						view.StopDroneMotor(motor.gameObject);
					}
				}
			}
		}

		protected void OnSlowmotionStart(Drone p_drone, float p_length)
		{
			if (m_drone_slowmotion != null)
			{
				m_drone_slowmotion.Stop();
			}
			Drone drone = p_drone;
			if (!drone || !drone.ready)
			{
				return;
			}
			int len = drone.body.frame.escs.Count;
			float[] rpms = new float[len];
			float[] rpmsStart = new float[len];
			for (int i = 0; i < drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = drone.body.frame.escs[i];
				if ((bool)droneESC)
				{
					DroneMotor motor = droneESC.motor;
					if (droneESC.hasMotor)
					{
						rpms[i] = (drone.fc.armed ? motor.rpmRatio : 0f);
						rpmsStart[i] = rpms[i];
					}
				}
			}
			float duration = 0f;
			m_drone_slowmotion = Activity.Run((Func<bool>)delegate
			{
				duration += Time.unscaledDeltaTime;
				if (duration > p_length)
				{
					m_drone_slowmotion = null;
					for (int j = 0; j < len; j++)
					{
						rpms[j] = 0f;
					}
					UpdateDroneSounds(p_drone, rpms);
					return false;
				}
				for (int k = 0; k < len; k++)
				{
					rpms[k] = Mathf.Lerp(rpmsStart[k], 0f, duration / p_length);
				}
				UpdateDroneSounds(p_drone, rpms);
				return true;
			}, 0f, false);
		}

		public void PlayDroneDamage(Drone p_drone, float p_intensity)
		{
			if (!(p_drone == null) && p_drone.ready)
			{
				view.PlayDronePartHit(p_drone.gameObject, p_intensity);
			}
		}

		protected void PlayMissionTargetHit(ColliderEventComponent p_target)
		{
			if (!p_target)
			{
				return;
			}
			MissionTargetTag component = p_target.GetComponent<MissionTargetTag>();
			if ((bool)component)
			{
				switch ((component.tags.Count > 0) ? component.tags[0] : MissionTargetType.None)
				{
				case MissionTargetType.Balloon:
					view.PlayGameBalloon(component.gameObject);
					break;
				case MissionTargetType.Gate:
				case MissionTargetType.Pole:
				case MissionTargetType.Checkpoint:
					view.PlayGameGateValid();
					break;
				case MissionTargetType.None:
				case MissionTargetType.PrecisionGround:
				case MissionTargetType.PrecisionAir:
					break;
				}
			}
		}

		protected void SetPauseStatus(bool p_flag)
		{
			if ((!m_is_paused || !p_flag) && (m_is_paused || p_flag))
			{
				m_is_paused = p_flag;
				view.UpdateGameStatus(p_flag ? "paused" : "playing");
			}
		}

		protected void OnDestroy()
		{
			StopDroneMotors();
		}
	}
}
