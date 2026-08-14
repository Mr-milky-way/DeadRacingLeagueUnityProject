using System.Collections.Generic;
using ExitGames.Client.Photon;
using Newtonsoft.Json;

namespace drl.network
{
	public class TournamentRoom : BaseRoom
	{
		public override NetworkRoom.GameType GameMode => NetworkRoom.GameType.Tournament;

		public override bool FirstRacerTimeoutAllowed => false;

		public TournamentRoom(NetworkRoom parentRoom)
			: base(parentRoom)
		{
			room.AutoColor = true;
		}

		public override void RoomSetup()
		{
			room.StateMachine.ClearAllStates();
			room.StateMachine.AddState(NetworkRoom.StateCode.MatchMaking, new TournamentMatchmakingState());
			room.StateMachine.AddState(NetworkRoom.StateCode.MatchLocked, new MatchLockedState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameLoading, new WaitingForRacersState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameWarmup, new GameWarmupState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameRunning, new RaceState());
			room.StateMachine.AddState(NetworkRoom.StateCode.GameFinished, new TournamentFinishedState());
		}

		public override void LocalPlayerSetup()
		{
			bool flag = false;
			string[] expectedUsers = room.PhotonRoom.ExpectedUsers;
			if (expectedUsers != null && expectedUsers.Length != 0)
			{
				string[] array = expectedUsers;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == room.Local.PlayerId)
					{
						flag = true;
						break;
					}
				}
				room.Local.IsSpectator = !flag;
			}
			else
			{
				room.Local.IsSpectator = !room.CanRace;
			}
			room.Local.IsLevelLoaded = false;
			room.Local.IsGameReady = false;
			room.Local.HasSkippedAnimation = false;
			room.Local.IsRoomReady = false;
		}

		public override void StartMatch()
		{
			room.Outgoing.SendUpdateRacerOrder();
		}

		public override void PullUsersIn()
		{
			room.Outgoing.SendPullUsers();
		}

		public override bool OnGameEvent(NetworkRoom.GameEvent gameEvent)
		{
			bool result = true;
			switch (gameEvent.EventCode)
			{
			case NetworkRoom.GameEventCode.OnMatchLocked:
				room.TimeLimit = 180.0;
				room.StartLevelLoading();
				break;
			case NetworkRoom.GameEventCode.OnLoadLevel:
				room.State = NetworkRoom.StateCode.GameLoading;
				break;
			case NetworkRoom.GameEventCode.OnPlayerReady:
				if (room.IsMaster)
				{
					NetworkActor networkActor3 = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor3 != null && !networkActor3.IsGameReady)
					{
						networkActor3.IsGameReady = true;
						room.Outgoing.SendPlayerMarkedReady(networkActor3.ID);
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
			case NetworkRoom.GameEventCode.OnPlayerSubmittedLeaderboard:
				if (room.IsMaster)
				{
					NetworkActor networkActor = room.TryGetPlayer(gameEvent.PlayerId);
					if (networkActor != null)
					{
						networkActor.HasSubmittedLeaderboard = true;
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
				bool flag = serverState == NetworkRoom.StateCode.GameRunning || serverState == NetworkRoom.StateCode.GameWarmup || serverState == NetworkRoom.StateCode.GameFinished || serverState == NetworkRoom.StateCode.GameLoading;
				room.CanRace = room.HeatAllowed && !flag && room.RacersCount < room.MaxRacers;
				room.CanSpectate = room.HeatAllowed && !flag && room.SpectatorsCount < room.MaxSpectators;
				room.PhotonRoom.IsOpen = room.HeatAllowed && room.Players.Count < room.MaxPlayers;
			}
		}

		public override void OnRoomPropertiesUpdated(Hashtable propertiesThatChanged)
		{
			base.OnRoomPropertiesUpdated(propertiesThatChanged);
			if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey("fc"))
			{
				room.FixedColorsLocal = (string.IsNullOrEmpty(room.FixedColors) ? new Dictionary<string, int>() : JsonConvert.DeserializeObject<Dictionary<string, int>>(room.FixedColors));
			}
		}
	}
}
