using UnityEngine;

namespace drl.network
{
	public class WaitingForRacersState : IRoomState
	{
		private float completeAnimationDelay = 2f;

		private float timeout;

		public void OnEnter(NetworkRoom room)
		{
			timeout = room.GameLoadingTimeout;
			completeAnimationDelay = 2f;
		}

		public void OnExit(NetworkRoom room)
		{
			completeAnimationDelay = 2f;
		}

		public void OnUpdate(NetworkRoom room)
		{
			bool num = room.PlayerList.TrueForAll((NetworkActor el) => el.IsGameReady);
			bool flag = room.PlayerList.TrueForAll((NetworkActor el) => el.IsCountdownReady) || room.GameMode != NetworkRoom.GameType.Tournament;
			if (num && flag)
			{
				completeAnimationDelay -= Time.deltaTime;
				if (room.IsMaster && completeAnimationDelay <= 0f)
				{
					room.Outgoing.SendWarmupStarted();
				}
				return;
			}
			timeout -= Time.deltaTime;
			bool flag2 = timeout <= 0f;
			if (!room.IsMaster || !flag2)
			{
				return;
			}
			foreach (NetworkActor racer in room.Racers)
			{
				if (racer != null && !racer.IsGameReady)
				{
					room.Outgoing.SendPlayerKick(racer.ID);
				}
			}
			room.Outgoing.SendWarmupStarted();
		}
	}
}
