using UnityEngine;

namespace drl.network
{
	public class RaceMatchmakinngState : IRoomState
	{
		private bool sentMatchStart;

		private float elapsedMatchmakingTime;

		public void OnEnter(NetworkRoom room)
		{
			sentMatchStart = false;
			elapsedMatchmakingTime = 0f;
			room.Local.GhostsProcessed = false;
			room.Local.GhostsProcessing = false;
			room.Reset();
			room.LobbyCountdown = room.MatchmakingTimeout;
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
			if (!room.IsMaster)
			{
				return;
			}
			if (room.IsQuickMatch)
			{
				if (room.RacersCount > 1)
				{
					elapsedMatchmakingTime += Time.deltaTime;
					if (elapsedMatchmakingTime >= 1f)
					{
						elapsedMatchmakingTime = 0f;
						room.LobbyCountdown--;
						if (!sentMatchStart && room.LobbyCountdown < 0)
						{
							sentMatchStart = true;
							room.GamePlugin.StartMatch();
						}
					}
					if (!sentMatchStart && room.RacersCount == room.MaxRacers && room.MatchCount == 0)
					{
						sentMatchStart = true;
						room.GamePlugin.StartMatch();
					}
				}
				else
				{
					room.LobbyCountdown = room.MatchmakingTimeout;
				}
			}
			else
			{
				bool flag = room.Racers.TrueForAll((NetworkActor el) => el.IsRoomReady);
				bool flag2 = room.Racers.TrueForAll((NetworkActor el) => !el.GhostsProcessing) && room.Spectators.TrueForAll((NetworkActor el) => !el.GhostsProcessing);
				if (room.RacersCount >= room.MinRequiredRacers && flag && !sentMatchStart && flag2)
				{
					sentMatchStart = true;
					room.ForceStartMatch();
				}
			}
		}
	}
}
