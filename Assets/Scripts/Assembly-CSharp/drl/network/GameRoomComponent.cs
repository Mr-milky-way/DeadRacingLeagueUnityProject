using System.Collections.Generic;

namespace drl.network
{
	public class GameRoomComponent : PhotonComponent<NetworkRoom>
	{
		public string Name;

		public NetworkRoom.GameType GameMode;

		public int PlayerCount;

		public Dictionary<int, PhotonPlayerComponent> Players = new Dictionary<int, PhotonPlayerComponent>();

		public override void UpdateData(NetworkRoom data)
		{
			base.UpdateData(data);
			Name = data.Id;
			GameMode = data.GameMode;
			PlayerCount = data.PlayerCount;
		}

		public void AddPlayer(NetworkActor player)
		{
			PhotonPlayerComponent photonPlayerComponent = PhotonDebugHelper.AddHelperComponent<PhotonPlayerComponent>("Player:" + player.ID, base.gameObject);
			photonPlayerComponent.UpdateData(player);
			Players.Add(player.ID, photonPlayerComponent);
		}

		public void RemovePlayer(int playerID)
		{
			PhotonPlayerComponent value = null;
			if (Players.TryGetValue(playerID, out value))
			{
				PhotonDebugHelper.RemoveHelper<PhotonPlayerComponent>(value.gameObject);
				Players.Remove(playerID);
			}
		}
	}
}
