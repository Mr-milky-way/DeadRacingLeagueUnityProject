using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.mvc;

namespace drl.game
{
	public class FreestyleModel : Model<DRLApp>
	{
		public TextAsset defaultRig;

		public DroneRigData rig
		{
			get
			{
				if (!defaultRig)
				{
					return null;
				}
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(defaultRig.bytes);
				return droneRigData;
			}
		}

		public List<GamePlayerData> playerStandings
		{
			get
			{
				GameModel game = base.app.model.game;
				List<GamePlayerData> list = new List<GamePlayerData>();
				for (int i = 0; i < game.players.Count; i++)
				{
					GamePlayerData playerData = game.GetPlayerData(i);
					if (playerData != null && playerData.type != GamePlayerType.Data && playerData.type != GamePlayerType.Spectator)
					{
						list.Add(playerData);
					}
				}
				list.Sort(SortGamePlayerData);
				return list;
			}
		}

		public void SetStandings(DRLStandingsView p_standings, List<GamePlayerData> p_players, bool p_clear = true)
		{
			if (p_clear || p_standings.count != p_players.Count)
			{
				p_standings.Clear();
				p_standings.SetCount(p_players.Count);
			}
			for (int i = 0; i < p_players.Count; i++)
			{
				GamePlayerData gamePlayerData = p_players[i];
				bool flag = gamePlayerData.type == GamePlayerType.Human;
				if (flag && gamePlayerData.color != base.app.model.storage.state.player.profile.color)
				{
					gamePlayerData.color = base.app.model.storage.state.player.profile.color;
				}
				DRLStandingsItemView dRLStandingsItemView = p_standings.Set(i, gamePlayerData.color, gamePlayerData.photo, gamePlayerData.name.ToUpper(), -1f, flag, gamePlayerData.playerId);
				if ((bool)dRLStandingsItemView)
				{
					dRLStandingsItemView.position = i;
				}
			}
		}

		public void SetStandings(DRLStandingsView p_standings, bool p_clear = true)
		{
			SetStandings(p_standings, playerStandings, p_clear);
		}

		private int SortGamePlayerData(GamePlayerData a, GamePlayerData b)
		{
			if (a.order >= b.order)
			{
				return 1;
			}
			return -1;
		}
	}
}
