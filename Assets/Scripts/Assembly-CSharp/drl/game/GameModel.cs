using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameModel : Model<DRLApp>
	{
		public DroneSimulation simulation;

		public DroneCamera camera;

		public OrbitTransform orbit;

		[SerializeField]
		private List<GamePlayerData> m_players;

		[SerializeField]
		private List<GameTypeController> m_modes;

		private GamePlayerData m_player_data;

		private bool m_paused;

		public GameFlag type;

		public GameFlag mode;

		public FCMode fcMode;

		public bool allowCrash;

		public bool fromEditor;

		public bool tournament;

		public bool replayProcessActive;

		public LevelModel level => AssertFind<LevelModel>("level");

		public GameReplayModel replay => AssertFind<GameReplayModel>("replay");

		public List<GamePlayerData> players
		{
			get
			{
				return Reflection<object>.Assert(ref m_players);
			}
			set
			{
				m_players = value;
			}
		}

		public List<GameTypeController> modes
		{
			get
			{
				return Reflection<object>.Assert(ref m_modes);
			}
			set
			{
				m_modes = value;
			}
		}

		public bool hasDifferentPlayers
		{
			get
			{
				bool result = false;
				if (players.Count <= 1)
				{
					return result;
				}
				for (int i = 0; i < players.Count; i++)
				{
					for (int j = i + 1; j < players.Count; j++)
					{
						GamePlayerData gamePlayerData = players[i];
						GamePlayerData gamePlayerData2 = players[j];
						if (gamePlayerData.playerId != gamePlayerData2.playerId)
						{
							result = true;
						}
					}
				}
				return result;
			}
		}

		public int playerId
		{
			get
			{
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i] != null && players[i].type == GamePlayerType.Human)
					{
						return players[i].id;
					}
				}
				return -1;
			}
		}

		public Drone playerDrone
		{
			get
			{
				if (m_player_data != null && (bool)m_player_data.drone)
				{
					return m_player_data.drone;
				}
				return (m_player_data = GetPlayerDataById(playerId))?.drone;
			}
		}

		public string playerDroneHash
		{
			get
			{
				Drone drone = playerDrone;
				if (!drone)
				{
					return "";
				}
				string text = (drone.EstimateTopSpeed() * 3.6f).ToString("0");
				try
				{
					string text2 = drone.body.frame.escs[0].motor.spec.data.GetMaxThrust().ToString("0");
					string text3 = drone.physics.thrust.ToString("0");
					string text4 = (drone.rigidbody.rb.mass * 1000f).ToString("0");
					string text5 = ((drone.physics.CdMax < 0f) ? (drone.body.frame.cD.y * 100f) : (drone.physics.CdMax * 100f)).ToString("0");
					return string.Join(".", text, text2, text3, text4, text5);
				}
				catch
				{
					return text + ".FAULT";
				}
			}
		}

		public GamePlayerData playerData => GetPlayerDataById(playerId);

		public int racerCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i] != null && players[i].type != GamePlayerType.Data && players[i].type != GamePlayerType.Spectator)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int validCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i] != null && players[i].type != GamePlayerType.Data)
					{
						num++;
					}
				}
				return num;
			}
		}

		public bool paused
		{
			get
			{
				if (type == GameFlag.MapEditor)
				{
					return m_paused;
				}
				DroneSimulation droneSimulation = simulation;
				if (!droneSimulation)
				{
					return false;
				}
				if (droneSimulation.pause == DroneSimulationPauseMode.Unpause)
				{
					return false;
				}
				return true;
			}
			set
			{
				m_paused = value;
			}
		}

		public bool multiplayer => mode == GameFlag.NetworkMultiplayer;

		public GamePlayerData TryFetchSpectatorData()
		{
			int p_id = -1;
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i] != null && (players[i].type == GamePlayerType.Human || players[i].type == GamePlayerType.Spectator))
				{
					p_id = players[i].id;
					break;
				}
			}
			return GetPlayerDataById(p_id);
		}

		public void Set(DRLAppArguments p_args)
		{
			type = p_args.game.type;
			mode = p_args.game.mode;
			allowCrash = p_args.game.allowCrash;
			tournament = p_args.game.promo;
			fcMode = p_args.game.fcMode;
			fromEditor = p_args.game.editor;
			players = new List<GamePlayerData>(p_args.game.players);
		}

		public bool IsPlayer(Drone p_drone)
		{
			if ((bool)p_drone)
			{
				return p_drone == playerDrone;
			}
			return false;
		}

		public GamePlayerData GetPlayerData(Drone p_drone)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].drone == p_drone)
				{
					return players[i];
				}
			}
			return null;
		}

		public int GetPlayerCount(GamePlayerType p_type = GamePlayerType.None)
		{
			int num = 0;
			for (int i = 0; i < players.Count; i++)
			{
				num += ((p_type == GamePlayerType.None) ? 1 : ((p_type == players[i].type) ? 1 : 0));
			}
			return num;
		}

		public GamePlayerData GetPlayerDataById(int p_id)
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

		public GamePlayerData GetPlayerDataById(string p_platform_id)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].playerId == p_platform_id)
				{
					return players[i];
				}
			}
			return null;
		}

		public GamePlayerData GetPlayerDataByOrder(int p_index)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].order == p_index)
				{
					return players[i];
				}
			}
			return null;
		}

		public GamePlayerData GetRacerDataByOrder(int p_index)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].isRacer && players[i].order == p_index)
				{
					return players[i];
				}
			}
			return null;
		}

		public GamePlayerData GetPlayerData(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= players.Count)
			{
				return null;
			}
			return players[p_index];
		}

		public GamePlayerData GetPlayerDataByDrone(Drone p_drone)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].drone == p_drone)
				{
					return players[i];
				}
			}
			return null;
		}

		public List<BlackboxData> GetReplays()
		{
			List<BlackboxData> list = new List<BlackboxData>();
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].replay != null)
				{
					list.Add(players[i].replay);
				}
			}
			return list;
		}

		public List<ReplayFile> GetReplaysV2(bool p_excludeBots = false)
		{
			List<ReplayFile> list = new List<ReplayFile>();
			for (int i = 0; i < players.Count; i++)
			{
				if ((!p_excludeBots || players[i].type != GamePlayerType.Ghost) && players[i].replayV2 != null)
				{
					list.Add(players[i].replayV2);
				}
			}
			return list;
		}

		public bool HasAllReplays()
		{
			for (int i = 0; i < players.Count; i++)
			{
				bool flag = (ReplayFile.EnableVersion2 ? (players[i].replayV2 == null) : (players[i].replay == null));
				if (players[i].isRacer && flag)
				{
					return false;
				}
			}
			return true;
		}
	}
}
