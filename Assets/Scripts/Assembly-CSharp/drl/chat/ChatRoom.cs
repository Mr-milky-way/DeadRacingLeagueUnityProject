using System;
using System.Collections.Generic;
using Photon.Chat;
using UnityEngine;

namespace drl.chat
{
	public class ChatRoom
	{
		public enum Scope
		{
			Global = 0,
			Tournament = 1,
			Clan = 2,
			Private = 3,
			Custom = 4
		}

		public const string GLOBAL = "global-chat";

		public const string NOTIFICATIONS = "global-notifications";

		public const string PRIVATE = "private-chat";

		public const string ROOM = "room-chat";

		public const string TOURNAMENT = "tournament-chat";

		public const string CHANNEL_MODIFIER = "";

		public const string TOURNAMENT_NOTIFICATIONS = "tournament-notifications";

		private ChatChannel channelData;

		private static Dictionary<string, string> m_time_key_prefs = new Dictionary<string, string>();

		public string ID { get; private set; }

		public int OnlinePlayers => channelData.Subscribers.Count;

		public Queue<IChatSendable> Messages { get; private set; }

		public Scope ScopeType { get; private set; }

		public bool IsPrivate
		{
			get
			{
				if (channelData != null)
				{
					return channelData.IsPrivate;
				}
				return false;
			}
		}

		public string PrivateSenderId { get; set; }

		public DateTime LastMessageTime { get; private set; }

		public DateTime LastReadTime { get; set; }

		public bool HasUnreadMessages => LastReadTime < LastMessageTime;

		public HashSet<string> Users
		{
			get
			{
				if (channelData != null)
				{
					return channelData.Subscribers;
				}
				return new HashSet<string>();
			}
		}

		private string ReadTimeKey => ID + "_time-read";

		public ChatRoom(ChatChannel channel, Scope scope)
		{
			channelData = channel;
			ID = channel.Name;
			ScopeType = scope;
			Messages = new Queue<IChatSendable>();
			PrivateSenderId = "";
			LastMessageTime = DateTime.UtcNow;
			LastReadTime = DateTime.UtcNow;
			if (PlayerPrefs.HasKey(ReadTimeKey))
			{
				string s = PlayerPrefs.GetString(ReadTimeKey);
				DateTime result = DateTime.UtcNow;
				if (DateTime.TryParse(s, out result))
				{
					LastReadTime = result;
					LastMessageTime = LastReadTime;
				}
			}
			Debug.Log($"ChatRoom > Joined channel id[{channel.Name}] scope[{scope}]");
		}

		public void MarkAsRead()
		{
			LastReadTime = DateTime.UtcNow;
			PlayerPrefs.SetString(ReadTimeKey, LastReadTime.ToString());
			PlayerPrefs.Save();
		}

		public void AddMessage(IChatSendable message)
		{
			if (Messages.Count >= 20)
			{
				Messages.Dequeue();
			}
			Messages.Enqueue(message);
			LastMessageTime = message.Date;
		}
	}
}
