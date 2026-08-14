using System.Collections.Generic;
using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class RaceModel : Model<DRLApp>
	{
		public TextAsset defaultRig;

		private DroneRigData m_rig;

		public RaceStatusType status;

		private ulong m2390482349a;

		private ulong m2390482349b;

		private ulong m2390482349c;

		public float timeStart;

		public int racersCount;

		public int ghostsCount;

		public int crashes;

		public float topSpeed;

		public bool countActive;

		public bool raceActive;

		public bool raceComplete;

		public bool stopTimeOnPause = true;

		public List<ColliderEventComponent> gates;

		public List<float> gateTimes = new List<float>();

		public string raceId;

		private RaceController m_controller;

		public List<GamePlayerData> Rankings = new List<GamePlayerData>();

		private Dictionary<Drone, int> m_progress;

		private bool m_standings_dirty;

		private float m_standings_refresh_time;

		public int currentLap;

		public float fastestLapTime;

		public float slowestLapTime;

		public float currentLapTime;

		public List<float> lapTimes;

		public int fastestLapIndex;

		public int slowestLapIndex;

		public int resets;

		public Drone playerDrone;

		public float distanceTraveled;

		private Vector3 lastPosition;

		private Vector3 currentPosition;

		public float timeInFirstPlace;

		public float playerPercentile;

		private static List<RaceStatusType> m_race_status_priority = new List<RaceStatusType>
		{
			RaceStatusType.Success,
			RaceStatusType.Running,
			RaceStatusType.Timeout,
			RaceStatusType.Forfeit,
			RaceStatusType.Crash,
			RaceStatusType.Quit,
			RaceStatusType.None
		};

		private float m_autoRefreshStandingsTimer = 0.75f;

		private float m_autoRefreshStandingsPeriod = 0.75f;

		private int m_debugCounter;

		public DroneRigData rig
		{
			get
			{
				if (m_rig != null)
				{
					return m_rig;
				}
				if (!defaultRig)
				{
					return m_rig;
				}
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(defaultRig.bytes);
				return m_rig = droneRigData;
			}
		}

		public float time
		{
			get
			{
				return SerializedData.FloatDecode(m2390482349a, 0f, 18000f, 8);
			}
			set
			{
				m2390482349a = SerializedData.FloatEncode(value, 0f, 18000f, 8, 10, 250);
			}
		}

		public float timeCheck
		{
			get
			{
				return SerializedData.FloatDecode(m2390482349b, 0f, 18000f, 8);
			}
			set
			{
				m2390482349b = SerializedData.FloatEncode(value, 0f, 18000f, 8, 10, 250);
			}
		}

		public float timeDoubleCheck
		{
			get
			{
				return SerializedData.FloatDecode(m2390482349c, 0f, 18000f, 8);
			}
			set
			{
				m2390482349c = SerializedData.FloatEncode(value, 0f, 18000f, 8, 10, 250);
			}
		}

		public RaceController controller
		{
			get
			{
				if (!m_controller)
				{
					return m_controller = GetComponent<RaceController>();
				}
				return m_controller;
			}
		}

		public Dictionary<Drone, int> progress => Reflection<object>.Assert(ref m_progress);

		public int GetRacerRankingCount()
		{
			if (Rankings == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < Rankings.Count; i++)
			{
				GamePlayerData gamePlayerData = Rankings[i];
				if (gamePlayerData != null && gamePlayerData.isRacer)
				{
					num++;
				}
			}
			return num;
		}

		public void ClearData()
		{
			progress.Clear();
			Rankings.Clear();
			time = 0f;
			timeCheck = 0f;
			timeStart = Time.time;
			crashes = 0;
			topSpeed = 0f;
			ResetRaceAnalytics();
		}

		public GamePlayerData GetPlayerRankings()
		{
			if (Rankings == null || Rankings.Count == 0)
			{
				return null;
			}
			string playerId = base.app.model.storage.state.player.profile.playerId;
			for (int i = 0; i < Rankings.Count; i++)
			{
				if (Rankings[i].playerId == playerId)
				{
					return Rankings[i];
				}
			}
			return null;
		}

		public void ResetRaceAnalytics()
		{
			fastestLapTime = float.MaxValue;
			slowestLapTime = float.MinValue;
			currentLap = 0;
			lapTimes.Clear();
			resets = 0;
			distanceTraveled = 0f;
			if ((bool)playerDrone)
			{
				lastPosition = playerDrone.transform.position;
			}
			timeInFirstPlace = 0f;
			currentLapTime = 0f;
		}

		public void AddDrone(Drone p_drone)
		{
			progress[p_drone] = 0;
			GamePlayerData playerData = base.app.model.game.GetPlayerData(p_drone);
			if (playerData != null)
			{
				playerData.raceStatus = RaceStatusType.Running;
				Rankings.Add(playerData);
			}
		}

		public void AddDrones(DroneSimulation p_simulation)
		{
			if ((bool)p_simulation)
			{
				for (int i = 0; i < p_simulation.drones.list.Count; i++)
				{
					Drone p_drone = p_simulation.drones.list[i];
					AddDrone(p_drone);
				}
			}
		}

		public int GetProgress(Drone p_drone)
		{
			DroneSimulation simulation = controller.game.model.simulation;
			if (!simulation)
			{
				return 0;
			}
			DroneGhostTransmitter byDrone = simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_drone);
			if (byDrone != null)
			{
				return byDrone.gateIndex;
			}
			if (!progress.ContainsKey(p_drone))
			{
				return 0;
			}
			return progress[p_drone];
		}

		private int SortByPathProgress(Drone p_d1, Drone p_d2, string username1, string username2)
		{
			int p_gate_progress = GetProgress(p_d1);
			int p_gate_progress2 = GetProgress(p_d2);
			float progressByDistance = GetProgressByDistance(p_d1, p_gate_progress);
			float progressByDistance2 = GetProgressByDistance(p_d2, p_gate_progress2);
			if (Mathf.Abs(progressByDistance - progressByDistance2) < 0.05f)
			{
				int pathProgress = GetPathProgress(p_d1, p_gate_progress);
				int pathProgress2 = GetPathProgress(p_d2, p_gate_progress2);
				return -1 * pathProgress.CompareTo(pathProgress2);
			}
			return progressByDistance.CompareTo(progressByDistance2);
		}

		private int GetPathProgress(Drone p_drone, int p_gate_progress)
		{
			if (base.app.model.game == null || base.app.model.game.level == null)
			{
				return 0;
			}
			TrackModel track = base.app.model.game.level.track;
			if (track == null)
			{
				return 0;
			}
			int count = track.gates.Count;
			if (Mathf.Clamp(p_gate_progress, 0, count - 1) >= count - 1)
			{
				return 0;
			}
			DroneSimulation simulation = controller.game.model.simulation;
			if (!simulation)
			{
				return 0;
			}
			SplineTracerComponent pathTrace = base.app.model.game.level.track.pathTrace;
			DroneNetworkTransmitter byDrone = simulation.transmitters.GetByDrone<DroneNetworkTransmitter>(p_drone);
			Vector3 p_point = ((byDrone == null) ? p_drone.position : byDrone.networkRacer.networkPosition);
			int result = p_gate_progress;
			if (!pathTrace)
			{
				return result;
			}
			SplineTracerSectionComponent sectionClamped = pathTrace.GetSectionClamped(p_gate_progress);
			if (!sectionClamped)
			{
				return result;
			}
			int closestSampleIndex = sectionClamped.GetClosestSampleIndex(p_point);
			if (closestSampleIndex >= 0)
			{
				result = closestSampleIndex;
			}
			return result;
		}

		private Collider GetGate(int p_id)
		{
			if (base.app.controller.game.model.level.track == null)
			{
				return null;
			}
			List<Collider> list = base.app.controller.game.model.level.track.gates;
			if (p_id < 0 || p_id >= list.Count)
			{
				return null;
			}
			return list[p_id];
		}

		private float GetProgressByDistance(Drone p_drone, int p_gate_progress)
		{
			int num = p_gate_progress;
			num = ((num > 0) ? num : 0);
			Collider gate = GetGate(num);
			if (gate == null)
			{
				return -1f;
			}
			DroneSimulation simulation = controller.game.model.simulation;
			if (!simulation)
			{
				return 0f;
			}
			DroneNetworkTransmitter byDrone = simulation.transmitters.GetByDrone<DroneNetworkTransmitter>(p_drone);
			return Vector3.Distance((byDrone == null) ? p_drone.position : byDrone.networkRacer.networkPosition, gate.transform.position);
		}

		public void SetProgress(Drone p_drone, int p_gate_id)
		{
			progress[p_drone] = p_gate_id;
		}

		public int SkipGates(Drone p_drone)
		{
			int num = gates.Count - 1;
			SetProgress(p_drone, num);
			return num;
		}

		public bool IncrementProgress(Drone p_drone, ColliderEventComponent p_gate)
		{
			int p_gate_index = gates.IndexOf(p_gate);
			return IncrementProgress(p_drone, p_gate_index);
		}

		public bool IncrementProgress(Drone p_drone, int p_gate_index)
		{
			if (GetProgress(p_drone) == p_gate_index)
			{
				progress[p_drone] = p_gate_index + 1;
				RefreshStandings();
				return true;
			}
			return false;
		}

		public virtual bool IsComplete(Drone p_drone)
		{
			return GetProgress(p_drone) >= gates.Count;
		}

		public virtual bool IsComplete()
		{
			bool result = true;
			for (int i = 0; i < Rankings.Count; i++)
			{
				if (Rankings[i].raceStatus == RaceStatusType.Running)
				{
					result = false;
				}
			}
			return result;
		}

		public void RefreshStandings()
		{
			m_standings_dirty = true;
		}

		protected void ApplyRefreshStandings()
		{
			if (Rankings.Count == 1)
			{
				UpdateRaceStatus(Rankings[0]);
				UpdateRaceTime(Rankings[0]);
			}
			else
			{
				Rankings.Sort(SortStanding);
			}
			GamePlayerData playerRankings = GetPlayerRankings();
			float p_delay = ((playerRankings == null || playerRankings.raceStatus != RaceStatusType.Running) ? 1f : (1f / 30f));
			Notify(p_delay, "game.standings@update", Rankings);
		}

		public void ForceSortRankings()
		{
			Rankings.Sort(SortStanding);
		}

		public void SortRankingsByOrder()
		{
			if (base.validContext && Rankings.Count != 0)
			{
				Rankings.Sort((GamePlayerData x, GamePlayerData y) => x.order.CompareTo(y.order));
			}
		}

		public int GetPlayerPosition(GamePlayerData p_data)
		{
			if (p_data == null)
			{
				return -1;
			}
			return GetPlayerPosition(p_data.playerId);
		}

		public int GetPlayerPosition(string p_id, bool p_ignore_ghosts = false)
		{
			int num = 0;
			for (int i = 0; i < Rankings.Count; i++)
			{
				GamePlayerData gamePlayerData = Rankings[i];
				if (!p_ignore_ghosts || gamePlayerData.type != GamePlayerType.Ghost)
				{
					if (gamePlayerData.playerId == p_id)
					{
						return num;
					}
					num++;
				}
			}
			return -1;
		}

		public float GetRaceTime(Drone p_drone)
		{
			if (p_drone == null)
			{
				return 300f;
			}
			GamePlayerData playerData = base.app.model.game.GetPlayerData(p_drone);
			if (playerData == null)
			{
				return 300f;
			}
			return UpdateRaceTime(playerData);
		}

		public float GetFirstNonGhostTime()
		{
			float result = 0f;
			foreach (GamePlayerData ranking in Rankings)
			{
				if (ranking.type == GamePlayerType.Human)
				{
					result = ranking.raceTime;
					break;
				}
			}
			return result;
		}

		public virtual float GetDeltaTime()
		{
			return 0f;
		}

		public virtual float GetGlobalTime()
		{
			return Time.time;
		}

		public float UpdateRaceTime(GamePlayerData p_data)
		{
			if (p_data == null)
			{
				Debug.LogWarning("RaceModel> GetRaceTime - Invalid Player Data!");
				return 300f;
			}
			switch (p_data.type)
			{
			case GamePlayerType.Human:
				return p_data.raceTime = time;
			case GamePlayerType.Ghost:
			{
				DroneGhostTransmitter byDrone = base.app.model.game.simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_data.drone);
				if (byDrone == null)
				{
					return 300f;
				}
				p_data.raceTime = byDrone.raceTime;
				return p_data.raceTime;
			}
			case GamePlayerType.Network:
			{
				NetworkActor player = base.app.model.network.GetPlayer(p_data.id);
				NetworkRoom room = base.app.model.network.room;
				if (room == null)
				{
					return 300f;
				}
				if (player == null)
				{
					return 300f;
				}
				float result = 300f;
				switch (player.RaceState)
				{
				case NetworkActor.RacerState.Running:
					result = room.ElapsedTime;
					break;
				case NetworkActor.RacerState.Timeout:
					result = player.RaceTime;
					break;
				case NetworkActor.RacerState.Complete:
					result = player.RaceTime;
					break;
				case NetworkActor.RacerState.Crash:
					result = player.RaceTime;
					break;
				}
				p_data.raceTime = player.RaceTime;
				return result;
			}
			case GamePlayerType.Data:
				return p_data.raceTime;
			default:
				return 300f;
			}
		}

		protected RaceStatusType UpdateRaceStatus(GamePlayerData p_data)
		{
			if (p_data == null)
			{
				Debug.LogWarning("RaceModel> GetRaceTime - Invalid Player Data!");
				return RaceStatusType.None;
			}
			switch (p_data.type)
			{
			case GamePlayerType.Human:
				return p_data.raceStatus = status;
			case GamePlayerType.Ghost:
			{
				if (p_data.raceStatus == RaceStatusType.Success)
				{
					return RaceStatusType.Success;
				}
				bool flag = false;
				DroneGhostTransmitter byDrone = base.app.model.game.simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_data.drone);
				if ((bool)byDrone && byDrone.elapsed + 0.5f >= byDrone.raceTime)
				{
					flag = true;
				}
				return p_data.raceStatus = (flag ? RaceStatusType.Success : RaceStatusType.Running);
			}
			case GamePlayerType.Network:
			{
				if (p_data.raceStatus == RaceStatusType.Success)
				{
					return RaceStatusType.Success;
				}
				NetworkActor player = base.app.model.network.GetPlayer(p_data.id);
				if (player == null)
				{
					return p_data.raceStatus = RaceStatusType.Quit;
				}
				RaceStatusType raceStatus = RaceStatusType.None;
				switch (player.RaceState)
				{
				case NetworkActor.RacerState.Running:
					raceStatus = RaceStatusType.Running;
					break;
				case NetworkActor.RacerState.Crash:
					raceStatus = RaceStatusType.Crash;
					break;
				case NetworkActor.RacerState.Timeout:
					raceStatus = RaceStatusType.Timeout;
					break;
				case NetworkActor.RacerState.Forfeit:
					raceStatus = RaceStatusType.Forfeit;
					break;
				case NetworkActor.RacerState.Complete:
					raceStatus = RaceStatusType.Success;
					break;
				}
				return p_data.raceStatus = raceStatus;
			}
			case GamePlayerType.Spectator:
				return p_data.raceStatus = RaceStatusType.None;
			case GamePlayerType.Data:
				if (p_data.raceStatus == RaceStatusType.Success)
				{
					return RaceStatusType.Success;
				}
				return p_data.raceStatus = RaceStatusType.Quit;
			default:
				Debug.LogWarning("RaceModel> GetRaceStatus - Tried to fetch time from invalid player type - name[" + p_data.name + "]");
				return RaceStatusType.None;
			}
		}

		protected int SortStanding(GamePlayerData a, GamePlayerData b)
		{
			int num = Rankings.IndexOf(a);
			int num2 = Rankings.IndexOf(b);
			int num3 = ((num >= num2) ? 1 : (-1));
			_ = base.app.model.storage.state.player.profile.isDeveloper;
			if (a == null || b == null)
			{
				if (a != null)
				{
					if (b != null)
					{
						return num3;
					}
					return -1;
				}
				return 1;
			}
			RaceStatusType raceStatusType = UpdateRaceStatus(a);
			RaceStatusType raceStatusType2 = UpdateRaceStatus(b);
			float num4 = UpdateRaceTime(a);
			if (num4 < 0f)
			{
				num4 = 300f;
			}
			float num5 = UpdateRaceTime(b);
			if (num5 < 0f)
			{
				num5 = 300f;
			}
			if (!raceActive)
			{
				if (num4 > 0f && raceStatusType == RaceStatusType.None)
				{
					raceStatusType = RaceStatusType.Success;
				}
				if (num5 > 0f && raceStatusType2 == RaceStatusType.None)
				{
					raceStatusType2 = RaceStatusType.Success;
				}
			}
			int num6 = GetProgress(a.drone);
			int value = GetProgress(b.drone);
			int num7 = ((num4 < num5) ? (-1) : ((num4 > num5) ? 1 : num3));
			int num8 = -1 * num6.CompareTo(value);
			int num9 = m_race_status_priority.IndexOf(raceStatusType);
			int num10 = m_race_status_priority.IndexOf(raceStatusType2);
			if (num9 != num10)
			{
				if (num9 >= num10)
				{
					return 1;
				}
				return -1;
			}
			int num11 = num3;
			switch (raceStatusType)
			{
			case RaceStatusType.Success:
				num11 = num7;
				break;
			case RaceStatusType.Running:
				num11 = num8;
				if (num11 == 0 && !raceComplete)
				{
					num11 = SortByPathProgress(a.drone, b.drone, a.name, b.name);
				}
				break;
			case RaceStatusType.Timeout:
				num11 = num8;
				break;
			case RaceStatusType.Forfeit:
				num11 = num8;
				break;
			case RaceStatusType.Crash:
				num11 = num8;
				break;
			case RaceStatusType.Quit:
				num11 = num3;
				break;
			}
			return num11;
		}

		protected void Update()
		{
			m_standings_refresh_time -= Time.unscaledDeltaTime;
			if (m_standings_refresh_time <= 0f && m_standings_dirty)
			{
				ApplyRefreshStandings();
				m_standings_dirty = false;
				m_standings_refresh_time = 0.1f;
			}
			if (raceComplete)
			{
				return;
			}
			m_autoRefreshStandingsTimer -= Time.unscaledDeltaTime;
			if (m_autoRefreshStandingsTimer <= 0f)
			{
				m_autoRefreshStandingsTimer = m_autoRefreshStandingsPeriod;
				ApplyRefreshStandings();
			}
			if ((bool)playerDrone && raceActive)
			{
				float num = Vector3.Distance(lastPosition, playerDrone.transform.position);
				if (!(num < 0.015f))
				{
					distanceTraveled += num;
					lastPosition = playerDrone.transform.position;
				}
			}
		}

		public void UpdateLapTimes(int p_newLap)
		{
			if (p_newLap > currentLap)
			{
				CalculateLapTimes(currentLap);
				currentLapTime = 0f;
				currentLap = p_newLap;
			}
		}

		public void CalculateLapTimes(int p_index)
		{
			if (p_index < 0)
			{
				Debug.LogWarning("RaceController> Calculate Laptimes / Negative Index");
				return;
			}
			if (p_index >= lapTimes.Count)
			{
				Debug.LogWarning($"RaceController> Calculate Laptimes / Array out of Bounds Index - {p_index}/{lapTimes.Count}");
				return;
			}
			lapTimes[p_index] = currentLapTime;
			Debug.Log($"RaceController>CalculateLapTimes: <color=green>SAVED LAP TIME: {currentLapTime}</color>");
			if (currentLapTime > slowestLapTime)
			{
				slowestLapTime = currentLapTime;
				slowestLapIndex = p_index;
				Debug.Log($"RaceController>CalculateLapTimes: <color=red>SAVED SLOWEST LAP TIME: {slowestLapTime}</color>");
			}
			if (currentLapTime < fastestLapTime)
			{
				fastestLapTime = currentLapTime;
				fastestLapIndex = p_index;
				Debug.Log($"RaceController>CalculateLapTimes: <color=green>SAVED FASTEST LAP TIME: {fastestLapTime}</color>");
			}
		}
	}
}
