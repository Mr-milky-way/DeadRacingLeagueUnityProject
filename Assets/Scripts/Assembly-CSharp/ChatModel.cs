using System;
using System.Collections.Generic;
using UnityEngine;
using drl.chat;
using drl.game;
using thelab.core;
using thelab.mvc;

public class ChatModel : Model<DRLApp>
{
	public List<UIChatMessage> messages = new List<UIChatMessage>();

	[HideInInspector]
	public string LastMessage = "";

	public ChatService service => AssertLocal<ChatService>("service");

	public ChatService.ServiceState State
	{
		get
		{
			if (!(service == null))
			{
				return service.State;
			}
			return ChatService.ServiceState.Disconnected;
		}
	}

	public Dictionary<string, ChatRoom> PublicChannels
	{
		get
		{
			if (!(service != null))
			{
				return new Dictionary<string, ChatRoom>();
			}
			return service.PublicChannels;
		}
	}

	public Dictionary<string, ChatRoom> PrivateChannels
	{
		get
		{
			if (!(service != null))
			{
				return new Dictionary<string, ChatRoom>();
			}
			return service.PrivateChannels;
		}
	}

	public bool IsTournamentChatAvailable => GetPublicChannelByScope(ChatRoom.Scope.Tournament) != null;

	public bool ArePrivateChannelsAvailable => PrivateChannels.Count > 0;

	public ChatRoom GlobalChat
	{
		get
		{
			if (PublicChannels == null || !PublicChannels.ContainsKey("global-chat"))
			{
				return null;
			}
			return PublicChannels["global-chat"];
		}
	}

	public int OnlinePlayers
	{
		get
		{
			if (GlobalChat == null)
			{
				return 0;
			}
			return GlobalChat.OnlinePlayers;
		}
	}

	private void Start()
	{
		ChatService chatService = service;
		chatService.OnStateChanged = (Action<ChatService.ServiceState>)Delegate.Remove(chatService.OnStateChanged, new Action<ChatService.ServiceState>(OnStateChanged));
		ChatService chatService2 = service;
		chatService2.OnStateChanged = (Action<ChatService.ServiceState>)Delegate.Combine(chatService2.OnStateChanged, new Action<ChatService.ServiceState>(OnStateChanged));
		ChatService chatService3 = service;
		chatService3.OnIncomingMessage = (Action<ChatRoom, IChatSendable>)Delegate.Remove(chatService3.OnIncomingMessage, new Action<ChatRoom, IChatSendable>(OnIncommingMessage));
		ChatService chatService4 = service;
		chatService4.OnIncomingMessage = (Action<ChatRoom, IChatSendable>)Delegate.Combine(chatService4.OnIncomingMessage, new Action<ChatRoom, IChatSendable>(OnIncommingMessage));
		ChatService chatService5 = service;
		chatService5.OnChatRoomJoined = (Action<string, ChatRoom.Scope>)Delegate.Remove(chatService5.OnChatRoomJoined, new Action<string, ChatRoom.Scope>(OnChatRoomJoined));
		ChatService chatService6 = service;
		chatService6.OnChatRoomJoined = (Action<string, ChatRoom.Scope>)Delegate.Combine(chatService6.OnChatRoomJoined, new Action<string, ChatRoom.Scope>(OnChatRoomJoined));
		ChatService chatService7 = service;
		chatService7.OnChatRoomLeft = (Action<string, ChatRoom.Scope>)Delegate.Remove(chatService7.OnChatRoomLeft, new Action<string, ChatRoom.Scope>(OnChatRoomLeft));
		ChatService chatService8 = service;
		chatService8.OnChatRoomLeft = (Action<string, ChatRoom.Scope>)Delegate.Combine(chatService8.OnChatRoomLeft, new Action<string, ChatRoom.Scope>(OnChatRoomLeft));
		ChatService chatService9 = service;
		chatService9.OnPlayerJoinedChat = (Action<string, string>)Delegate.Remove(chatService9.OnPlayerJoinedChat, new Action<string, string>(OnPlayerJoinedChat));
		ChatService chatService10 = service;
		chatService10.OnPlayerJoinedChat = (Action<string, string>)Delegate.Combine(chatService10.OnPlayerJoinedChat, new Action<string, string>(OnPlayerJoinedChat));
		ChatService chatService11 = service;
		chatService11.OnPlayerLeftChat = (Action<string, string>)Delegate.Remove(chatService11.OnPlayerLeftChat, new Action<string, string>(OnPlayerLeftChat));
		ChatService chatService12 = service;
		chatService12.OnPlayerLeftChat = (Action<string, string>)Delegate.Combine(chatService12.OnPlayerLeftChat, new Action<string, string>(OnPlayerLeftChat));
		ChatService chatService13 = service;
		chatService13.OnTournamentNotification = (Action<string>)Delegate.Remove(chatService13.OnTournamentNotification, new Action<string>(OnTournamentPullEvent));
		ChatService chatService14 = service;
		chatService14.OnTournamentNotification = (Action<string>)Delegate.Combine(chatService14.OnTournamentNotification, new Action<string>(OnTournamentPullEvent));
	}

