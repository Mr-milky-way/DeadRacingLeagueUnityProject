using System;
using System.Collections.Generic;

namespace drl.chat
{
	[Serializable]
	public class GameInviteMessage : ChatMessage
	{
		public int RegionCode;

		public string RoomId;

		public string RoomName;

		public bool IsQuickMatch;

		public bool IsRace;

		public bool IsCrossplay;

		public List<string> blockedList;

		public override ChatStreamType MessageType => ChatStreamType.GameInvite;
	}
}
