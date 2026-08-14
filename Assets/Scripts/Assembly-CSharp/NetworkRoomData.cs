using System;
using System.Collections.Generic;
using UnityEngine;
using drl.network;

[Serializable]
public class NetworkRoomData
{
	[SerializeField]
	private bool inRoom;

	[Header("[Base Properties]")]
	[SerializeField]
	private int playerCount;

	[SerializeField]
	private int maxPlayers;

	[SerializeField]
	private NetworkRoom.StateCode state;

	[Header("[Sync Properties]")]
	[SerializeField]
	private int activeRacersCount;

	[SerializeField]
	private bool allowGhosts;

	[SerializeField]
	private bool autoColor;

	[SerializeField]
	private bool canRace;

	[SerializeField]
	private bool canSpectate;

	[SerializeField]
	private string customMapId;

	[SerializeField]
	private string customMapName;

	[SerializeField]
	private int droneClass;

	[SerializeField]
	private bool drlPilotMode;

	[SerializeField]
	private string selectedDrone;

	[SerializeField]
	private float elapsedTime;

	[SerializeField]
	private NetworkRoom.GameType gameMode;

	[SerializeField]
	private List<NetworkGhost> ghosts = new List<NetworkGhost>();

	[SerializeField]
	private string ghostsData;

	[SerializeField]
	private int heatIdx;

	[SerializeField]
	private int lobbyCountdown;

	[SerializeField]
	private string mapId;

	[SerializeField]
	private MapCategory mapVotingCategory;

	[SerializeField]
	private int matchCount;

	[SerializeField]
	private NetworkRoom.MatchmakingFlow matchmakingType;

	[SerializeField]
	private string matchId;

	[SerializeField]
	private string raceId;

	[SerializeField]
	private int maxHeats;

	[SerializeField]
	private int maxRacers;

	[SerializeField]
	private int maxSpectators;

	[SerializeField]
	private string playersIdsData;

	[SerializeField]
	private float progress;

	[SerializeField]
	private int racersCount;

	[SerializeField]
	private string roomTitle;

	[SerializeField]
	private NetworkRoom.StateCode serverState;

	[SerializeField]
	private float timeLimit;

	[SerializeField]
	private string tournamentId;

	[SerializeField]
	private string trackId;

	[SerializeField]
	private NetworkActorData local = new NetworkActorData();

	[SerializeField]
	private NetworkActorData master = new NetworkActorData();

	[SerializeField]
	private TimeoutMode timeoutMode;

	public NetworkRoom Room;

	public void Enter(NetworkRoom room)
	{
		Room = room;
		local = new NetworkActorData();
		master = new NetworkActorData();
		ghosts = new List<NetworkGhost>();
		inRoom = true;
	}

	public void Left()
	{
		Room = null;
		local = new NetworkActorData();
		master = new NetworkActorData();
		ghosts = new List<NetworkGhost>();
		inRoom = false;
	}

	public void Update(NetworkRoom room)
	{
		if (room != null)
		{
			playerCount = room.PlayerCount;
			maxPlayers = room.MaxPlayers;
			state = room.State;
			activeRacersCount = room.ActiveRacersCount;
			allowGhosts = room.AllowGhosts;
			autoColor = room.AutoColor;
			canRace = room.CanRace;
			canSpectate = room.CanSpectate;
			customMapId = room.CustomMapId;
			customMapName = room.CustomMapName;
			droneClass = room.DroneClass;
			drlPilotMode = room.DRLPilotMode;
			selectedDrone = room.SelectedDrone;
			elapsedTime = room.ElapsedTime;
			gameMode = room.GameMode;
			ghosts = room.Ghosts;
			ghostsData = room.GhostsData;
			heatIdx = room.HeatIdx;
			lobbyCountdown = room.LobbyCountdown;
			mapId = room.MapId;
			mapVotingCategory = room.MapVotingCategory;
			matchCount = room.MatchCount;
			matchmakingType = room.MatchmakingType;
			matchId = room.MatchId;
			raceId = room.RaceId;
			maxHeats = room.MaxHeats;
			maxRacers = room.MaxRacers;
			maxSpectators = room.MaxSpectators;
			playersIdsData = room.PlayersIdsData;
			progress = room.Progress;
			racersCount = room.RacersCount;
			roomTitle = room.RoomTitle;
			serverState = room.ServerState;
			timeLimit = (float)room.TimeLimit;
			tournamentId = room.TournamentId;
			trackId = room.TrackId;
			timeoutMode = room.TimeoutMode;
			local.Update(room.Local);
			master.Update(room.Master);
		}
	}
}
