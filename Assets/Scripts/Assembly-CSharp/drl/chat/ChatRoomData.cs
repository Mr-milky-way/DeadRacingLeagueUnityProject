using System;
using System.Collections.Generic;

namespace drl.chat
{
	[Serializable]
	public class ChatRoomData
	{
		public string Id;

		public int OnlinePlayers;

		public List<PublicChatMessage> PublicMessages = new List<PublicChatMessage>();

		public List<PrivateChatMessage> PrivateMessages = new List<PrivateChatMessage>();

		public List<GameInviteMessage> GameInvites = new List<GameInviteMessage>();

		public void Set(ChatRoom data)
		{
			if (data != null)
			{
				Id = data.ID;
				OnlinePlayers = data.OnlinePlayers;
			}
		}

		public void AddMessage(IChatSendable incomingMessage)
		{
			switch (incomingMessage.MessageType)
			{
			case ChatStreamType.Public:
				PublicMessages.Add((PublicChatMessage)incomingMessage);
				break;
			case ChatStreamType.Private:
				PrivateMessages.Add((PrivateChatMessage)incomingMessage);
				break;
			case ChatStreamType.GameInvite:
				GameInvites.Add((GameInviteMessage)incomingMessage);
				break;
			}
		}
	}
}