	private void OnStateChanged(ChatService.ServiceState state)
	{
		switch (state)
		{
		case ChatService.ServiceState.Connected:
			Notify("chat.server.connected");
			break;
		case ChatService.ServiceState.Disconnected:
			Notify("chat.server.disconnected");
			break;
		case ChatService.ServiceState.InProgress:
			Notify("chat.server.connecting");
			break;
		}
	}

	private void OnIncommingMessage(ChatRoom room, IChatSendable message)
	{
		switch (message.MessageType)
		{
		case ChatStreamType.Public:
			Notify("chat.incoming.public", room, message);
			break;
		case ChatStreamType.Private:
			Notify("chat.incoming.private", room, message);
			break;
		case ChatStreamType.GameInvite:
			Notify("chat.incoming.invite", room, message);
			break;
		}
	}

	private void OnChatRoomJoined(string chatRoomName, ChatRoom.Scope scope)
	{
		Notify("chat.room.joined", chatRoomName, scope);
	}

	private void OnChatRoomLeft(string chatRoomName, ChatRoom.Scope scope)
	{
		Notify("chat.room.left", chatRoomName, scope);
	}

	private void OnPlayerLeftChat(string channnel, string user)
	{
		Notify("chat.channnel.player.left", channnel, user);
	}

	private void OnPlayerJoinedChat(string channnel, string user)
	{
		Notify("chat.channnel.player.joined", channnel, user);
	}

	public void TryConnect()
	{
		if (!service.IsOnline)
		{
			PlayerStateModel player = base.app.model.storage.state.player;
			UpdateUserData(player);
			service.Connect(player.profile.platformId, player.profile.username);
		}
	}

	public void Disconnect()
	{
		service.Disconnect();
	}

	public void UpdateUserData(PlayerStateModel data)
	{
		service.PlayerId = data.profile.playerId;
		service.PlatformId = data.profile.platformId;
		service.UserColor = data.profile.color;
		service.BadgeLevel = data.userRank;
		service.Platform = OS.prefix;
	}

	public void SendPublicMessage(string channelID, string messageContent)
	{
		if (string.IsNullOrEmpty(channelID))
		{
			Debug.LogError("ChatModel > SendPublicMessage - ChannelID was null or empty.");
		}
		else
		{
			service.SendPublicMessage(channelID, messageContent);
		}
	}

	public void SendGlobalMessage(string messageContent)
	{
		SendPublicMessage("global-chat", messageContent);
	}

	public void SendGameInvite(CloudRegionCode region, string roomId, string roomName, bool isRace, string inviteBody, bool isQuickMatch, bool isCrossplay)
	{
		service.SendGameInvite("global-notifications", inviteBody, region, roomId, roomName, isRace, isQuickMatch, isCrossplay);
	}

