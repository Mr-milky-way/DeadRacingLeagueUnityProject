using System.Collections.Generic;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	public class DroneGhostTransmitter : DroneInputTransmitter
	{
		private BlackboxData m_data;

		private ReplayFile m_dataV2;

		public byte gateFrameFlag = 1;

		public float speed = 1f;

		public Vector3 position;

		public Quaternion rotation;

		public float[] rpm;

		public Vector4 input;

		public Vector3 velocity;

		public Vector3 pid;

		public float torque;

		public float[] thrust;

		public Vector3 dragFactors;

		public Vector3 dragForce;

		public float elapsed;

		public int gateIndex = -1;

		public float duration;

		private float m_raceTime;

		public Vector3 podium;

		public Quaternion podiumRotation;

		public Vector3 startPosition;

		public Quaternion startRotation = Quaternion.identity;

		public bool usePodium;

		public float podiumBlendDuration = 2f;

		public ControllerStateType controller = ControllerStateType.Taranis;

		public List<BlackboxFrame> gateFrames;

		public List<ReplayEvent> gateEvents;

		private bool m_use_physics;

		public bool enablePhysicsOnComplete;

		public BlackboxData data
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
				if (m_data == null)
				{
					return;
				}
				m_data.ParseTracks();
				gateFrames = new List<BlackboxFrame>();
				gateFrames = m_data.GetFrames(32);
				gateFrames.RemoveAll((BlackboxFrame it) => (byte)it.data[0] != gateFrameFlag);
				duration = m_data.elapsed;
				SerializedData header = m_data.header;
				if (header != null)
				{
					raceTime = header.Get("race-time", 0f);
					controller = (ControllerStateType)header.Get("controller-type", 2);
					usePodium = true;
					byte key = 1;
					if (m_data.tracks.ContainsKey(key))
					{
						BlackboxData.Sample(m_data.tracks[key], 0f, p_smooth: true).GetTransform(out startPosition, out startRotation);
					}
				}
			}
		}

		public ReplayFile dataV2
		{
			get
			{
				return m_dataV2;
			}
			set
			{
				m_dataV2 = value;
				if (m_dataV2 == null)
				{
					return;
				}
				ReplayFile replayFile = m_dataV2;
				gateEvents = new List<ReplayEvent>();
				for (int i = 0; i < replayFile.header.events.Count; i++)
				{
					ReplayEvent replayEvent = replayFile.header.events[i];
					if (replayEvent.typeFlag == ReplayEventType.Gate)
					{
						gateEvents.Add(replayEvent);
					}
				}
				duration = replayFile.duration;
				ReplayHeader header = replayFile.header;
				if (header != null)
				{
					raceTime = header.raceTime;
					controller = header.controllerTypeFlag;
					usePodium = true;
					replayFile.Seek(0.16f);
					startPosition = replayFile.EvaluateVector3(ReplayChannelIds.DronePos, 0.5f);
					startRotation = replayFile.EvaluateQuaternion(ReplayChannelIds.DroneQuat, 0.5f);
				}
			}
		}

		public Vector2 leftInput => new Vector2(input.x, input.y);

		public Vector2 rightInput => new Vector2(input.z, input.w);

		public float raceTime
		{
			get
			{
				if (speed == 0f)
				{
					return 180f;
				}
				return m_raceTime / speed;
			}
			private set
			{
				m_raceTime = value;
			}
		}

		public float raceCompletion
		{
			get
			{
				if (!(raceTime <= 0f))
				{
					return elapsed / raceTime;
				}
				return 0f;
			}
		}

		public bool usePhysics
		{
			get
			{
				return m_use_physics;
			}
			set
			{
				m_use_physics = value;
				Drone drone = base.drone;
				if ((bool)drone && drone.ready)
				{
					drone.enabled = !m_use_physics;
					drone.rigidbody.rb.constraints = ((!m_use_physics) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None);
					drone.rigidbody.isKinematic = !m_use_physics;
					drone.rigidbody.rb.useGravity = m_use_physics;
					drone.rigidbody.rb.velocity = (m_use_physics ? velocity : Vector3.zero);
					drone.rigidbody.SetCollisionEnabled(m_use_physics);
					if ((bool)drone.fc)
					{
						drone.fc.gameObject.SetActive(value: false);
					}
				}
			}
		}

		protected override void Start()
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			velocity = Vector3.zero;
			input = Vector4.zero;
			pid = Vector3.zero;
			rpm = new float[0];
			float num = 0.05f;
			num = 0f;
			speed = Random.Range(speed - num, speed + num);
		}

		public override ControllerStateType GetControllerType()
		{
			return controller;
		}

		protected override void OnUpdate(float p_dt)
		{
			if ((ReplayFile.EnableVersion2 && dataV2 == null) || (!ReplayFile.EnableVersion2 && data == null))
			{
				return;
			}
			Seek(elapsed);
			elapsed = Mathf.Clamp(elapsed + p_dt * speed, 0f, duration);
			if (!(elapsed >= duration) || !enablePhysicsOnComplete || usePhysics)
			{
				return;
			}
			Debug.Log($"DroneGhostTransmitter> OnUpdate / Replay Complete [{drone}]");
			if ((bool)drone)
			{
				DroneCollisionSleep droneCollisionSleep = drone.GetComponent<DroneCollisionSleep>();
				if (!droneCollisionSleep)
				{
					droneCollisionSleep = drone.gameObject.AddComponent<DroneCollisionSleep>();
				}
				droneCollisionSleep.Clear();
			}
			usePhysics = true;
		}

		public int SeekGateIndex(float p_time)
		{
			if (ReplayFile.EnableVersion2)
			{
				int result = 0;
				for (int i = 0; i < gateEvents.Count; i++)
				{
					if (gateEvents[i].time > p_time)
					{
						return i;
					}
				}
				return result;
			}
			object[] array = BlackboxData.Sample(gateFrames, p_time, p_smooth: false)?.data;
			if (array != null)
			{
				if (array.Length >= 5)
				{
					return (int)array[4];
				}
				return -1;
			}
			return -1;
		}

		public void Seek(float p_time)
		{
			if (ReplayFile.EnableVersion2)
			{
				ReplayFile replayFile = dataV2;
				float p_ratio = replayFile.Seek(p_time);
				if (rpm == null)
				{
					rpm = new float[4];
				}
				if (rpm.Length < 4)
				{
					rpm = new float[4];
				}
				if (thrust == null)
				{
					thrust = new float[4];
				}
				if (thrust.Length < 4)
				{
					thrust = new float[4];
				}
				position = replayFile.EvaluateVector3(ReplayChannelIds.DronePos, p_ratio);
				rotation = replayFile.EvaluateQuaternion(ReplayChannelIds.DroneQuat, p_ratio);
				velocity = replayFile.EvaluateVector3(ReplayChannelIds.DroneVel, p_ratio);
				Vector4 vector = replayFile.EvaluateVector4(ReplayChannelIds.Drone4RPM, p_ratio);
				rpm[0] = vector[0];
				rpm[1] = vector[1];
				rpm[2] = vector[2];
				rpm[3] = vector[3];
				input = replayFile.EvaluateVector4(ReplayChannelIds.Input, p_ratio);
				pid = replayFile.EvaluateVector3(ReplayChannelIds.DronePID, p_ratio);
				dragFactors = replayFile.EvaluateVector3(ReplayChannelIds.DroneDrag, p_ratio);
				dragForce = replayFile.EvaluateVector3(ReplayChannelIds.DroneDragForce, p_ratio);
				vector = replayFile.EvaluateVector4(ReplayChannelIds.Drone4Thrust, p_ratio);
				thrust[0] = vector[0];
				thrust[1] = vector[1];
				thrust[2] = vector[2];
				thrust[3] = vector[3];
				torque = replayFile.EvaluateFloat("drone-torque", p_ratio);
				gateIndex = SeekGateIndex(p_time);
			}
			else
			{
				BlackboxData blackboxData = data;
				byte key = 1;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					blackboxFrame.GetTransform(out position, out rotation);
				}
				key = 2;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					velocity = blackboxFrame.GetVector3();
				}
				key = 4;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					rpm = blackboxFrame.GetFloats();
				}
				key = 8;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					input = blackboxFrame.GetVector4();
				}
				key = 16;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					pid = blackboxFrame.GetVector3();
				}
				key = 64;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time, p_smooth: true);
					blackboxFrame.GetPhysics(out dragFactors, out dragForce, out thrust, out torque);
				}
				gateIndex = SeekGateIndex(p_time);
			}
			UpdateDrone();
		}

		public void UpdateDrone()
		{
			Drone drone = base.drone;
			if (!drone || !drone.ready)
			{
				return;
			}
			Vector3 b = position;
			if (true)
			{
				Vector3 a = (usePodium ? podium : startPosition);
				Quaternion a2 = (usePodium ? podiumRotation : startRotation);
				if (Mathf.Abs(a.y - startPosition.y) < 0.35f)
				{
					a.y = startPosition.y;
				}
				float f = ((podiumBlendDuration <= 0f) ? 1f : Mathf.Clamp01(elapsed / podiumBlendDuration));
				b = Vector3.Lerp(a, b, Mathf.Pow(f, 0.7f));
				rotation = Quaternion.Lerp(a2, rotation, Mathf.Pow(f, 0.7f));
			}
			if (!m_use_physics)
			{
				drone.position = b;
				drone.transform.rotation = rotation;
			}
			int num = Mathf.Min(drone.body.frame.escs.Count, (rpm != null) ? rpm.Length : 0);
			for (int i = 0; i < num; i++)
			{
				DroneESC droneESC = drone.body.frame.escs[i];
				float num2 = droneESC.motor.rpmMax * rpm[i];
				droneESC.motor.rpm = (m_use_physics ? 0f : num2);
				droneESC.motor.rpmAudio = (m_use_physics ? 0f : num2);
			}
		}

		public override string GetPrefix()
		{
			return "gh";
		}
	}
}
