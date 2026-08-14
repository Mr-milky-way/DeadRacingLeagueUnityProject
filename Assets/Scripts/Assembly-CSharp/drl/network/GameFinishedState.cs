using UnityEngine;

namespace drl.network
{
	public class GameFinishedState : IRoomState
	{
		private float kickMatchmakingTime;

		public void OnEnter(NetworkRoom room)
		{
			if (room.IsMaster)
			{
				room.MatchCount++;
			}
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
			kickMatchmakingTime += Time.deltaTime;
			if (kickMatchmakingTime >= 5f)
			{
				kickMatchmakingTime = 0f;
				if (room.IsMaster)
				{
					room.StartMatchmaking();
				}
			}
		}
	}
}
