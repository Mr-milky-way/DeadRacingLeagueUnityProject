using System;
using System.Collections.Generic;
using Photon.Chat;
using UnityEngine;
using drl.game;

namespace drl.chat
{
	public class ChatService : MonoBehaviour
	{
		public enum ServiceState
		{
			Disconnected = 0,
			InProgress = 1,
			Connected = 2
		}

		public enum OnlineStatus
		{
			Offline = 0,
			Online = 2,
			Playing = 6
		}

		[SerializeField]
		private ChatServiceData chatServiceData;

		public readonly Dictionary<string, ChatRoom> PublicChannels = new Dictionary<string, ChatRoom>();

		public readonly Dictionary<string, ChatRoom> PrivateChannels = new Dictionary<string, ChatRoom>();

		public Action<ServiceState> OnStateChanged;

		public Action<ChatRoom, IChatSendable> OnIncomingMessage;

		public Action<string, ChatRoom.Scope> OnChatRoomJoined;

		public Action<string, ChatRoom.Scope> OnChatRoomLeft;

		public Action<string, string> OnPlayerJoinedChat;

		public Action<string, string> OnPlayerLeftChat;

		public Action<string> OnTournamentNotification;

		private string m_userId = "";

		private readonly Dictionary<string, ChatRoom.Scope> subscribedChannels = new Dictionary<string, ChatRoom.Scope>();

		private bool autoConnect;

		private float autoConnectTimer;

		private ChatServiceListener listener;

		private DateTime sessionDateTime;

		public bool IsOnline
		{
			get
			{
				if (Client != null)
				{
					return Client.CanChat;
				}
				return false;
			}
		}

		public ServiceState State { get; private set; }

		public string UserId
		{
			get
			{
				if (Client != null)
				{
					return Client.UserId;
				}
				return "";
			}
			set
			{
				m_userId = value;
			}
		}

		public string Username { get; private set; }

		public string PlayerId { get; set; }

		public string PlatformId { get; set; }

		public Color UserColor { get; set; }

		public int BadgeLevel { get; set; }

		public string Platform { get; set; }

		public ChatClient Client { get; private set; }

		public string APIKEY => "3234c075-731a-4d7e-9913-5a97a793cc06";

		public void Init()
		{
			Debug.Log("ChatService> Init");
			listener = new ChatServiceListener();
			listener.OnStateChanged = SetServiceState;
			listener.OnIncomingMessage = ProcessIncomingMessage;
			listener.OnChannelSubscribed = OnSubscribedToChannel;
			listener.OnChannelUnsubscribed = OnUnsubscribedToChannel;
			listener.OnUserStatusChanged = OnUserStatusChanged;
			listener.OnUserSubscribedToChannel = OnUserSubscribedToChannel;
			listener.OnUserUnsubscribedToChannel = OnUserUnsubscribedToChannel;
			Client = new ChatClient(listener);
		}

		private void OnUserSubscribedToChannel(string channelId, string user)
		{
			OnPlayerJoinedChat?.Invoke(channelId, user);
		}

		private void OnUserUnsubscribedToChannel(string channelId, string user)
		{
			OnPlayerLeftChat?.Invoke(channelId, user);
		}

		public void Connect(string userId, string username, bool keepConnectionAlive = true)
		{
			if (Client == null)
			{
				Debug.LogError("ChatService > Connect - Chat client was null, unable to connect ");
				return;
			}
			m_userId = userId;
			Username = username;
			autoConnect = keepConnectionAlive;
			Client.ChatRegion = "US";
			Client.Connect(APIKEY, DRLVersion.server, new Photon.Chat.AuthenticationValues(userId));
			sessionDateTime = DRLTime.serverClock;
		}

		public void Disconnect()
		{
			if (Client != null)
			{
				autoConnect = false;
				Client.Disconnect();
			}
		}