	public void SendTournamentPullEvent(string p_tournamentMatchId)
	{
		if (!string.IsNullOrEmpty(p_tournamentMatchId))
		{
			service.SendTournamentPullEvent(p_tournamentMatchId);
		}
	}

	public void SendInfoMessage()
	{
		if (GlobalChat != null)
		{
			IChatSendable[] array = GlobalChat.Messages.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsInfo)
				{
					return;
				}
			}
		}
		service.SendInfoMessage("global-chat");
	}

	public void SendMessageToChannelWithScope(string messageContent, ChatRoom.Scope scope)
	{
		ChatRoom publicChannelByScope = GetPublicChannelByScope(scope);
		if (publicChannelByScope != null)
		{
			SendPublicMessage(publicChannelByScope.ID, messageContent);
		}
	}

	public void SendPrivateMessage(string receiverId, string body, string receiverName, string receiverPhotoURL, Color receiverColor)
	{
		if (string.IsNullOrEmpty(receiverId))
		{
			Debug.LogError("ChatModel > SendPrivateMessage - Recipient user ID was null or empty.");
		}
		else
		{
			service.SendPrivateMessage(receiverId, body, receiverName, receiverPhotoURL, receiverColor);
		}
	}

	public void MarkChannelAsRead(string channelId)
	{
		if (!string.IsNullOrEmpty(channelId))
		{
			service.MarkChannelAsRead(channelId);
			Notify("social.badges.clear", channelId);
		}
	}

	public bool IsChannelUnread(string channelId)
	{
		bool result = false;
		if (PublicChannels.TryGetValue(channelId, out var value))
		{
			result = value.HasUnreadMessages;
		}
		if (PrivateChannels.TryGetValue(channelId, out value))
		{
			result = value.HasUnreadMessages;
		}
		return result;
	}

	public ChatRoom TryGetChannel(string channelId)
	{
		if (PublicChannels.TryGetValue(channelId, out var value))
		{
			return value;
		}
		PrivateChannels.TryGetValue(channelId, out value);
		return value;
	}

	public string GetPrivateChannelNameByUser(string remoteUserSteamId)
	{
		return service.Client.GetPrivateChannelNameByUser(remoteUserSteamId);
	}

	public void SubscribeToTournamentChannel(string tournamentId)
	{
		if (!string.IsNullOrEmpty(tournamentId))
		{
			service.SubscribeToChannel(tournamentId, ChatRoom.Scope.Tournament, 20);
		}
	}

	public void UnsubscribeToTournamentChannel()
	{
		ChatRoom publicChannelByScope = GetPublicChannelByScope(ChatRoom.Scope.Tournament);
		if (publicChannelByScope != null)
		{
			service.UnsubscribeToChannel(publicChannelByScope.ID);
		}
	}

	public ChatRoom GetPublicChannelByScope(ChatRoom.Scope scope)
	{
		return new List<ChatRoom>(service.PublicChannels.Values)?.Find((ChatRoom chatRoom) => chatRoom.ScopeType == scope);
	}

	public List<ChatRoom> GetAllPublicChannelsByScope(ChatRoom.Scope scope)
	{
		List<ChatRoom> list = new List<ChatRoom>(service.PublicChannels.Values);
		if (list == null)
		{
			return new List<ChatRoom>();
		}
		return list.FindAll((ChatRoom chatRoom) => chatRoom.ScopeType == scope);
	}

	private void OnTournamentPullEvent(string p_matchId)
	{
		if (!string.IsNullOrEmpty(p_matchId))
		{
			Debug.Log("ChatModel> Recieved tournament pull event for match: " + p_matchId);
			Notify("tournament.action.match-starting", p_matchId);
		}
	}

	public void OnPersistency()
	{
		base.app.model.chat = this;
		service.Init();
	}
}
