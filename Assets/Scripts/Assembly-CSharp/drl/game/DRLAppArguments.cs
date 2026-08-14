using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class DRLAppArguments : AssetLibrary
	{
		[Serializable]
		public class Game
		{
			public DRLService backend;

			public GameFlag type;

			public GameFlag mode;

			public FCMode fcMode;

			public DRLMap map;

			public DRLMapTrack track;

			public DRLMission mission;

			public DRLCampaign campaign;

			public DRLQuest quest;

			public string podium;

			public bool allowCrash;

			public bool promo;

			public DRLTournamentLegacyData tournamentLegacy;

			public DRLTournamentData tournamentData;

			public DRLTournamentMatchData tournamentMatchData;

			private bool m_tournamentPromo;

			[SerializeField]
			private List<GamePlayerData> m_players;

			public BlackboxRecord replay;

			public ReplayFile replayV2;

			public bool garage;

			public bool editor;

			public bool tryouts;

			public OpponentModeType opponentType;

			public bool isFromBrackets;

			public bool isCustomMap
			{
				get
				{
					if (!map)
					{
						return false;
					}
					return map.data != null;
				}
			}

			public string mapGUID
			{
				get
				{
					if (!isCustomMap)
					{
						if (!map)
						{
							return "";
						}
						return map.guid;
					}
					return map.data.guid;
				}
			}

			public string trackGUID
			{
				get
				{
					if (!isCustomMap)
					{
						return track.guid;
					}
					return "";
				}
			}

			public bool isTournamentActive => tournamentData != null;

			public bool isTournamentMatchActive => tournamentMatchData != null;

			public string tournamentLegacyGUID
			{
				get
				{
					if (tournamentLegacy != null)
					{
						return tournamentLegacy.guid;
					}
					return "";
				}
			}

			public bool tournamentPromo
			{
				get
				{
					if (!tournamentLegacyGUID.ToLower().Contains("st18"))
					{
						return m_tournamentPromo;
					}
					return true;
				}
				set
				{
					m_tournamentPromo = value;
				}
			}

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

			public void Clear()
			{
				if ((bool)map)
				{
					map.data = null;
				}
				map = null;
				track = null;
				mission = null;
				quest = null;
				campaign = null;
				fcMode = FCMode.Pro;
				allowCrash = false;
				promo = false;
				if (m_players != null)
				{
					for (int i = 0; i < m_players.Count; i++)
					{
						if (m_players[i].replayV2 != null)
						{
							m_players[i].replayV2.Destroy();
						}
					}
				}
				m_players = new List<GamePlayerData>();
				replay = null;
				replayV2 = null;
				podium = "PD-a6d";
				garage = false;
				editor = false;
				opponentType = OpponentModeType.Off;
				tournamentData = null;
				tournamentMatchData = null;
			}

			public GamePlayerData AddPlayer(GamePlayerData p_data)
			{
				if (p_data == null)
				{
					Debug.LogWarning("DRLAppArguments> Tried to add 'null' player!");
					return null;
				}
				p_data.order = players.Count;
				if (p_data.id < 0)
				{
					p_data.id = p_data.order;
				}
				if (p_data.type == GamePlayerType.Ghost)
				{
					p_data.id += 100;
				}
				players.Add(p_data);
				return p_data;
			}

			public GamePlayerData AddPlayer(GamePlayerType p_type, string p_player_id, string p_platform_id, string p_name, Color[] p_colors, Texture2D p_photo)
			{
				GamePlayerData gamePlayerData = new GamePlayerData();
				gamePlayerData.SetPlayer(p_type, p_player_id, p_platform_id, p_name, p_colors);
				return AddPlayer(gamePlayerData);
			}

			public GamePlayerData AddPlayer(GamePlayerType p_type, string p_player_id, string p_platform_id, string p_name, Color[] p_colors, string p_photo)
			{
				GamePlayerData gamePlayerData = new GamePlayerData();
				gamePlayerData.SetPlayer(p_type, p_player_id, p_platform_id, p_name, p_colors);
				if (p_name != "NPC")
				{
					gamePlayerData.RefreshPlayerPhoto();
				}
				return AddPlayer(gamePlayerData);
			}

			public GamePlayerData AddGhostPlayer(BlackboxData p_data)
			{
				if (p_data == null)
				{
					return null;
				}
				SerializedData header = p_data.header;
				string p_platform_id = header.Get(DRLService.PlatformIdKey, "");
				string p_player_id = header.Get("player-id", "");
				string p_name = header.Get("profile-name", "");
				string p_v = header.Get("profile-color", "");
				string p_v2 = header.Get("profile-secondary-color", "");
				if (isTournamentActive)
				{
					p_v = header.Get<string>("profile-tournament-color");
					p_v2 = header.Get<string>("profile-tournament-color-2");
				}
				Color color = Colorf.ParseRGB(p_v);
				Color color2 = Colorf.ParseRGB(p_v2);
				Color[] p_colors = new Color[2] { color, color2 };
				string podiumId = header.Get("podium-id", "PD-a6d");
				GamePlayerData gamePlayerData = AddPlayer(GamePlayerType.Ghost, p_player_id, p_platform_id, p_name, p_colors, "");
				gamePlayerData.podiumId = podiumId;
				gamePlayerData.replay = p_data;
				return gamePlayerData;
			}

			public void RemoveGhostPlayers()
			{
				RemovePlayersByType(GamePlayerType.Ghost);
			}

			public GamePlayerData AddGhostPlayer(ReplayFile p_replay)
			{
				if (p_replay == null)
				{
					return null;
				}
				ReplayHeader header = p_replay.header;
				string platformId = header.platformId;
				string playerId = header.playerId;
				string profileName = header.profileName;
				Color color = header.profileColor;
				Color color2 = header.profileColor;
				if (isTournamentActive)
				{
					color = Colorf.ParseRGB(header.profileTournamentColorHex);
					color2 = Colorf.ParseRGB(header.profileTournamentColor2Hex);
				}
				Color[] p_colors = new Color[2] { color, color2 };
				string podiumGUID = header.podiumGUID;
				GamePlayerData gamePlayerData = AddPlayer(GamePlayerType.Ghost, playerId, platformId, profileName, p_colors, "");
				gamePlayerData.podiumId = podiumGUID;
				gamePlayerData.replayV2 = p_replay;
				return gamePlayerData;
			}

			public void AddGhostPlayer(BlackboxRecord p_data)
			{
				if (p_data != null)
				{
					List<BlackboxData> clips = p_data.clips;
					_ = new Color[6]
					{
						DRLColor.profileColors[1],
						DRLColor.profileColors[9],
						DRLColor.profileColors[5],
						DRLColor.profileColors[3],
						DRLColor.profileColors[6],
						DRLColor.profileColors[11]
					};
					for (int i = 0; i < clips.Count; i++)
					{
						BlackboxData p_data2 = clips[i];
						AddGhostPlayer(p_data2);
					}
				}
			}

			public void AddGhostPlayer(ReplayRecord p_replays)
			{
				if (p_replays != null)
				{
					List<ReplayFile> replays = p_replays.replays;
					_ = new Color[6]
					{
						DRLColor.profileColors[1],
						DRLColor.profileColors[9],
						DRLColor.profileColors[5],
						DRLColor.profileColors[3],
						DRLColor.profileColors[6],
						DRLColor.profileColors[11]
					};
					for (int i = 0; i < replays.Count; i++)
					{
						ReplayFile p_replay = replays[i];
						AddGhostPlayer(p_replay);
					}
				}
			}

			public GamePlayerData GetPlayerById(string p_id)
			{
				return players.Find((GamePlayerData p_it) => p_it.playerId == p_id);
			}

			public void RemovePlayersByType(GamePlayerType p_type)
			{
				players.RemoveAll((GamePlayerData it) => it.type == p_type);
			}

			public void AddReplay(BlackboxRecord p_data)
			{
				if (p_data == null)
				{
					Debug.LogWarning("DRLAppArguments> AddReplay - Replay data is null!");
					return;
				}
				BlackboxRecord blackboxRecord = (replay = p_data);
				int count = blackboxRecord.clips.Count;
				Debug.Log("DRLAppArguments> AddReplay - clips [" + count + "]");
				for (int i = 0; i < count; i++)
				{
					GamePlayerData gamePlayerData = AddGhostPlayer(blackboxRecord.clips[i]);
					Debug.Log("DRLAppArguments> AddReplay / Adding name[" + gamePlayerData.name + "]");
				}
			}

			public void AddReplay(ReplayFile p_data)
			{
				if (p_data == null)
				{
					Debug.LogWarning("DRLAppArguments> AddReplay - Replay data is null!");
					return;
				}
				GamePlayerData gamePlayerData = AddGhostPlayer(replayV2 = p_data);
				Debug.Log("DRLAppArguments> AddReplay / Adding name[" + gamePlayerData.name + "]");
			}

			public void SetPlayerType(GamePlayerType p_type)
			{
				for (int i = 0; i < players.Count; i++)
				{
					players[i].type = p_type;
				}
			}
		}

		[Serializable]
		public class Leaderboards
		{
			public DRLMap map;

			public DRLMapTrack track;

			public MapData customMap;

			public bool isCustomMap;

			public DRLQuest quest;

			public int controllerTypeIndex;

			public int platformIndex;

			public int questIndex;

			public DRLMission mission;

			public int missionIndex;

			public int physicsIndex = 1;

			public int sizeIndex;

			public DRLCampaign campaign;

			public int campaignIndex;

			public GameFlag gameType;

			public int racePage;

			public int campaignRaceModePage;

			public bool isCampaignRaceMode;

			public DRLLeaderboardData campaignSelectd;

			public bool valid
			{
				get
				{
					if ((!isCustomMap && (bool)map && (bool)track) || (isCustomMap && customMap != null))
					{
						return true;
					}
					if ((bool)campaign)
					{
						return true;
					}
					if ((bool)mission && (bool)quest)
					{
						return true;
					}
					return false;
				}
			}
		}

		[Serializable]
		public class Tournament
		{
			private DRLTournamentData m_data;

			public string guid;

			public string id;

			public int skill;

			public DRLTournamentData data
			{
				get
				{
					return m_data;
				}
				set
				{
					DRLTournamentData dRLTournamentData = (m_data = value);
					guid = ((dRLTournamentData == null) ? "" : dRLTournamentData.guid);
					id = ((dRLTournamentData == null) ? "" : dRLTournamentData.id);
					skill = dRLTournamentData?.minimumSkill ?? 0;
				}
			}
		}

		public enum LeaderboardType
		{
			open = 0,
			drl = 1,
			campaign = 2
		}

		[SerializeField]
		private Game m_game;

		public Leaderboards leaderboardsOpen;

		public Leaderboards leaderboardsDRL;

		public Leaderboards leaderboardsCampaign;

		public LeaderboardType lastLeaderboard;

		public Tournament tournament;

		public Game game => Reflection<object>.Assert(ref m_game);

		public void Clear()
		{
			base.assets.Clear();
			game.Clear();
			game.type = GameFlag.None;
			game.mode = GameFlag.SinglePlayer;
		}
	}
}
