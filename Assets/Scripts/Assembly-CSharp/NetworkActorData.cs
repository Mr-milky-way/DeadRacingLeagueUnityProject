using System;
using UnityEngine;
using drl.network;

[Serializable]
public class NetworkActorData
{
	[SerializeField]
	private int id;

	[SerializeField]
	private string userId;

	[SerializeField]
	private bool isMaster;

	[SerializeField]
	private bool isLocal;

	[SerializeField]
	private int badgeLevel;

	[SerializeField]
	private float cameraFov;

	[SerializeField]
	private float cameraTilt;

	[SerializeField]
	private int controllerType;

	[SerializeField]
	private bool isGameReady;

	[SerializeField]
	private bool isLevelLoaded;

	[SerializeField]
	private bool isReplaySent;

	[SerializeField]
	private bool isRoomReady;

	[SerializeField]
	private bool isSpectator;

	[SerializeField]
	private Color mainColor;

	[SerializeField]
	private int order;

	[SerializeField]
	private Color profileColor;

	[SerializeField]
	private string profileName;

	[SerializeField]
	private string profilePhoto;

	[SerializeField]
	private NetworkActor.RacerState raceState;

	[SerializeField]
	private float raceTime;

	[SerializeField]
	private string platformId;

	[SerializeField]
	private string playerId;

	[SerializeField]
	private int viewId;

	[SerializeField]
	private string votedTrackGUID;

	[SerializeField]
	private bool hasSubmittedLeaderboard;

	[HideInInspector]
	public NetworkActor Actor;

	public int ID => id;

	public void Update(NetworkActor actor)
	{
		Actor = actor;
		if (actor != null)
		{
			id = actor.ID;
			userId = actor.UserId;
			isMaster = actor.IsMaster;
			isLocal = actor.IsLocal;
			badgeLevel = actor.BadgeLevel;
			cameraFov = actor.CameraFOV;
			cameraTilt = actor.CameraTilt;
			controllerType = actor.ControllerType;
			isGameReady = actor.IsGameReady;
			isLevelLoaded = actor.IsLevelLoaded;
			isReplaySent = actor.IsReplaySent;
			isRoomReady = actor.IsRoomReady;
			isSpectator = actor.IsSpectator;
			mainColor = actor.MainColor;
			order = actor.Order;
			profileColor = actor.ProfileColor;
			profileName = actor.ProfileName;
			profilePhoto = actor.ProfilePhoto;
			raceState = actor.RaceState;
			raceTime = actor.RaceTime;
			platformId = actor.PlatformId;
			playerId = actor.PlayerId;
			viewId = actor.ViewId;
			votedTrackGUID = actor.VotedTrackGUID;
			hasSubmittedLeaderboard = actor.HasSubmittedLeaderboard;
		}
	}
}
