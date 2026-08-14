using System;
using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;

namespace drl.chat
{
	public class ChatServiceListener : IChatClientListener
	{
		public Action<ChatService.ServiceState> OnStateChanged;

		public Action<string, string, object> OnIncomingMessage;

		public Action<string> OnChannelSubscribed;

		public Action<string> OnChannelUnsubscribed;

		public Action<string, int, bool, object> OnUserStatusChanged;

		public Action<string, string> OnUserSubscribedToChannel;

		public Action<string, string> OnUserUnsubscribedToChannel;

		public void DebugReturn(DebugLevel level, string message)
		{
		}

		public void OnChatStateChange(ChatState state)
		{
			if (OnStateChanged != null)
			{
				switch (state)
				{
				case ChatState.ConnectedToFrontEnd:
					OnStateChanged(ChatService.ServiceState.Connected);
					break;
				case ChatState.Uninitialized:
				case ChatState.Disconnected:
					OnStateChanged(ChatService.ServiceState.Disconnected);
					break;
				default:
					OnStateChanged(ChatService.ServiceState.InProgress);
					break;
				}
			}
		}

		public void OnConnected()
		{
		}

		public void OnDisconnected()
		{
		}

		public void OnGetMessages(string channelName, string[] senders, object[] messages)
		{
			if (OnIncomingMessage != null)
			{
				for (int i = 0; i < senders.Length; i++)
				{
					OnIncomingMessage(channelName, senders[i], messages[i].ToString());
				}
			}
		}

		public void OnPrivateMessage(string sender, object message, string channelName)
		{
			if (OnIncomingMessage != null)
			{
				OnIncomingMessage(channelName, sender, message.ToString());
			}
		}

		public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
		{
			if (OnUserStatusChanged != null)
			{
				OnUserStatusChanged(user, status, gotMessage, message);
			}
		}

		public void OnSubscribed(string[] channels, bool[] results)
		{
			if (OnChannelSubscribed != null)
			{
				for (int i = 0; i < channels.Length; i++)
				{
					OnChannelSubscribed(channels[i]);
					Debug.Log("ChatServiceListener > OnSubscribed - Channel: " + channels[i] + " result: " + results[i]);
				}
			}
		}

		public void OnUnsubscribed(string[] channels)
		{
			if (OnChannelUnsubscribed != null)
			{
				for (int i = 0; i < channels.Length; i++)
				{
					OnChannelUnsubscribed(channels[i]);
					Debug.Log("ChatServiceListener > OnUnsubscribed - Channel: [" + channels[i] + "]");
				}
			}
		}

		public void OnUserSubscribed(string channel, string user)
		{
			if (OnUserSubscribedToChannel != null)
			{
				OnUserSubscribedToChannel(channel, user);
			}
		}

		public void OnUserUnsubscribed(string channel, string user)
		{
			if (OnUserUnsubscribedToChannel != null)
			{
				OnUserUnsubscribedToChannel(channel, user);
			}
		}
	}
}
