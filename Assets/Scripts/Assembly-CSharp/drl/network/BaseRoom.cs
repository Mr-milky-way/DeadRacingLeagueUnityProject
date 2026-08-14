using ExitGames.Client.Photon;

namespace drl.network
{
	public abstract class BaseRoom : IGamePlugin
	{
		protected NetworkRoom room;

		public abstract NetworkRoom.GameType GameMode { get; }

		public abstract bool FirstRacerTimeoutAllowed { get; }

		public abstract void RoomSetup();

		public abstract void LocalPlayerSetup();

		public abstract void StartMatch();

		public abstract bool OnGameEvent(NetworkRoom.GameEvent gameEvent);

		public BaseRoom(NetworkRoom parentRoom)
		{
			room = parentRoom;
		}

		public virtual void UpdateRestrictions()
		{
			if (room != null && room.IsMaster)
			{
				room.MaxPlayers = room.MaxRacers + room.MaxSpectators;
				room.RacersCount = room.Racers.Count;
				room.SpectatorsCount = room.Spectators.Count;
				room.ActiveRacersCount = room.Racers.FindAll((NetworkActor el) => el.RaceState == NetworkActor.RacerState.Running).Count;
				room.ForfeitRacersCount = room.Racers.FindAll((NetworkActor el) => el.RaceState == NetworkActor.RacerState.Forfeit).Count;
				room.CompleteRacersCount = room.Racers.FindAll((NetworkActor el) => el.RaceState == NetworkActor.RacerState.Complete).Count;
			}
		}

		public virtual void SwitchToRacer(NetworkActor playerToPromote)
		{
			bool flag = room.RacersCount < room.MaxRacers;
			if ((room.ServerState == NetworkRoom.StateCode.MatchMaking || room.ServerState == NetworkRoom.StateCode.GameFinished) && flag)
			{
				playerToPromote.IsSpectator = false;
				room.Outgoing.SendSwitchToRacer(playerToPromote.ID);
			}
		}

		public virtual void SwitchToSpectator(NetworkActor playerToDowngrade, bool forced, bool notify = true)
		{
			bool flag = room.SpectatorsCount < room.MaxSpectators;
			bool flag2 = room.ServerState == NetworkRoom.StateCode.MatchMaking || room.ServerState == NetworkRoom.StateCode.GameFinished;
			if ((room.RacersCount >= 2 && flag2 && flag) || forced)
			{
				playerToDowngrade.IsSpectator = true;
				room.Outgoing.SendSwitchToSpectator(playerToDowngrade.ID, notify);
			}
		}

		public virtual void OnRoomPropertiesUpdated(Hashtable propertiesThatChanged)
		{
		}

		public virtual void PullUsersIn()
		{
		}
	}
}
