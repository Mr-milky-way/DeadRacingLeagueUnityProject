using System;
using UnityEngine;

namespace drl.chat
{
	[Serializable]
	public abstract class ChatMessage : IChatSendable, ISerializationCallbackReceiver
	{
		[SerializeField]
		private string senderId = "";

		[SerializeField]
		private string senderName = "";

		[SerializeField]
		private string playerId = "";

		[SerializeField]
		private string platformId = "";

		[SerializeField]
		private Color senderColor = Color.white;

		[SerializeField]
		private string body = "";

		[SerializeField]
		private long dateTicks;

		[SerializeField]
		private int badgeLevel;

		[SerializeField]
		private string platform = "";

		public bool IsValidated;

		public bool IsBlocked;

		public int BadgeLevel
		{
			get
			{
				return badgeLevel;
			}
			set
			{
				badgeLevel = value;
			}
		}

		public string Platform
		{
			get
			{
				return platform;
			}
			set
			{
				platform = value;
			}
		}

		public string SenderId
		{
			get
			{
				return senderId;
			}
			set
			{
				senderId = value;
			}
		}

		public string SenderName
		{
			get
			{
				return senderName;
			}
			set
			{
				senderName = value;
			}
		}

		public string PlayerId
		{
			get
			{
				return playerId;
			}
			set
			{
				playerId = value;
			}
		}

		public string PlatformId
		{
			get
			{
				return platformId;
			}
			set
			{
				platformId = value;
			}
		}

		public string Body
		{
			get
			{
				return body;
			}
			set
			{
				body = value;
			}
		}

		public Color SenderColor
		{
			get
			{
				return senderColor;
			}
			set
			{
				senderColor = value;
			}
		}

		public bool IsMine { get; set; }

		public DateTime Date { get; set; }

		public bool IsInfo { get; set; }

		public abstract ChatStreamType MessageType { get; }

		public void OnAfterDeserialize()
		{
			Date = new DateTime(dateTicks);
		}

		public void OnBeforeSerialize()
		{
			dateTicks = Date.Ticks;
		}
	}
}
