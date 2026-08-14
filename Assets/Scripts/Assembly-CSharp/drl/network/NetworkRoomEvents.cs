using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace drl.network
{
	public class NetworkRoomEvents
	{
		private NetworkRoom room;

		public NetworkRoomEvents(NetworkRoom parentRoom)
		{
			room = parentRoom;
		}

		public NetworkRoom.GameEvent OnEvent(NetworkRoom.GameEventCode eventCode, object content, PhotonPlayer sender)
		{
			int num = sender?.ID ?? (-1);
			NetworkRoom.GameEvent gameEvent = new NetworkRoom.GameEvent
			{
				EventCode = eventCode,
				Content = content,
				PlayerId = num,
				Notify = true
			};
			bool flag = true;
			if (room?.Local == null)
			{
				return gameEvent;
			}
			switch (eventCode)
			{
			case NetworkRoom.GameEventCode.OnMatchmaking:
				room.State = NetworkRoom.StateCode.MatchMaking;
				break;
			case NetworkRoom.GameEventCode.OnMatchLocked:
				room.State = NetworkRoom.StateCode.MatchLocked;
				break;
			case NetworkRoom.GameEventCode.OnLoadLevel:
			{
				Hashtable hashtable3 = (Hashtable)content;
				NetworkRoom.LoadGameData loadGameData = new NetworkRoom.LoadGameData();
				if (hashtable3 != null)
				{
					loadGameData.UpdateData(hashtable3);
				}
				if (room.IsLoadingLevel)
				{
					gameEvent.Content = null;
					room.CachedLevelData = loadGameData;
					break;
				}
				gameEvent.Content = loadGameData;
				room.IsLoadingLevel = true;
				room.Local?.Reset();
				room.CachedLevelData = null;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerMarkedReady:
			{
				NetworkActor networkActor3 = room.TryGetPlayer((int)content);
				if (room?.Local == null)
				{
					break;
				}
				if (networkActor3 != null && networkActor3.IsLocal)
				{
					foreach (int value in room.CachedRemoteRacers.Values)
					{
						room.OnIncomingGameEvent(NetworkRoom.GameEventCode.OnPlayerSpawned, value, room.Local.RawData);
					}
					room.CachedRemoteRacers.Clear();
					room.SetInterestGroupEnabled(1, isEnabled: true);
				}
				gameEvent.Content = networkActor3;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerSkippedIntro:
				if (room.IsMaster)
				{
					NetworkActor networkActor6 = room.TryGetPlayer(num);
					if (networkActor6 != null && !networkActor6.HasSkippedAnimation)
					{
						networkActor6.HasSkippedAnimation = true;
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnGameWarmup:
				room.State = NetworkRoom.StateCode.GameWarmup;
				if (!room.IsMaster)
				{
					break;
				}
				foreach (NetworkActor item in room.PlayerList.FindAll((NetworkActor el) => !el.IsLevelLoaded && !el.IsSpectator))
				{
					item.IsSpectator = true;
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerLoadedLevel:
				if (room.IsMaster)
				{
					NetworkActor networkActor5 = room.TryGetPlayer(num);
					if (networkActor5 != null && !networkActor5.IsLevelLoaded)
					{
						networkActor5.IsLevelLoaded = true;
						room.Outgoing.SendLoadPlayer(num);
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerSpawned:
				if (room?.Local != null)
				{
					if (room.Local.IsGameReady)
					{
						NetworkActor content5 = room.TryGetPlayer((int)content);
						gameEvent.Content = content5;
					}
					else
					{
						int num2 = (int)content;
						room.CachedRemoteRacers[num2] = num2;
						gameEvent.Content = null;
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnGameStart:
				room.IsLoadingLevel = false;
				room.State = NetworkRoom.StateCode.GameRunning;
				room.Outgoing.ResetEvent(NetworkRoom.GameEventCode.OnPlayerLoadedLevel);
				room.Outgoing.ResetEvent(NetworkRoom.GameEventCode.OnPlayerReady);
				room.Outgoing.ResetEvent(NetworkRoom.GameEventCode.OnPlayerSkippedIntro);
				break;
			case NetworkRoom.GameEventCode.OnGameEnd:
			{
				room.State = NetworkRoom.StateCode.GameFinished;
				room.SetInterestGroupEnabled(1, isEnabled: false);
				Hashtable hashtable = (Hashtable)content;
				NetworkRoom.GameFinishedData gameFinishedData = new NetworkRoom.GameFinishedData();
				if (hashtable != null)
				{
					gameFinishedData.UpdateData(hashtable);
				}
				gameEvent.Content = gameFinishedData;
				break;
			}
			case NetworkRoom.GameEventCode.OnChatMessage:
			{
				Hashtable data = (Hashtable)content;
				gameEvent.Content = room.Chat.AddMessage(data);
				break;
			}
			case NetworkRoom.GameEventCode.OnWebHookTest:
				Debug.Log("OnWebHookTest" + (string)content);
				break;
			case NetworkRoom.GameEventCode.ErrorInfo:
				Debug.Log("PhotonEvents > OnErrorInfo: " + (string)content);
				break;
			case NetworkRoom.GameEventCode.OnSwitchedToRacer:
			{
				NetworkActor player = (NetworkActor)(gameEvent.Content = room.TryGetPlayer((int)content));
				room.UpdatePlayersOrders();
				room.AddTolastPlayersList(player);
				break;
			}
			case NetworkRoom.GameEventCode.OnSwitchedToSpectator:
			{
				NetworkActor content6 = room.TryGetPlayer((int)content);
				gameEvent.Content = content6;
				room.UpdatePlayersOrders();
				break;
			}
			case NetworkRoom.GameEventCode.OnGateEvent:
			{
				int num3 = (int)content;
				gameEvent.Content = num3;
				flag = false;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerCompletedGame:
				if (room.IsMaster)
				{
					int count = room.Racers.Count;
					room.Racers.FindAll((NetworkActor el) => el.RaceState == NetworkActor.RacerState.Complete);
					room.Progress = Mathf.Clamp01((float)(count - room.ActiveRacersCount) / (float)count);
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerForfeitGame:
			{
				int playerId2 = gameEvent.PlayerId;
				NetworkActor networkActor2 = ((room == null) ? null : room.TryGetPlayer(playerId2));
				string text = ((networkActor2 == null) ? ("player-" + playerId2) : networkActor2.ProfileName);
				Debug.Log("NetworkRoomEvent> OnEvent / OnPlayerForfeitGame - player[" + text + "]");
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerCrashed:
			{
				int playerId4 = gameEvent.PlayerId;
				NetworkActor networkActor7 = ((room == null) ? null : room.TryGetPlayer(playerId4));
				string text3 = ((networkActor7 == null) ? ("player-" + playerId4) : networkActor7.ProfileName);
				Hashtable hashtable4 = (Hashtable)content;
				NetworkRoom.DroneState droneState = new NetworkRoom.DroneState();
				if (hashtable4 != null)
				{
					droneState.UpdateData(hashtable4);
				}
				gameEvent.Content = droneState;
				Debug.Log("NetworkRoomEvent> OnEvent / OnPlayerCrash - player[" + text3 + "]");
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerDamage:
			{
				int playerId3 = gameEvent.PlayerId;
				NetworkActor networkActor4 = ((room == null) ? null : room.TryGetPlayer(playerId3));
				string text2 = ((networkActor4 == null) ? ("player-" + playerId3) : networkActor4.ProfileName);
				Hashtable hashtable2 = (Hashtable)content;
				NetworkRoom.DamageData damageData = new NetworkRoom.DamageData();
				if (hashtable2 != null)
				{
					damageData.UpdateData(hashtable2);
				}
				gameEvent.Content = damageData;
				Debug.Log("NetworkRoomEvent> OnEvent / OnPlayerDamage - player[" + text2 + "]");
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerRecovered:
			{
				int playerId = gameEvent.PlayerId;
				if (room == null)
				{
					break;
				}
				NetworkActor networkActor = ((room == null) ? null : room.TryGetPlayer(playerId));
				if (networkActor != null)
				{
					gameEvent.Content = networkActor;
					if (room.Local.ID != playerId)
					{
						Debug.Log("NetworkRoomEvent> OnEvent / OnPlayerRecovered - player[" + networkActor.ProfileName + "]");
					}
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnReplayDataReady:
			{
				string content4 = (string)content;
				gameEvent.Content = content4;
				break;
			}
			case NetworkRoom.GameEventCode.OnTrackListGenerated:
			{
				List<string> voteTrackList = (List<string>)(gameEvent.Content = new List<string>((string[])content));
				room.VoteTrackList = voteTrackList;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerVotedTrack:
			{
				NetworkActor content3 = room.TryGetPlayer(num);
				gameEvent.Content = content3;
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerKicked:
				room.Service.KickedFromRoom = room.Id;
				room.Service.TryLeaveRoom();
				break;
			case NetworkRoom.GameEventCode.OnDroneRigChanged:
			{
				string content2 = (string)content;
				gameEvent.Content = content2;
				break;
			}
			}
			if (flag)
			{
				Debug.Log("NetworkRoomEvents > OnEvent code[" + eventCode.ToString() + "] senderId[" + num + "]");
			}
			return gameEvent;
		}
	}
}