		public void SendPublicMessage(string channelName, string content)
		{
			PublicChatMessage publicChatMessage = new PublicChatMessage();
			publicChatMessage.Body = content;
			publicChatMessage.SenderId = UserId;
			publicChatMessage.SenderName = Username;
			publicChatMessage.PlayerId = PlayerId;
			publicChatMessage.PlatformId = PlatformId;
			publicChatMessage.SenderColor = UserColor;
			publicChatMessage.Date = DateTime.UtcNow;
			publicChatMessage.BadgeLevel = BadgeLevel;
			publicChatMessage.Platform = Platform;
			publicChatMessage.IsInfo = false;
			ChatStreamContainer chatStreamContainer = new ChatStreamContainer();
			chatStreamContainer.StreamType = ChatStreamType.Public;
			chatStreamContainer.JSONBody = JsonUtility.ToJson(publicChatMessage);
			Client.PublishMessage(channelName, JsonUtility.ToJson(chatStreamContainer));
		}

		public void SendGameInvite(string channelName, string body, CloudRegionCode region, string roomId, string roomName, bool isRace, bool isQuickMatch, bool isCrossplay, List<string> blockedList = null)
		{
			GameInviteMessage gameInviteMessage = new GameInviteMessage();
			gameInviteMessage.Body = body;
			gameInviteMessage.SenderId = UserId;
			gameInviteMessage.SenderName = Username;
			gameInviteMessage.PlayerId = PlayerId;
			gameInviteMessage.PlatformId = PlatformId;
			gameInviteMessage.SenderColor = UserColor;
			gameInviteMessage.Date = DRLTime.serverClock;
			gameInviteMessage.BadgeLevel = BadgeLevel;
			gameInviteMessage.RegionCode = (int)region;
			gameInviteMessage.RoomId = roomId;
			gameInviteMessage.RoomName = roomName;
			gameInviteMessage.IsRace = isRace;
			gameInviteMessage.IsQuickMatch = isQuickMatch;
			gameInviteMessage.IsCrossplay = isCrossplay;
			gameInviteMessage.blockedList = blockedList;
			ChatStreamContainer chatStreamContainer = new ChatStreamContainer();
			chatStreamContainer.StreamType = ChatStreamType.GameInvite;
			chatStreamContainer.JSONBody = JsonUtility.ToJson(gameInviteMessage);
			Client.PublishMessage(channelName, JsonUtility.ToJson(chatStreamContainer));
		}

		public void SendTournamentPullEvent(string p_matchId)
		{
		}

		public void SendInfoMessage(string channelName)
		{
			PublicChatMessage publicChatMessage = new PublicChatMessage();
			publicChatMessage.Body = "Hello world!";
			publicChatMessage.SenderId = UserId;
			publicChatMessage.SenderName = "DRLSIM";
			publicChatMessage.PlayerId = "drl-sim-info-message";
			publicChatMessage.SenderColor = Color.red;
			publicChatMessage.Date = DateTime.UtcNow;
			publicChatMessage.BadgeLevel = 0;
			publicChatMessage.IsInfo = true;
			publicChatMessage.Platform = "info-message";
			if (PublicChannels.ContainsKey("global-chat"))
			{
				ChatRoom chatRoom = PublicChannels["global-chat"];
				if (chatRoom != null)
				{
					chatRoom.Messages.Enqueue(publicChatMessage);
					OnIncomingMessage?.Invoke(chatRoom, publicChatMessage);
				}
			}
		}

		public void SendPrivateMessage(string receiverId, string content, string receiverName, string receiverPhotoURL, Color receiverColor)
		{
			PrivateChatMessage privateChatMessage = new PrivateChatMessage();
			privateChatMessage.Body = content;
			privateChatMessage.SenderId = UserId;
			privateChatMessage.SenderName = Username;
			privateChatMessage.PlayerId = PlayerId;
			privateChatMessage.PlatformId = PlatformId;
			privateChatMessage.SenderColor = UserColor;
			privateChatMessage.ReceiverId = receiverId;
			privateChatMessage.ReceiverName = receiverName;
			privateChatMessage.ReceiverPhotoURL = receiverPhotoURL;
			privateChatMessage.ReceiverColor = receiverColor;
			privateChatMessage.Date = DateTime.UtcNow;
			privateChatMessage.BadgeLevel = BadgeLevel;
			privateChatMessage.Platform = Platform;
			ChatStreamContainer chatStreamContainer = new ChatStreamContainer();
			chatStreamContainer.StreamType = ChatStreamType.Private;
			chatStreamContainer.JSONBody = JsonUtility.ToJson(privateChatMessage);
			Client.SendPrivateMessage(receiverId, JsonUtility.ToJson(chatStreamContainer));
		}

