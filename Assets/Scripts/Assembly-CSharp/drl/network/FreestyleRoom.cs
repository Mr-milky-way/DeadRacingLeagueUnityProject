using System;

namespace drl.network
{
	[Serializable]
	public class FreestyleRoom : BaseRoom
	{
		public override NetworkRoom.GameType GameMode => NetworkRoom.GameType.Freestyle;

		public override bool FirstRacerTimeoutAllowed => false;

		public FreestyleRoom(NetworkRoom parentRoom)
			: base(parentRoom)
		{
			room.MapVotingCategory = MapCategory.None;
		}

		public override void RoomSetup()
		{
			room.StateMachine.ClearAllStates();
			room.StateMachine.AddState(NetworkRoom.StateCode.MatchMaking, new FreestyleMatchmakingState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameRunning, new FreestyleState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameFinished, new GameFinishedState());
		}

		public override void LocalPlayerSetup()
		{
			room.Local.IsLevelLoaded = false;
			room.Local.IsGameReady = false;
			room.Local.IsSpectator = !room.CanRace;
			room.Local.HasSkippedAnimation = false;
			room.Local.IsRoomReady = false;
		}

		public override void StartMatch()
		{
			room.Local.Reset();
			room.StartLevelLoading();
		}

		public override bool OnGameEvent(NetworkRoom.GameEvent gameEvent)
		{
			bool result = true;
			switch (gameEvent.EventCode)
			{
			case NetworkRoom.GameEventCode.OnPlayerSpawned:
				result = gameEvent.Content != null;
				break;
			case NetworkRoom.GameEventCode.OnPlayerReady:
				if (room.IsMaster)
				{
					NetworkActor networkActor = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor != null && !networkActor.IsGameReady)
					{
						networkActor.IsGameReady = true;
						room.Outgoing.SendStartGame(networkActor.ID);
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerCountdownReady:
				if (room.IsMaster)
				{
					NetworkActor networkActor2 = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor2 != null && networkActor2.IsGameReady)
					{
						networkActor2.IsCountdownReady = true;
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnGameStart:
				foreach (int value in room.CachedRemoteRacers.Values)
				{
					room.OnIncomingGameEvent(NetworkRoom.GameEventCode.OnPlayerSpawned, value, room.Local.RawData);
				}
				room.CachedRemoteRacers.Clear();
				room.SetInterestGroupEnabled(1, isEnabled: true);
				break;
			}
			return result;
		}

		public override void UpdateRestrictions()
		{
			if (room != null && room.IsMaster)
			{
				base.UpdateRestrictions();
				room.CanRace = room.RacersCount < room.MaxRacers;
				room.CanSpectate = room.SpectatorsCount < room.MaxSpectators;
				room.PhotonRoom.IsOpen = room.Players.Count < room.MaxPlayers;
			}
		}

		public override void SwitchToRacer(NetworkActor playerToPromote)
		{
			if (room.RacersCount < room.MaxRacers)
			{
				playerToPromote.IsSpectator = false;
				room.Outgoing.SendSwitchToRacer(playerToPromote.ID);
			}
		}

		public override void SwitchToSpectator(NetworkActor playerToDowngrade, bool forced, bool notify = true)
		{
			if ((room.SpectatorsCount < room.MaxSpectators && room.RacersCount >= 2) || forced)
			{
				playerToDowngrade.IsSpectator = true;
				room.Outgoing.SendSwitchToSpectator(playerToDowngrade.ID, notify);
			}
		}
	}
}
