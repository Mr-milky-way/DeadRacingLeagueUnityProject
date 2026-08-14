namespace drl.network
{
	public class FreestyleMatchmakingState : IRoomState
	{
		private bool sentMatchStart;

		private float elapsedMatchmakingTime;

		public void OnEnter(NetworkRoom room)
		{
			sentMatchStart = false;
			elapsedMatchmakingTime = 0f;
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
				if (!sentMatchStart && room.Local.IsRoomReady)
				{
					sentMatchStart = true;
					room.GamePlugin.StartMatch();
				}
				return;
			}
			bool flag = room.Racers.TrueForAll((NetworkActor el) => el.IsRoomReady);
			if (room.RacersCount >= room.MinRequiredRacers && flag && !sentMatchStart)
			{
				sentMatchStart = true;
				room.GamePlugin.StartMatch();
			}
		}
	}
}
