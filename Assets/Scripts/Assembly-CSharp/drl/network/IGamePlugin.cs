using ExitGames.Client.Photon;

namespace drl.network
{
	public interface IGamePlugin
	{
		NetworkRoom.GameType GameMode { get; }

		bool FirstRacerTimeoutAllowed { get; }

		void RoomSetup();

		void LocalPlayerSetup();

		bool OnGameEvent(NetworkRoom.GameEvent gameEvent);

		void StartMatch();

		void UpdateRestrictions();

		void SwitchToRacer(NetworkActor playerToPromote);

		void SwitchToSpectator(NetworkActor playerToDowngrade, bool forced, bool notify = true);

		void OnRoomPropertiesUpdated(Hashtable propertiesThatChanged);

		void PullUsersIn();
	}
}
