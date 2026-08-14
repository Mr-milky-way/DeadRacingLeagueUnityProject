using System;

namespace drl.chat
{
	[Serializable]
	public class ChatStreamContainer
	{
		public ChatStreamType StreamType;

		public string JSONBody;
	}
}
