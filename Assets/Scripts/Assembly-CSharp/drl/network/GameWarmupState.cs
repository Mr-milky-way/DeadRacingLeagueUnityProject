using UnityEngine;

namespace drl.network
{
	public class GameWarmupState : IRoomState
	{
		private float nextStep = 2f;

		private float initialDelay = 1.5f;

		private float delayTimer;

		public void OnEnter(NetworkRoom room)
		{
			nextStep = 2f;
			delayTimer = 0f;
			room.ServerWarmupStarted = PhotonNetwork.time + (double)initialDelay;
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
			delayTimer += Time.deltaTime;
			if (delayTimer < initialDelay || !room.IsMaster || !(nextStep >= 0f))
			{
				return;
			}
			double num = PhotonNetwork.time - room.ServerWarmupStarted;
			if ((double)nextStep - ((double)room.WarmupTimeout - num) >= 0.0)
			{
				room.Outgoing.SendWarmupStep(nextStep);
				if (nextStep <= 0f)
				{
					room.Outgoing.SendStartGame();
				}
				nextStep -= 1f;
			}
		}
	}
}
