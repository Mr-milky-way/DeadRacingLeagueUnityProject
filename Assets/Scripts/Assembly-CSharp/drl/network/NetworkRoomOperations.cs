using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.network
{
	public class NetworkRoomOperations
	{
		public class Operation
		{
			public readonly NetworkRoom.GameEventCode eventCode;

			public readonly object eventContent;

			public readonly bool isReliable;

			public readonly RaiseEventOptions eventOptions;

			public readonly bool isUnique;

			public readonly bool debugOperation;

			public readonly string uniqueKey;

			public readonly bool notifyApp;

			public Operation(NetworkRoom.GameEventCode p_eventCode, object p_content, bool p_reliable, RaiseEventOptions p_options, bool p_unique = false, bool p_debug = false, string p_unique_key = null, bool p_notify = false)
			{
				eventCode = p_eventCode;
				eventContent = p_content;
				isReliable = p_reliable;
				eventOptions = p_options;
				isUnique = p_unique;
				debugOperation = p_debug;
				uniqueKey = p_unique_key;
				notifyApp = p_notify;
			}
		}

		private Queue<Operation> operationBuffer = new Queue<Operation>();

		private NetworkRoom room;

		public Dictionary<string, NetworkRoom.GameEvent> Record { get; private set; }

		public NetworkRoomOperations(NetworkRoom parentRoom)
		{
			room = parentRoom;
			Record = new Dictionary<string, NetworkRoom.GameEvent>();
		}

		public void Reset()
		{
			if (room.IsMaster)
			{
				RemoveFromCache(NetworkRoom.GameEventCode.OnLoadLevel);
				RemoveFromCache(NetworkRoom.GameEventCode.OnPlayerSpawned);
				RemoveCacheOfLeftPlayers();
			}
			Record.Clear();
			operationBuffer.Clear();
			if (PhotonNetwork.networkingPeer != null)
			{
				NetworkingPeer networkingPeer = PhotonNetwork.networkingPeer;
				networkingPeer.OnReconnected = (Action)Delegate.Remove(networkingPeer.OnReconnected, new Action(ResendQueuedOperations));
				NetworkingPeer networkingPeer2 = PhotonNetwork.networkingPeer;
				networkingPeer2.OnReconnected = (Action)Delegate.Combine(networkingPeer2.OnReconnected, new Action(ResendQueuedOperations));
			}
		}

		public void ResetEvent(NetworkRoom.GameEventCode eventCodeToReset)
		{
			if (Record.ContainsKey(eventCodeToReset.ToString()))
			{
				Record.Remove(eventCodeToReset.ToString());
			}
		}

		public void RemoveFromCache(NetworkRoom.GameEventCode eventCode)
		{
			RaiseEventOptions options = new RaiseEventOptions
			{
				CachingOption = EventCaching.RemoveFromRoomCache
			};
			PhotonNetwork.RaiseEvent((byte)eventCode, null, sendReliable: true, options);
		}

		public void RemoveCacheOfLeftPlayers()
		{
			RaiseEventOptions options = new RaiseEventOptions
			{
				CachingOption = EventCaching.RemoveFromRoomCacheForActorsLeft
			};
			PhotonNetwork.RaiseEvent(0, null, sendReliable: true, options);
		}

		private bool SendOperation(NetworkRoom.GameEventCode p_event, object p_content, bool p_reliable, RaiseEventOptions p_options, bool uniqueCall = false, bool debugLog = true, string uniqueKey = null, bool p_notifyApp = true)
		{
			if (PhotonNetwork.networkingPeer != null && PhotonNetwork.networkingPeer.IsReconnecting())
			{
				operationBuffer.Enqueue(new Operation(p_event, p_content, p_reliable, p_options, uniqueCall, debugLog, uniqueKey, p_notifyApp));
				Debug.Log("NetworkRoomOperations> " + PhotonNetwork.networkingPeer.PlayerName + " is reconnecting - can't send " + p_event.ToString() + " - enqueuing event.");
			}
			string key = (string.IsNullOrEmpty(uniqueKey) ? p_event.ToString() : uniqueKey);
			if (uniqueCall && Record.ContainsKey(key))
			{
				return false;
			}
			if (PhotonNetwork.RaiseEvent((byte)p_event, p_content, p_reliable, p_options))
			{
				NetworkRoom.GameEvent value = new NetworkRoom.GameEvent
				{
					EventCode = p_event,
					Content = p_content,
					PlayerId = room.Local.ID,
					Notify = p_notifyApp
				};
				Record[key] = value;
				if (debugLog)
				{
					Debug.Log("NetworkRoomOperations > SendOperation eventCode[" + p_event.ToString() + "]");
				}
				return true;
			}
			return false;
		}

		public bool SendMatchmakingStarted(int newPlayerId = -1)
		{
			if (newPlayerId == -1)
			{
				return SendOperation(NetworkRoom.GameEventCode.OnMatchmaking, null, p_reliable: true, new RaiseEventOptions
				{
					Receivers = ReceiverGroup.All
				});
			}
			return SendOperation(NetworkRoom.GameEventCode.OnMatchmaking, null, p_reliable: true, new RaiseEventOptions
			{
				TargetActors = new int[1] { newPlayerId }
			});
		}

		public bool SendMatchLocked()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnMatchLocked, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendPullUsers()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPullUsersIn, room.MatchId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendLoadLevel(NetworkRoom.LoadGameData data)
		{
			RaiseEventOptions options = new RaiseEventOptions
			{
				CachingOption = EventCaching.RemoveFromRoomCache
			};
			PhotonNetwork.RaiseEvent(2, null, sendReliable: true, options);
			return SendOperation(NetworkRoom.GameEventCode.OnLoadLevel, data.ToHashTable(), p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All,
				CachingOption = EventCaching.AddToRoomCacheGlobal
			});
		}

		public bool SendLevelLoaded()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerLoadedLevel, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendWarmupStarted()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGameWarmup, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendWarmupStep(float countdownTime)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGameWarmupStep, countdownTime, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendStartGame()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGameStart, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendLoadPlayer(int playerId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnLoadPlayer, null, p_reliable: true, new RaiseEventOptions
			{
				TargetActors = new int[1] { playerId }
			});
		}

		public bool SendPlayerReady()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerReady, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.MasterClient
			}, uniqueCall: true);
		}

		public bool SendPlayerCountdownReady()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerCountdownReady, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.MasterClient
			}, uniqueCall: true);
		}

		public bool SendPlayerSkippedIntro()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerSkippedIntro, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.MasterClient
			}, uniqueCall: true);
		}

		public bool SendPlayerMarkedReady(int playerReady)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerMarkedReady, playerReady, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendStartGame(int playerReady)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGameStart, null, p_reliable: true, new RaiseEventOptions
			{
				TargetActors = new int[1] { playerReady }
			});
		}

		public bool SendEndGame(NetworkRoom.GameFinishedData finishedData)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGameEnd, finishedData.ToHashTable(), p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendChatMessage(NetworkRoomChat.Message message)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnChatMessage, message.ToHashTable(), p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendLocalPlayerSpawned(int spawnedPlayerId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerSpawned, spawnedPlayerId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others,
				CachingOption = EventCaching.AddToRoomCache
			});
		}

		public bool SendWebHookTest()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnWebHookTest, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All,
				ForwardToWebhook = true
			});
		}

		public bool SendSwitchToRacer(int playerId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnSwitchedToRacer, playerId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendSwitchToSpectator(int playerId, bool notifyApp = true)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnSwitchedToSpectator, playerId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: false, debugLog: false, null, notifyApp);
		}

		public bool SendGateEvent(int gateId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnGateEvent, gateId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			}, uniqueCall: false, debugLog: false);
		}

		public bool SendPlayerCompletedGame()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerCompletedGame, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendPlayerSubmittedLeaderboard()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerSubmittedLeaderboard, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendPlayerForfeitGame()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerForfeitGame, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendPlayerCrashed(NetworkRoom.DroneState droneState)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerCrashed, droneState.ToHashTable(), p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendPlayerDamage(NetworkRoom.DamageData data)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerDamage, data.ToHashTable(), p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendPlayerRecovered(int playerId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerRecovered, playerId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendReplayData(string replayDataUrl)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnReplayDataReady, replayDataUrl, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendPlayerVotedTrack()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerVotedTrack, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendTrackListGenerated(string[] trackList)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnTrackListGenerated, trackList, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			}, uniqueCall: true);
		}

		public bool SendPlayerKick(int playerId)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnPlayerKicked, null, p_reliable: true, new RaiseEventOptions
			{
				TargetActors = new int[1] { playerId }
			});
		}

		public bool SendDroneRigChanged(string newDroneRig)
		{
			return SendOperation(NetworkRoom.GameEventCode.OnDroneRigChanged, newDroneRig, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendUpdateRacerOrder()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnOrderUpdate, null, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			});
		}

		public bool SendRaceReady()
		{
			return SendOperation(NetworkRoom.GameEventCode.OnRaceReady, room.MatchId, p_reliable: true, new RaiseEventOptions
			{
				Receivers = ReceiverGroup.MasterClient
			});
		}

		public bool SendOperation(Operation p_operation)
		{
			if (p_operation == null)
			{
				return false;
			}
			string key = (string.IsNullOrEmpty(p_operation.uniqueKey) ? p_operation.eventCode.ToString() : p_operation.uniqueKey);
			if (p_operation.isUnique && Record.ContainsKey(key))
			{
				return false;
			}
			if (PhotonNetwork.RaiseEvent((byte)p_operation.eventCode, p_operation.eventContent, p_operation.isReliable, p_operation.eventOptions))
			{
				NetworkRoom.GameEvent value = new NetworkRoom.GameEvent
				{
					EventCode = p_operation.eventCode,
					Content = p_operation.eventContent,
					PlayerId = room.Local.ID,
					Notify = p_operation.notifyApp
				};
				Record[key] = value;
				if (p_operation.debugOperation)
				{
					Debug.Log("NetworkRoomOperations > SendOperation eventCode[" + p_operation.eventCode.ToString() + "]");
				}
				return true;
			}
			return false;
		}

		private void ResendQueuedOperations()
		{
			Debug.Log($"NetworkRoomOperations> Resending queued operations on reconnect [{operationBuffer.Count}]");
			if (operationBuffer.Count != 0)
			{
				while (operationBuffer.Count > 0)
				{
					SendOperation(operationBuffer.Dequeue());
				}
			}
		}
	}
}
