using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	public class NetworkRoomChat
	{
		public class Message
		{
			public int SenderId;

			public string SenderName;

			public string PlayerId;

			public Color SenderColor;

			public string Content;

			public DateTime Date;

			public bool IsMine;

			public bool IsValidated;

			public bool IsBlocked;

			public Message(Hashtable data = null)
			{
				SenderName = "Unknown";
				PlayerId = "";
				SenderColor = Color.white;
				Content = "";
				Date = DateTime.UtcNow;
				if (data != null)
				{
					UpdateData(data);
				}
			}

			public Hashtable ToHashTable()
			{
				return new Hashtable
				{
					{ "sender", SenderId },
					{
						"name",
						SenderName ?? ""
					},
					{
						"player-id",
						PlayerId ?? ""
					},
					{
						"color",
						(int)Colorf.ColorToRGB(SenderColor)
					},
					{
						"content",
						Content ?? ""
					},
					{ "date", Date.Ticks }
				};
			}

			public void UpdateData(Hashtable data)
			{
				SenderId = (int)data["sender"];
				SenderName = (string)data["name"];
				PlayerId = (string)data["player-id"];
				SenderColor = Colorf.RGBToColor((uint)(int)data["color"]);
				Content = (string)data["content"];
				Date = new DateTime((long)data["date"], DateTimeKind.Utc);
			}
		}

		private DateTime lastReadTime;

		private NetworkRoom room;

		public Queue<Message> History { get; private set; }

		public bool HasUnreadMessages => lastReadTime < LastMessageTime;

		public DateTime LastMessageTime { get; private set; }

		public NetworkRoomChat(NetworkRoom parentRoom)
		{
			room = parentRoom;
			LastMessageTime = DateTime.UtcNow;
			lastReadTime = DateTime.UtcNow;
			History = new Queue<Message>();
		}

		public Message AddMessage(Hashtable data)
		{
			Message message = new Message(data);
			message.IsMine = message.SenderId == room.Local.ID;
			if (History.Count >= 5)
			{
				History.Dequeue();
			}
			History.Enqueue(message);
			LastMessageTime = message.Date;
			return message;
		}

		public void MarkAsRead()
		{
			lastReadTime = DateTime.UtcNow;
		}

		public void SendChatMessage(string messageContent)
		{
			Message message = new Message();
			message.Content = messageContent;
			message.SenderId = room.Local.ID;
			message.SenderName = room.Local.ProfileName;
			message.PlayerId = room.Local.PlayerId;
			message.SenderColor = room.Local.MainColor;
			room.Outgoing.SendChatMessage(message);
		}
	}
}
