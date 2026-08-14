using System;
using UnityEngine;

namespace drl.chat
{
	[Serializable]
	public class PrivateChatMessage : ChatMessage
	{
		public string ReceiverId;

		public string ReceiverName;

		public string ReceiverPhotoURL;

		public Color ReceiverColor = Color.white;

		public override ChatStreamType MessageType => ChatStreamType.Private;
	}
}
