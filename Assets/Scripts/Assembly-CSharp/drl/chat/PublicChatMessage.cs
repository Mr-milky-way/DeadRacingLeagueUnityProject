using System;

namespace drl.chat
{
	[Serializable]
	public class PublicChatMessage : ChatMessage
	{
		public override ChatStreamType MessageType => ChatStreamType.Public;
	}
}
