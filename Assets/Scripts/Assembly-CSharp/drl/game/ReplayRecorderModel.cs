using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ReplayRecorderModel : Model<DRLApp>
	{
		public ReplayRecord recordV2;

		public BlackboxRecord record;

		public List<Drone> drones;

		public bool paused;

		public float duration = 300f;

		public int fps = 100;

		internal float fps_elapsed;

		public float elapsed;

		internal float spf
		{
			get
			{
				float num = fps;
				if (fps > 0)
				{
					return 1f / num;
				}
				return 99999f;
			}
		}

		public BlackboxData Add(Drone p_drone, bool p_is_player)
		{
			if (!p_drone)
			{
				return null;
			}
			if (drones.Contains(p_drone))
			{
				return null;
			}
			drones.Add(p_drone);
			byte b = 175;
			if (!p_is_player)
			{
				b &= 0xF7;
			}
			duration = 900f;
			BlackboxData blackboxData = record.Add(duration, fps, b);
			base.app.model?.service?.opponent?.TryAddLoadedReplay(record);
			SerializedData header = blackboxData.header;
			header.Set("drone-rig", (!p_drone.hasRig) ? "" : p_drone.rig.ToJson());
			header.Set("physics-tune", (!p_drone.hasPhysics) ? "" : p_drone.physics.ToJson());
			header.Set("fc-profile", (p_drone.fcProfileData == null) ? "" : p_drone.fcProfileData.ToJson());
			blackboxData.header = header;
			return blackboxData;
		}

		public BlackboxData Add(Drone p_drone)
		{
			return Add(p_drone, p_is_player: false);
		}

		public ReplayFile AddReplay(Drone p_drone, bool p_is_player)
		{
			if (!p_drone)
			{
				return null;
			}
			if (drones.Contains(p_drone))
			{
				return null;
			}
			drones.Add(p_drone);
			DroneBlackboxDataFlag droneBlackboxDataFlag = DroneBlackboxDataFlag.Basic;
			ReplayFile replayFile = new ReplayFile();
			string hash = base.app.hash;
			string text = UnityEngine.Random.Range(0, 9999).ToString("0000");
			replayFile.Initialize(DRLPaths.Storage.replaysRoot + hash + "-" + text + ".rpl2.bytes");
			if (p_drone.hasRig)
			{
				replayFile.header.SetDroneRig(p_drone.rig);
			}
			if (p_drone.hasPhysics)
			{
				replayFile.header.SetPhysicsTune(p_drone.physics);
			}
			if (p_drone.fcProfileData != null)
			{
				replayFile.header.SetFCProfile(p_drone.fcProfileData);
			}
			bool num = base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot;
			int p_crash_nodes = 0;
			if (num)
			{
				p_crash_nodes = p_drone.body.frame.crash.nodes.Count;
			}
			switch (droneBlackboxDataFlag)
			{
			case DroneBlackboxDataFlag.Basic:
				replayFile.AddSimulatorChannels(p_all: false, p_crash_nodes);
				break;
			case DroneBlackboxDataFlag.All:
				replayFile.AddSimulatorChannels(p_all: true, p_crash_nodes);
				break;
			}
			replayFile.header.Initialize(ReplayStream.GetReplayTempFilePath("", "header_"));
			recordV2.replays.Add(replayFile);
			return replayFile;
		}

		public ReplayFile AddReplay(Drone p_drone)
		{
			return AddReplay(p_drone, p_is_player: false);
		}

		public void Remove(Drone p_drone)
		{
			int dataIndex = GetDataIndex(p_drone);
			if (dataIndex >= 0)
			{
				record.RemoveAt(dataIndex);
				if (ReplayFile.EnableVersion2)
				{
					recordV2.replays.RemoveAt(dataIndex);
				}
				drones.Remove(p_drone);
			}
		}

		public void Replace(Drone p_old, Drone p_new)
		{
			int dataIndex = GetDataIndex(p_old);
			if (dataIndex >= 0)
			{
				drones[dataIndex] = p_new;
			}
		}

		public int GetDataIndex(Drone p_drone)
		{
			if (!p_drone)
			{
				return -1;
			}
			if (!drones.Contains(p_drone))
			{
				return -1;
			}
			return drones.IndexOf(p_drone);
		}

		public BlackboxData GetData(Drone p_drone)
		{
			if (record == null)
			{
				return null;
			}
			int dataIndex = GetDataIndex(p_drone);
			if (dataIndex < 0)
			{
				return null;
			}
			if (dataIndex >= record.clips.Count)
			{
				return null;
			}
			return record.clips[dataIndex];
		}

		public ReplayFile GetReplay(Drone p_drone)
		{
			int dataIndex = GetDataIndex(p_drone);
			if (dataIndex < 0)
			{
				return null;
			}
			return recordV2.replays[dataIndex];
		}

		public void SetData(Drone p_drone, BlackboxData p_data)
		{
			int dataIndex = GetDataIndex(p_drone);
			if (dataIndex >= 0)
			{
				record.Set(dataIndex, p_data);
			}
		}

		public void SetReplay(Drone p_drone, ReplayFile p_replay)
		{
			int dataIndex = GetDataIndex(p_drone);
			if (dataIndex >= 0)
			{
				recordV2.replays[dataIndex] = p_replay;
			}
		}

		public void Clear()
		{
			fps_elapsed = 0f;
			elapsed = 0f;
			paused = true;
			if (ReplayFile.EnableVersion2)
			{
				recordV2.Destroy();
			}
			else
			{
				record = new BlackboxRecord();
			}
			drones.Clear();
		}

		public void Stop()
		{
			fps_elapsed = 0f;
			elapsed = 0f;
			paused = true;
			if (ReplayFile.EnableVersion2)
			{
				recordV2.Seek(0L);
				return;
			}
			if (record == null)
			{
				record = new BlackboxRecord();
			}
			List<BlackboxData> clips = record.clips;
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].elapsed = 0f;
				clips[i].iterator = 0;
			}
		}

		public void Initialize()
		{
			Clear();
		}

		public void ToBytesAsync(BlackboxRecord p_record, Action<byte[]> p_callback, float p_delay = 0f)
		{
			Activity.Init();
			new Thread((ThreadStart)delegate
			{
				if (p_delay > 0f)
				{
					Thread.Sleep(Mathf.RoundToInt(p_delay * 1000f));
				}
				byte[] d = null;
				d = Serialize.ToBytes(p_record);
				Activity.RunOnce(delegate
				{
					DRLApp.LogMemStats($"ReplayRecorderModel> ToBytesAsync Complete - size[{d.Length / 1024}kb]", p_show_delta: true);
					if (p_callback != null)
					{
						p_callback(d);
					}
				});
			}).Start();
		}

		public void ToBytesAsync(ReplayFile p_replay, Action<byte[]> p_callback, float p_delay = 0f)
		{
			Activity.Init();
			byte[] d = null;
			ReplayFile rpl = p_replay;
			new Thread((ThreadStart)delegate
			{
				if (p_delay > 0f)
				{
					Thread.Sleep(Mathf.RoundToInt(p_delay * 1000f));
				}
				if (rpl == null)
				{
					Debug.LogWarning("ReplayRecorderModel> ToBytesAsync / Replay is <null>");
				}
				else
				{
					if (!rpl.valid)
					{
						Debug.LogWarning("ReplayRecorderModel> ToBytesAsync / Replay is invalid");
					}
					if (rpl.file == null)
					{
						Debug.Log("ReplayRecorderModel> ToBytesAsync / Replay is MemoryStream");
					}
					else
					{
						Debug.Log("ReplayRecorderModel> ToBytesAsync / Replay is FileStream\n" + rpl.file.Name);
					}
					if (rpl.valid)
					{
						if (rpl.file != null)
						{
							MemoryStream memoryStream = new MemoryStream();
							FileStream fileStream = new FileStream(rpl.file.Name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
							Debug.Log($"ReplayRecorderModel> ToBytesAsync / FileStream Found - [{fileStream.Length} bytes]");
							fileStream.CopyTo(memoryStream);
							memoryStream.Flush();
							memoryStream.Position = 0L;
							Debug.Log($"ReplayRecorderModel> ToBytesAsync / MemoryStream copy complete - [{memoryStream.Length} bytes]");
							d = memoryStream.ToArray();
							memoryStream.Close();
							fileStream.Close();
						}
						else
						{
							rpl.memory.Flush();
							rpl.memory.Position = 0L;
							d = rpl.memory.ToArray();
						}
					}
					Debug.Log($"ReplayRecorderModel> ToBytesAsync / ByteArray generated - [{((d != null) ? d.Length : 0)} bytes] valid[{d != null}]");
					if (d == null)
					{
						d = new byte[0];
					}
					Activity.RunOnce(delegate
					{
						DRLApp.LogMemStats($"ReplayRecorderModel> ToBytesAsync Complete - size[{d.Length / 1024}kb]", p_show_delta: true);
						if (p_callback != null)
						{
							p_callback(d);
						}
					});
				}
			}).Start();
		}

		public void ToBytesAsync(Action<byte[]> p_callback, float p_delay = 0f)
		{
			DRLApp.LogMemStats("ReplayRecorderModel> ToBytesAsync Start", p_show_delta: true);
			if (ReplayFile.EnableVersion2)
			{
				Drone drone = base.app.model.game.playerDrone;
				if (drone == null)
				{
					drone = base.app.model.game.TryFetchSpectatorData().drone;
				}
				ReplayFile replay = GetReplay(drone);
				ToBytesAsync(replay, p_callback, p_delay);
			}
			else
			{
				ToBytesAsync(record, p_callback, p_delay);
			}
		}

		public void FromBytesAsync(byte[] p_data, Action<BlackboxRecord> p_callback)
		{
			new Thread((ThreadStart)delegate
			{
				if (p_data == null)
				{
					Debug.LogWarning("ReplayREcordModel> Tried to parse null data!");
				}
				else
				{
					BlackboxRecord bbr = null;
					bbr = Serialize.FromBytes<BlackboxRecord>(p_data);
					Activity.RunOnce(delegate
					{
						if (p_callback != null)
						{
							p_callback(bbr);
						}
					});
				}
			}).Start();
		}

		public void FromBytesAsync(byte[] p_data, Action<ReplayFile> p_callback)
		{
			new Thread((ThreadStart)delegate
			{
				if (p_data == null)
				{
					Debug.LogWarning("ReplayREcordModel> Tried to parse null data!");
				}
				else
				{
					Stream streamPool = ReplayStream.GetStreamPool();
					streamPool.Write(p_data, 0, p_data.Length);
					if (streamPool is FileStream)
					{
						((FileStream)streamPool).Flush(flushToDisk: true);
					}
					else
					{
						streamPool.Flush();
					}
					streamPool.Position = 0L;
					ReplayFile rpl = new ReplayFile();
					rpl.Deserialize(streamPool);
					Activity.RunOnce(delegate
					{
						if (p_callback != null)
						{
							p_callback(rpl);
						}
					});
				}
			}).Start();
		}

		public void PushEvent(byte p_event, Drone p_drone, params object[] p_data)
		{
			if (record == null || paused)
			{
				return;
			}
			if (ReplayFile.EnableVersion2)
			{
				GetReplay(p_drone)?.PushEvent((ReplayEventType)p_event, elapsed, p_drone, p_data);
				return;
			}
			BlackboxData data = GetData(p_drone);
			if (data != null)
			{
				BlackboxFrame blackboxFrame = data.Push(32);
				Vector3 position = p_drone.position;
				object[] array = new object[(p_data.Length == 0) ? 4 : 5];
				array[0] = p_event;
				array[1] = position.x;
				array[2] = position.y;
				array[3] = position.z;
				if (p_data.Length != 0)
				{
					array[4] = p_data[0];
				}
				blackboxFrame.Set(blackboxFrame.time, blackboxFrame.type, array);
			}
		}

		public void UpdateDrones(float p_dt)
		{
			if (record == null || paused)
			{
				return;
			}
			elapsed += p_dt;
			fps_elapsed += p_dt;
			if (fps_elapsed >= spf)
			{
				fps_elapsed = 0f;
				if (ReplayFile.EnableVersion2)
				{
					int num = Mathf.Min(drones.Count, recordV2.replays.Count);
					for (int i = 0; i < num; i++)
					{
						ReplayFile replayFile = recordV2.replays[i];
						Drone drone = drones[i];
						if ((bool)drone)
						{
							replayFile.Write(elapsed, drone);
						}
					}
				}
				else
				{
					int num2 = Mathf.Min(drones.Count, record.clips.Count);
					for (int j = 0; j < num2; j++)
					{
						BlackboxData blackboxData = record.clips[j];
						Drone drone2 = drones[j];
						if (drone2 == null)
						{
							continue;
						}
						DroneBlackboxDataFlag droneBlackboxDataFlag = DroneBlackboxDataFlag.Transform;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag))
						{
							blackboxData.Push((byte)droneBlackboxDataFlag, drone2.position, drone2.transform.rotation);
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.Velocity;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag) && drone2.hasRigidbody)
						{
							blackboxData.Push((byte)droneBlackboxDataFlag, drone2.rigidbody.rb.velocity);
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.PIDControl;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag) && drone2.hasFc)
						{
							blackboxData.Push((byte)droneBlackboxDataFlag, drone2.fc);
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.RPM;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag) && drone2.hasBody && drone2.body.hasFrame)
						{
							blackboxData.Push((byte)droneBlackboxDataFlag, drone2.body.frame.GetRPMRatios());
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.Input;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag))
						{
							Vector4 d = new Vector4(RCI.GetRawAxis(RawAxis.LeftStickX), RCI.GetRawAxis(RawAxis.LeftStickY), RCI.GetRawAxis(RawAxis.RightStickX), RCI.GetRawAxis(RawAxis.RightStickY));
							blackboxData.Push((byte)droneBlackboxDataFlag, d);
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.Physics;
						if (blackboxData.IsAllowed(droneBlackboxDataFlag) && drone2.hasRigidbody)
						{
							blackboxData.Push((byte)droneBlackboxDataFlag, drone2.rigidbody.currentDragFactors, drone2.rigidbody.currentDragForce, drone2.rigidbody.currentThrust, drone2.rigidbody.currentTorque);
						}
						droneBlackboxDataFlag = DroneBlackboxDataFlag.TransformPart;
						if (!blackboxData.IsAllowed(droneBlackboxDataFlag))
						{
							continue;
						}
						CrashData crashData = drone2.crashData;
						if (crashData == null || !crashData.isBroken)
						{
							continue;
						}
						List<DroneCrashNode> nodes = drone2.body.frame.crash.nodes;
						for (int k = 0; k < nodes.Count; k++)
						{
							DroneCrashNode droneCrashNode = nodes[k];
							if (!(droneCrashNode == null))
							{
								Vector3 position = droneCrashNode.transform.position;
								Quaternion rotation = droneCrashNode.transform.rotation;
								blackboxData.Push((byte)droneBlackboxDataFlag, k, position, rotation);
							}
						}
					}
				}
			}
			record.Update(p_dt);
		}

		protected void OnDestroy()
		{
			if (ReplayFile.EnableVersion2)
			{
				try
				{
					recordV2.Destroy();
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
