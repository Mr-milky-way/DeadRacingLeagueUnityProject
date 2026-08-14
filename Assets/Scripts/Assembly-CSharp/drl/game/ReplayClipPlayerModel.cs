using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ReplayClipPlayerModel : Model<DRLApp>
	{
		private ReplayPlayerModel m_parent;

		private BlackboxData m_clip;

		private ReplayFile m_clipV2;

		public GamePlayerData player;

		public Drone drone;

		public GameFlag type;

		public ControllerStateType controller;

		public float raceTime;

		public Vector3 position;

		public Quaternion rotation;

		public List<Vector3> positionParts;

		public List<Quaternion> rotationParts;

		public List<List<BlackboxFrame>> framesParts;

		public float[] rpm;

		public Vector4 input;

		public Vector3 velocity;

		public Vector3 pid;

		public float torque;

		public float[] thrust;

		public float[] partsBuffer;

		public Vector3 dragFactors;

		public Vector3 dragForce;

		public Vector3 podium;

		public Quaternion podiumRotation;

		public Vector3 startPosition;

		public Quaternion startRotation;

		public bool usePodium;

		public float podiumBlendDuration = 2f;

		public float tilt;

		[SerializeField]
		private float m_elapsed;

		public float crashTime = -1f;

		[Range(0f, 3f)]
		public float speed = 1f;

		public float duration;

		public bool reverse;

		public bool playing;

		public bool paused;

		public List<CollectFrameEvent> collects;

		internal List<ActionEventData> actions;

		private bool m_is_crash_active;

		public ReplayPlayerModel parent
		{
			get
			{
				if (!m_parent)
				{
					return m_parent = AssertParent<ReplayPlayerModel>("parent");
				}
				return m_parent;
			}
			set
			{
				m_parent = value;
			}
		}

		public GameModel game => base.app.model.game;

		public BlackboxData clip
		{
			get
			{
				return m_clip;
			}
			set
			{
				Clear(p_drone: false);
				m_clip = value;
				if (m_clip == null)
				{
					return;
				}
				m_clip.ParseTracks();
				GamePlayerData pd = (player = new GamePlayerData());
				duration = m_clip.elapsed;
				SerializedData header = m_clip.header;
				if (header == null)
				{
					return;
				}
				string text = "profile-tournament-color";
				if (!header.ContainsKey(text))
				{
					Debug.LogWarning("ReplayClipPlayerModel> get-clip / Profile Color [" + text + "] not found!");
					text = "profile-color";
				}
				string text2 = header.Get("drone-rig", "");
				if (!string.IsNullOrEmpty(text2))
				{
					pd.rig = DroneRigData.FromJson(text2);
				}
				raceTime = header.Get("race-time", 0f);
				type = (GameFlag)header.Get("game-type", 14);
				controller = (ControllerStateType)header.Get("controller-type", 2);
				tilt = header.Get("camera-tilt", 30f);
				pd.order = header.Get("order", 0);
				pd.playerId = header.Get("player-id", "");
				pd.platformId = header.Get(DRLService.PlatformIdKey, "");
				pd.name = header.Get("profile-name", "");
				pd.upperName = pd.name.ToUpper();
				pd.color = Colorf.ParseRGB(header.Get(text, "ff0000"));
				pd.photo = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false, QualitySettings.activeColorSpace == ColorSpace.Linear);
				pd.photo.SetPixel(0, 0, Colorf.transparent);
				pd.photo.Apply();
				string p_id = (string.IsNullOrEmpty(pd.playerId) ? pd.platformId : pd.playerId);
				base.app.model.service.GetPlayerAvatar(p_id, delegate(Texture2D p_result)
				{
					if ((bool)p_result)
					{
						pd.photo.Resize(p_result.width, p_result.width, p_result.format, hasMipMap: false);
						pd.photo.LoadRawTextureData(p_result.GetRawTextureData());
						pd.photo.Apply();
					}
				});
				if (pd.rig == null)
				{
					return;
				}
				byte key = 1;
				List<BlackboxFrame> p_frames;
				if (m_clip.tracks.ContainsKey(key))
				{
					p_frames = m_clip.tracks[key];
					BlackboxData.Sample(p_frames, 0f, p_smooth: true).GetTransform(out startPosition, out startRotation);
				}
				key = 32;
				actions = new List<ActionEventData>();
				if (m_clip.tracks.ContainsKey(key))
				{
					p_frames = m_clip.tracks[key];
					for (int num = 0; num < p_frames.Count; num++)
					{
						BlackboxFrame blackboxFrame = p_frames[num];
						byte num2 = Reflection<object>.Get<byte>(blackboxFrame.data, 0);
						float x = Reflection<object>.Get<float>(blackboxFrame.data, 1);
						float y = Reflection<object>.Get<float>(blackboxFrame.data, 2);
						float z = Reflection<object>.Get<float>(blackboxFrame.data, 3);
						Vector4 vector = new Vector4(x, y, z, blackboxFrame.time);
						object[] data = Reflection<object>.Get<object[]>(blackboxFrame.data, 4);
						if (num2 == 6)
						{
							ActionEventData item = new ActionEventData
							{
								@event = vector,
								data = data
							};
							actions.Add(item);
						}
					}
				}
				for (int num3 = 0; num3 < game.level.track.actions.Count; num3++)
				{
					MapAssetAction mapAssetAction = game.level.track.actions[num3];
					if ((bool)mapAssetAction)
					{
						switch (mapAssetAction.tag)
						{
						case GameFlag.ActionBreakGlass:
							mapAssetAction.gameObject.SetActive(value: false);
							break;
						case GameFlag.ActionNone:
							mapAssetAction.gameObject.SetActive(value: true);
							break;
						}
					}
				}
				for (int num4 = 0; num4 < actions.Count; num4++)
				{
					ActionEventData actionEventData = actions[num4];
					int actionIndex = actionEventData.actionIndex;
					MapAssetAction mapAssetAction2 = ((actionIndex < 0) ? null : game.level.track.actions[actionIndex]);
					if ((bool)mapAssetAction2)
					{
						mapAssetAction2.gameObject.SetActive(value: true);
						if (mapAssetAction2.tag == GameFlag.ActionBreakGlass)
						{
							mapAssetAction2.evaluateStartTime = actionEventData.@event.w;
						}
					}
				}
				bool p_async = false;
				List<DroneCrashNode> nodes = this.drone.body.frame.crash.nodes;
				positionParts = new List<Vector3>();
				rotationParts = new List<Quaternion>();
				for (int num5 = 0; num5 < nodes.Count + 1; num5++)
				{
					positionParts.Add(Vector3.zero);
					rotationParts.Add(Quaternion.identity);
				}
				Drone drone = base.app.model.storage.factory.Instantiate(pd.rig, p_async);
				framesParts = new List<List<BlackboxFrame>>();
				SetDrone(drone, p_clone: false);
				string text3 = header.Get("physics-tune", "");
				if (!string.IsNullOrEmpty(text3))
				{
					drone.physics = DronePhysicsData.FromJson(text3);
				}
				string text4 = header.Get("fc-profile", "");
				if (!string.IsNullOrEmpty(text4))
				{
					drone.fcProfileData = Serialize.FromJson<FCProfileData>(text4);
				}
				collects = new List<CollectFrameEvent>();
				key = 32;
				if (!m_clip.tracks.ContainsKey(key))
				{
					return;
				}
				p_frames = m_clip.tracks[key];
				for (int num6 = 0; num6 < p_frames.Count; num6++)
				{
					BlackboxFrame blackboxFrame2 = p_frames[num6];
					ReplayEventType replayEventType = (ReplayEventType)Reflection<object>.Get<byte>(blackboxFrame2.data, 0);
					float x2 = Reflection<object>.Get<float>(blackboxFrame2.data, 1);
					float y2 = Reflection<object>.Get<float>(blackboxFrame2.data, 2);
					float z2 = Reflection<object>.Get<float>(blackboxFrame2.data, 3);
					Vector4 vector2 = new Vector4(x2, y2, z2, blackboxFrame2.time);
					switch (replayEventType)
					{
					case ReplayEventType.Collect:
					{
						CollectFrameEvent item2 = new CollectFrameEvent
						{
							index = Reflection<object>.Get<int>(blackboxFrame2.data, 4),
							time = blackboxFrame2.time,
							position = vector2
						};
						collects.Add(item2);
						break;
					}
					}
				}
			}
		}

		public ReplayFile clipV2
		{
			get
			{
				return m_clipV2;
			}
			set
			{
				Clear(p_drone: false);
				m_clipV2 = value;
				if (m_clipV2 == null)
				{
					return;
				}
				ReplayFile replayFile = m_clipV2;
				GamePlayerData pd = (player = new GamePlayerData());
				duration = replayFile.duration;
				ReplayHeader header = replayFile.header;
				if (header == null)
				{
					return;
				}
				string text = "profile-tournament-color";
				if (!header.data.ContainsKey(text))
				{
					Debug.LogWarning("ReplayClipPlayerModel> set-clip-v2 / Profile Color [" + text + "] not found!");
					text = "profile-color";
				}
				pd.rig = header.GetDroneRig();
				pd.rig.allowDynamicColor = true;
				raceTime = header.raceTime;
				type = header.gameTypeFlag;
				controller = header.controllerTypeFlag;
				tilt = header.cameraTilt;
				pd.order = header.order;
				pd.playerId = header.playerId;
				pd.platformId = header.platformId;
				pd.name = header.profileName;
				pd.upperName = pd.name.ToUpper();
				pd.color = ((!header.data.ContainsKey("profile-tournament-color")) ? header.profileColor : Colorf.ParseRGB(header.profileTournamentColorHex));
				pd.color2 = ((!header.data.ContainsKey("profile-tournament-color-2")) ? header.profileColor : header.profileTournamentColor2);
				pd.photo = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false, QualitySettings.activeColorSpace == ColorSpace.Linear);
				pd.photo.SetPixel(0, 0, Colorf.transparent);
				pd.photo.Apply();
				string p_id = (string.IsNullOrEmpty(pd.playerId) ? pd.platformId : pd.playerId);
				if (!base.app.online)
				{
					PlayerStateModel playerStateModel = base.app.model.storage.state.player;
					pd.photo = playerStateModel.profile.photo as Texture2D;
				}
				else
				{
					base.app.model.service.GetPlayerAvatar(p_id, delegate(Texture2D p_result)
					{
						if (!p_result)
						{
							return;
						}
						byte[] rawTextureData = p_result.GetRawTextureData();
						Debug.Log($"ReplayClipPlayerModel> GetPlayerAvatar / player-id[{pd.playerId}] platform-id[{pd.platformId}] raw data length [{rawTextureData.Length} bytes]");
						try
						{
							pd.photo.Resize(p_result.width, p_result.width, TextureFormat.RGBA32, hasMipMap: false);
							pd.photo.LoadRawTextureData(rawTextureData);
							pd.photo.Apply();
						}
						catch (Exception ex)
						{
							Debug.LogWarning("ReplayClipPlayerModel> GetPlayerAvatar / Error\n   " + ex.Message);
						}
					});
				}
				if (pd.rig == null)
				{
					return;
				}
				bool p_async = false;
				Drone drone = base.app.model.storage.factory.Instantiate(pd.rig, p_async);
				SetDrone(drone, p_clone: false);
				drone.physics = header.GetPhysicsTune();
				drone.fcProfileData = header.GetFCProfile();
				replayFile.Seek(0L);
				startPosition = replayFile.EvaluateVector3(ReplayChannelIds.DronePos, 0f);
				startRotation = replayFile.EvaluateQuaternion(ReplayChannelIds.DroneQuat, 0f);
				if (this.drone.body.frame.crash != null)
				{
					this.drone.body.frame.crash.Link();
					this.drone.body.frame.crash.SetFixData();
					List<DroneCrashNode> nodes = this.drone.body.frame.crash.nodes;
					positionParts = new List<Vector3>();
					rotationParts = new List<Quaternion>();
					for (int num = 0; num < nodes.Count + 1; num++)
					{
						positionParts.Add(Vector3.zero);
						rotationParts.Add(Quaternion.identity);
					}
					crashTime = 0f;
					ReplayEvent replayEvent = header.events.Find((ReplayEvent it) => it.typeFlag == ReplayEventType.Crash);
					if (replayEvent != null)
					{
						crashTime = replayEvent.time;
						replayFile.SetSimulatorCrashChannelsOffset(-replayEvent.sample);
					}
				}
				for (int num2 = 0; num2 < game.level.track.actions.Count; num2++)
				{
					MapAssetAction mapAssetAction = game.level.track.actions[num2];
					if ((bool)mapAssetAction)
					{
						switch (mapAssetAction.tag)
						{
						case GameFlag.ActionBreakGlass:
							mapAssetAction.gameObject.SetActive(value: false);
							break;
						case GameFlag.ActionNone:
							mapAssetAction.gameObject.SetActive(value: true);
							break;
						}
					}
				}
				actions = new List<ActionEventData>();
				for (int num3 = 0; num3 < header.events.Count; num3++)
				{
					ReplayEvent replayEvent2 = header.events[num3];
					if (replayEvent2.typeFlag == ReplayEventType.Action)
					{
						ActionEventData item = new ActionEventData
						{
							@event = replayEvent2.position
						};
						item.data = replayEvent2.data;
						item.@event.w = replayEvent2.time;
						actions.Add(item);
					}
				}
				for (int num4 = 0; num4 < actions.Count; num4++)
				{
					ActionEventData actionEventData = actions[num4];
					int actionIndex = actionEventData.actionIndex;
					MapAssetAction mapAssetAction2 = ((actionIndex < 0) ? null : game.level.track.actions[actionIndex]);
					if ((bool)mapAssetAction2)
					{
						mapAssetAction2.gameObject.SetActive(value: true);
						if (mapAssetAction2.tag == GameFlag.ActionBreakGlass)
						{
							mapAssetAction2.evaluateStartTime = actionEventData.@event.w;
						}
					}
				}
				collects = new List<CollectFrameEvent>();
				for (int num5 = 0; num5 < header.events.Count; num5++)
				{
					ReplayEvent replayEvent3 = header.events[num5];
					if (replayEvent3.typeFlag == ReplayEventType.Collect)
					{
						CollectFrameEvent item2 = new CollectFrameEvent
						{
							index = Reflection<object>.Get<int>(replayEvent3.data, 0),
							time = replayEvent3.time,
							position = replayEvent3.position
						};
						collects.Add(item2);
					}
				}
				for (int num6 = 0; num6 < collects.Count; num6++)
				{
					int index = collects[num6].index;
					if (index >= 0 && index < parent.trackCollectables.Count)
					{
						parent.trackCollectables[index].evaluateStartTime = collects[num6].time;
					}
				}
			}
		}

		public Vector2 leftInput => new Vector2(input.x, input.y);

		public Vector2 rightInput => new Vector2(input.z, input.w);

		public float elapsed
		{
			get
			{
				return m_elapsed;
			}
			set
			{
				if (Mathf.Abs(m_elapsed - value) > 0f)
				{
					Seek(value);
				}
			}
		}

		public float ratio
		{
			get
			{
				if (!(duration <= 0f))
				{
					return Mathf.Clamp01(elapsed / duration);
				}
				return 0f;
			}
			set
			{
				elapsed = duration * Mathf.Clamp01(value);
			}
		}

		public bool hasCrash => crashTime >= 0f;

		[ContextMenu("Save Replay CSV")]
		public void SaveCSV()
		{
			BlackboxData blackboxData = clip;
			if (blackboxData == null)
			{
				Debug.LogWarning("ReplayClipPlayerModel> Replay Clip is <null>");
				return;
			}
			string text = DRLPaths.Storage.replaysRoot + base.app.hash + ".csv";
			Debug.Log("ReplayClipPlayerModel> Replay Clip Saved at " + text);
			File.WriteAllText(text, blackboxData.ToCSV());
		}

		public int GetCollectCount(float p_time)
		{
			int num = 0;
			for (int i = 0; i < collects.Count && collects[i].time <= p_time; i++)
			{
				num++;
			}
			return num;
		}

		public int GetCollectCount()
		{
			return GetCollectCount(elapsed);
		}

		public void Clear(bool p_drone)
		{
			m_elapsed = 0f;
			duration = 0f;
			raceTime = 0f;
			speed = 1f;
			position = Vector3.zero;
			rotation = Quaternion.identity;
			velocity = Vector3.zero;
			input = Vector4.zero;
			pid = Vector3.zero;
			rpm = new float[0];
			reverse = false;
			playing = false;
			paused = false;
			m_clip = null;
			m_clipV2 = null;
			if (player != null && (bool)player.photo)
			{
				UnityEngine.Object.Destroy(player.photo);
				player.photo = null;
			}
			player = null;
			if (p_drone && (bool)drone)
			{
				drone.Destroy();
				drone = null;
			}
		}

		public bool IsPlaying()
		{
			if (!playing)
			{
				if (!parent)
				{
					return false;
				}
				return parent.playing;
			}
			return true;
		}

		public bool IsPaused()
		{
			if (!paused)
			{
				if (!parent)
				{
					return false;
				}
				return parent.paused;
			}
			return true;
		}

		public void Clear()
		{
			Clear(p_drone: true);
		}

		public void SetDrone(Drone p_drone, bool p_clone)
		{
			if ((bool)drone)
			{
				drone.Destroy();
			}
			if (!p_drone)
			{
				return;
			}
			drone = (p_clone ? UnityEngine.Object.Instantiate(p_drone) : p_drone);
			if (!drone)
			{
				return;
			}
			drone.name = p_drone.name;
			Action on_drone_ready = delegate
			{
				if ((bool)drone)
				{
					drone.fc.enabled = false;
					drone.rigidbody.enabled = false;
					drone.body.frame.camera.tilt = tilt;
					if (player != null)
					{
						drone.renderer.playerColor = player.color;
					}
					drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeAll;
					drone.rigidbody.rb.isKinematic = true;
					Transform rigColliderContainer = drone.body.GetRigColliderContainer();
					if ((bool)rigColliderContainer)
					{
						rigColliderContainer.gameObject.SetActive(value: false);
					}
					drone.renderer.SetTrailsDuration(0.1f);
					Seek(elapsed);
				}
			};
			if (drone.ready)
			{
				on_drone_ready();
			}
			else
			{
				UnityAction<DroneEvent> cb = null;
				cb = delegate(DroneEvent ev)
				{
					if (ev.type == DroneEventType.Ready)
					{
						on_drone_ready();
						drone.OnEvent.RemoveListener(cb);
					}
				};
				drone.OnEvent.AddListener(cb);
			}
			drone.transform.SetParent(base.transform, worldPositionStays: true);
		}

		public void SetDrone(Drone p_drone)
		{
			SetDrone(p_drone, p_clone: true);
		}

		public void Seek(float p_time, bool p_update_drone)
		{
			m_elapsed = Mathf.Clamp(p_time, 0f, duration);
			if (ReplayFile.EnableVersion2)
			{
				ReplayFile replayFile = clipV2;
				float p_ratio = replayFile.Seek(m_elapsed);
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
				if (partsBuffer == null)
				{
					partsBuffer = new float[7];
				}
				if (partsBuffer.Length < 7)
				{
					partsBuffer = new float[7];
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
				if (crashTime > 0f)
				{
					bool num = m_elapsed >= crashTime;
					bool flag = drone.body.frame.crash != null && drone.body.frame.crash.nodes != null;
					if (num && flag)
					{
						int num2 = Mathf.Min(drone.body.frame.crash.nodes.Count + 1, positionParts.Count);
						for (int i = 0; i < num2; i++)
						{
							positionParts[i] = replayFile.EvaluateVector3(ReplayChannelIds.DronePartPos, i, p_ratio);
							rotationParts[i] = replayFile.EvaluateQuaternion(ReplayChannelIds.DronePartQuat, i, p_ratio);
						}
					}
				}
			}
			else
			{
				if (clip == null)
				{
					return;
				}
				BlackboxData blackboxData = clip;
				float p_time2 = m_elapsed;
				byte key = 1;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					blackboxFrame.GetTransform(out position, out rotation);
				}
				key = 2;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					velocity = blackboxFrame.GetVector3();
				}
				key = 4;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					rpm = blackboxFrame.GetFloats();
				}
				key = 8;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					input = blackboxFrame.GetVector4();
				}
				key = 16;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					pid = blackboxFrame.GetVector3();
				}
				key = 64;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					blackboxFrame.GetPhysics(out dragFactors, out dragForce, out thrust, out torque);
				}
			}
			if ((bool)parent)
			{
				parent.EvaluateCollectables(p_time);
			}
			game.level.track.EvaluateActions(p_time);
			if (p_update_drone)
			{
				UpdateDrone();
			}
		}

		public void Seek(float p_time)
		{
			Seek(p_time, p_update_drone: true);
		}

		public void UpdateDrone()
		{
			Drone drone = this.drone;
			if (!drone || !drone.ready || !parent)
			{
				return;
			}
			bool num = type == GameFlag.Race || type == GameFlag.Campaign;
			Vector3 b = position;
			if (num)
			{
				Vector3 a = (usePodium ? podium : startPosition);
				Quaternion a2 = (usePodium ? podiumRotation : startRotation);
				if (new Vector4(a2.x, a2.y, a2.z, a2.w).magnitude < 0.01f)
				{
					a2 = rotation;
				}
				if (Mathf.Abs(a.y - startPosition.y) < 0.35f)
				{
					a.y = startPosition.y;
				}
				float f = ((podiumBlendDuration <= 0f) ? 1f : Mathf.Clamp01(elapsed / podiumBlendDuration));
				b = Vector3.Lerp(a, b, Mathf.Pow(f, 0.7f));
				rotation = Quaternion.Lerp(a2, rotation, Mathf.Pow(f, 0.7f));
			}
			bool flag = false;
			bool flag2 = false;
			if (crashTime > 0f)
			{
				flag = m_elapsed >= crashTime;
				flag2 = this.drone.body.frame.crash != null && this.drone.body.frame.crash.nodes != null;
			}
			if (!flag)
			{
				drone.position = b;
				drone.transform.rotation = rotation;
			}
			float num2 = speed * parent.speed;
			if (!IsPlaying())
			{
				num2 = 0f;
			}
			if (IsPaused())
			{
				num2 = 0f;
			}
			if (reverse)
			{
				num2 = 0f - num2;
			}
			if (parent.reverse)
			{
				num2 = 0f - num2;
			}
			float num3 = ((num2 < 0f) ? (-1f) : 1f);
			num2 = Mathf.Abs(num2);
			SetDroneMotorRpm(num2 * num3);
			if (flag && flag2)
			{
				DroneCrashBody crash = this.drone.body.frame.crash;
				List<DroneCrashNode> nodes = crash.nodes;
				int num4 = nodes.Count + 1;
				for (int i = 0; i < num4; i++)
				{
					if (i < positionParts.Count && i < rotationParts.Count)
					{
						DroneCrashNode droneCrashNode = ((i <= 0) ? null : nodes[i - 1]);
						Transform transform = ((i > 0) ? (droneCrashNode ? droneCrashNode.transform : null) : (crash ? crash.transform : null));
						if ((bool)transform)
						{
							transform.position = positionParts[i];
							transform.rotation = rotationParts[i];
						}
					}
				}
			}
			if (!flag && m_is_crash_active)
			{
				drone.FixSnap();
				drone.renderer.SetTrailsActive(p_flag: true);
			}
			if (flag && !m_is_crash_active)
			{
				drone.renderer.SetTrailsActive(p_flag: false);
			}
			m_is_crash_active = flag;
		}

		public void SetDroneMotorRpm(float p_speed)
		{
			Drone drone = this.drone;
			if ((bool)drone)
			{
				int num = Mathf.Min(drone.body.frame.escs.Count, rpm.Length);
				for (int i = 0; i < num; i++)
				{
					DroneESC droneESC = drone.body.frame.escs[i];
					float num2 = droneESC.motor.rpmMax * rpm[i];
					droneESC.motor.rpm = num2 * p_speed;
					droneESC.motor.rpmAudio = num2 * p_speed;
				}
			}
		}

		public void Step()
		{
			float num = Time.unscaledDeltaTime * speed * parent.speed;
			if (reverse)
			{
				num = 0f - num;
			}
			if ((bool)parent && parent.reverse)
			{
				num = 0f - num;
			}
			Seek(elapsed + num, p_update_drone: false);
			UpdateDrone();
		}
	}
}
