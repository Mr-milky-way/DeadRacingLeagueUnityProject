using System;
using UnityEngine;

namespace drl.chat
{
	public interface IChatSendable
	{
		string SenderId { get; set; }

		string SenderName { get; set; }

		string PlayerId { get; set; }

		string Body { get; set; }

		Color SenderColor { get; set; }

		ChatStreamType MessageType { get; }

		bool IsMine { get; set; }

		DateTime Date { get; set; }

		int BadgeLevel { get; set; }

		bool IsInfo { get; set; }
	}
}
