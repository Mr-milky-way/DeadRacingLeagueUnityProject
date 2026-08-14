namespace drl.network
{
	public class RaceRoom : BaseRoom
	{
		public override NetworkRoom.GameType GameMode => NetworkRoom.GameType.Race;

		public override bool FirstRacerTimeoutAllowed => true;

		public RaceRoom(NetworkRoom parentRoom)
			: base(parentRoom)
		{
			room.MinRequiredRacers = 2;
			room.AllowGhosts = true;
		}

		public override void RoomSetup()
		{
			room.StateMachine.ClearAllStates();
			room.StateMachine.AddState(NetworkRoom.StateCode.MatchMaking, new RaceMatchmakinngState());
			room.StateMachine.AddState(NetworkRoom.StateCode.MatchLocked, new RaceLockedState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameLoading, new WaitingForRacersState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameWarmup, new GameWarmupState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameRunning, new RaceState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameFinished, new GameFinishedState());
		}

		public override void LocalPlayerSetup()
		{
			room.Local.IsLevelLoaded = false;
			room.Local.IsGameReady = false;
			room.Local.IsSpectator = !room.CanRace;
			room.Local.HasSkippedAnimation = false;
			room.Local.IsRoomReady = false;
			room.TryUpdateLocalGhosts();
		}

		public override void StartMatch()
		{
			room.Outgoing.SendMatchLocked();
		}

		public override bool OnGameEvent(NetworkRoom.GameEvent gameEvent)
		{
			bool result = true;
			switch (gameEvent.EventCode)
			{
			case NetworkRoom.GameEventCode.OnLoadLevel:
				room.State = NetworkRoom.StateCode.GameLoading;
				break;
			case NetworkRoom.GameEventCode.OnPlayerReady:
				if (room.IsMaster)
				{
					NetworkActor networkActor2 = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor2 != null && !networkActor2.IsGameReady)
					{
						networkActor2.IsGameReady = true;
						room.Outgoing.SendPlayerMarkedReady(networkActor2.ID);
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerCountdownReady:
				if (room.IsMaster)
				{
					NetworkActor networkActor = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor != null && networkActor.IsGameReady)
					{
						networkActor.IsCountdownReady = true;
					}
				}
				break;
			}
			return result;
		}

		public override void UpdateRestrictions()
		{
			if (room != null && room.IsMaster)
			{
				base.UpdateRestrictions();
				NetworkRoom.StateCode serverState = room.ServerState;
				bool flag = serverState == NetworkRoom.StateCode.MatchLocked || serverState == NetworkRoom.StateCode.GameLoading || serverState == NetworkRoom.StateCode.GameWarmup || serverState == NetworkRoom.StateCode.GameRunning || serverState == NetworkRoom.StateCode.GameFinished;
				room.CanRace = !flag && room.RacersCount < room.MaxRacers;
				room.CanSpectate = !flag && room.SpectatorsCount < room.MaxSpectators;
				room.PhotonRoom.IsOpen = room.CanRace || room.CanSpectate;
			}
		}
	}
}