		public void SetOnlineStatus(OnlineStatus status)
		{
			Client.SetOnlineStatus((int)status);
		}

		public void MarkChannelAsRead(string channelId)
		{
			if (!string.IsNullOrEmpty(channelId))
			{
				if (PublicChannels.TryGetValue(channelId, out var value))
				{
					value.MarkAsRead();
				}
				if (PrivateChannels.TryGetValue(channelId, out value))
				{
					value.MarkAsRead();
				}
			}
		}

		public void SubscribeToChannel(string channelId, ChatRoom.Scope scope, int history = -1)
		{
			if (!subscribedChannels.ContainsKey(channelId))
			{
				subscribedChannels.Add(channelId, scope);
				if (IsOnline)
				{
					ChannelCreationOptions channelCreationOptions = new ChannelCreationOptions();
					channelCreationOptions.PublishSubscribers = true;
					channelCreationOptions.MaxSubscribers = 500;
					Client.Subscribe(channelId, 0, history, channelCreationOptions);
				}
			}
			else
			{
				Debug.Log("ChatService > SubscribeToChannel - Already subscribed to " + channelId);
			}
		}

		public void UnsubscribeToChannel(string channelId)
		{
			if (subscribedChannels.ContainsKey(channelId))
			{
				subscribedChannels.Remove(channelId);
				if (IsOnline)
				{
					Client.Unsubscribe(new string[1] { channelId });
				}
			}
		}

		public bool IsSubscribedToChannel(string channelId)
		{
			return subscribedChannels.ContainsKey(channelId);
		}

		private void OnConnected()
		{
			Debug.Log("ChatService > OnConnected - server region: " + Client.ChatRegion);
			PublicChannels.Clear();
			PrivateChannels.Clear();
			subscribedChannels.Clear();
			int history = 20;
			SubscribeToChannel("global-chat", ChatRoom.Scope.Global, history);
			SubscribeToChannel("global-notifications", ChatRoom.Scope.Global);
			SetOnlineStatus(OnlineStatus.Online);
		}

		private void OnDisconnected()
		{
		}

		private void SetServiceState(ServiceState state)
		{
			State = state;
			if (OnStateChanged != null)
			{
				OnStateChanged(State);
			}
			switch (state)
			{
			case ServiceState.Connected:
				OnConnected();
				break;
			case ServiceState.Disconnected:
				OnDisconnected();
				break;
			}
		}

		private void OnSubscribedToChannel(string channelName)
		{
			if (Client.PublicChannels.TryGetValue(channelName, out var value))
			{
				ChatRoom.Scope value2 = ChatRoom.Scope.Custom;
				subscribedChannels.TryGetValue(channelName, out value2);
				ChatRoom value3 = new ChatRoom(value, value2);
				PublicChannels[channelName] = value3;
				if (OnChatRoomJoined != null)
				{
					OnChatRoomJoined(channelName, value2);
				}
			}
			else
			{
				Debug.LogError("ChatService > OnSubscribedToChannel - channel not found with Id: " + channelName);
			}
		}

		private void OnUnsubscribedToChannel(string channelName)
		{
			if (PublicChannels.TryGetValue(channelName, out var value))
			{
				PublicChannels.Remove(channelName);
				if (OnChatRoomLeft != null)
				{
					OnChatRoomLeft(value.ID, value.ScopeType);
				}
			}
			else
			{
				Debug.LogError("ChatService > OnSubscribedToChannel - channel not found with Id: " + channelName);
			}
		}

