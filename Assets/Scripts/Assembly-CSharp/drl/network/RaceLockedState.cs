using UnityEngine;

namespace drl.network
{
	public class RaceLockedState : IRoomState
	{
		private bool sentMapLoad;

		private float elapsedTime;

		public void OnEnter(NetworkRoom room)
		{
			sentMapLoad = false;
			elapsedTime = 0f;
			room.Local.Reset();
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
			if (room.IsUsingGhosts)
			{
				if (room.PlayerList.TrueForAll((NetworkActor el) => el.GhostsProcessed) && !sentMapLoad)
				{
					Debug.Log("$RaceLockedState>OnUpdate - all players processed ghosts");
					sentMapLoad = true;
					room.StartLevelLoading();
				}
			}
			else if (!sentMapLoad)
			{
				Debug.Log("$RaceLockedState>OnUpdate - No ghosts to process");
				sentMapLoad = true;
				room.StartLevelLoading();
			}
		}
	}
}
