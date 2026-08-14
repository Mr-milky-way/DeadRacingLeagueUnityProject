using System;
using UnityEngine;

namespace drl.game
{
	public struct UIChatMessage
	{
		public string messageId;

		public string platformId;

		public Color userColor;

		public string name;

		public string text;

		public DateTime msgTime;

		public bool left;

		public bool isFriend;

		public bool isPrivate;

		public int rank;

		public string playerId;

		public string channel;

		public bool isInfo;

		public string platform;

		public string senderId;
	}
}