		private void ProcessIncomingMessage(string channelName, string sender, object message)
		{
			if (channelName == "tournament-notifications")
			{
				OnTournamentNotification?.Invoke(message.ToString());
			}
			else
			{
				if (!Client.TryGetChannel(channelName, out var channel))
				{
					return;
				}
				ChatRoom value = null;
				if (!channel.IsPrivate && !PublicChannels.TryGetValue(channel.Name, out value))
				{
					ChatRoom.Scope value2 = ChatRoom.Scope.Custom;
					subscribedChannels.TryGetValue(channel.Name, out value2);
					value = new ChatRoom(channel, value2);
					PublicChannels.Add(channel.Name, value);
				}
				if (channel.IsPrivate && !PrivateChannels.TryGetValue(channel.Name, out value))
				{
					value = new ChatRoom(channel, ChatRoom.Scope.Private);
					PrivateChannels.Add(channel.Name, value);
				}
				if (value.IsPrivate && UserId != sender)
				{
					value.PrivateSenderId = sender;
				}
				ChatStreamContainer chatStreamContainer = null;
				try
				{
					chatStreamContainer = JsonUtility.FromJson<ChatStreamContainer>(message.ToString());
				}
				catch (Exception)
				{
					return;
				}
				if (chatStreamContainer == null)
				{
					return;
				}
				IChatSendable chatSendable = null;
				switch (chatStreamContainer.StreamType)
				{
				case ChatStreamType.Public:
					chatSendable = JsonUtility.FromJson<PublicChatMessage>(chatStreamContainer.JSONBody);
					break;
				case ChatStreamType.Private:
					chatSendable = JsonUtility.FromJson<PrivateChatMessage>(chatStreamContainer.JSONBody);
					break;
				case ChatStreamType.GameInvite:
					chatSendable = JsonUtility.FromJson<GameInviteMessage>(chatStreamContainer.JSONBody);
					break;
				default:
					Debug.LogError("ChatRoom > AddMessage - stream type not configured for: " + chatStreamContainer.StreamType);
					return;
				}
				if (chatSendable == null)
				{
					Debug.LogError("ChatService > ProcessIncomingMessage - There was an error serializing a message for channel " + channelName);
					return;
				}
				chatSendable.SenderId = sender;
				chatSendable.IsMine = sender == UserId;
				if (channelName == "global-notifications" && chatSendable.Date < sessionDateTime)
				{
					return;
				}
				value.AddMessage(chatSendable);
				if (value != null && OnIncomingMessage != null)
				{
					OnIncomingMessage(value, chatSendable);
				}
				if (Application.isEditor && value != null)
				{
					switch (channelName)
					{
					case "global-chat":
						chatServiceData.Global.AddMessage(chatSendable);
						break;
					case "global-notifications":
						chatServiceData.Notifications.AddMessage(chatSendable);
						break;
					}
				}
			}
		}

		private void OnUserStatusChanged(string user, int status, bool gotMessage, object message)
		{
			Debug.Log("ChatService > OnUserStatusChanged - user[" + user + "] status[" + status + "] gotMessage[" + gotMessage + "] message[" + message?.ToString() + "]");
		}

		private void KeepConnection()
		{
			if (autoConnect && State == ServiceState.Disconnected)
			{
				autoConnectTimer += Time.deltaTime;
				if (autoConnectTimer > 3f)
				{
					Debug.Log("ChatService > KeepConnection - Trying to reconnect to Chat server");
					autoConnectTimer = 0f;
					Connect(m_userId, Username);
				}
			}
		}

		private void OnApplicationQuit()
		{
			if (Client != null)
			{
				Client.Disconnect();
				Client.StopThread();
			}
		}

		private void Update()
		{
			if (Client == null)
			{
				return;
			}
			Client.Service();
			KeepConnection();
			if (Application.isEditor)
			{
				if (PublicChannels.ContainsKey("global-chat"))
				{
					chatServiceData.Global.Set(PublicChannels["global-chat"]);
				}
				if (PublicChannels.ContainsKey("global-notifications"))
				{
					chatServiceData.Notifications.Set(PublicChannels["global-notifications"]);
				}
			}
		}
	}
}
