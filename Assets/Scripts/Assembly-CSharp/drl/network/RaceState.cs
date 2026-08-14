namespace drl.network
{
	public class RaceState : IRoomState
	{
		private bool endGameSent;

		public void OnEnter(NetworkRoom room)
		{
			endGameSent = false;
			room.InGameTimeStarted = PhotonNetwork.time;
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
			if (room.IsMaster)
			{
				if (room.ActiveRacersCount == 0 && !endGameSent)
				{
					endGameSent = true;
					room.SendGameCompleted();
				}
				if (room.TimeLeft <= 0f && !endGameSent)
				{
					endGameSent = true;
					room.SendGameTimeout();
				}
			}
		}
	}
}
