using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.network;

namespace drl.chat
{
	[RequireComponent(typeof(ChatService))]
	public class ChatDebug : MonoBehaviour
	{
		[SerializeField]
		private string userId = "";

		private static int chatOptions;

		private static string newChatMessage = "";

		private static string unknownReceiverId = "";

		private static string receiverId = "";

		private static Vector2 scrollPosition;

		private ChatService service;

		private void Awake()
		{
			service = GetComponent<ChatService>();
			if (service == null)
			{
				Debug.LogError("ChatDebug can't run without a ChatService attached to the same GameObject");
			}
		}

		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2 - 400, 0f, 800f, Screen.height));
			GUILayout.BeginVertical();
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(800f), GUILayout.Height(400f));
			GUILayout.Space(50f);
			chatOptions = GUI.Toolbar(new Rect(0f, 100f, 800f, 30f), chatOptions, new string[3] { "Status", "Global", "Private" });
			GUILayout.Space(80f);
			switch (chatOptions)
			{
			case 0:
				GUILayout.Label("Status: " + service.State);
				GUILayout.Label("UserId: " + service.UserId);
				if (service.IsOnline)
				{
					userId = service.UserId;
					if (GUILayout.Button("Disconnect"))
					{
						service.Disconnect();
					}
				}
				if (service.State == ChatService.ServiceState.Disconnected && GUILayout.Button("Connect"))
				{
					service.Connect(SystemInfo.deviceUniqueIdentifier, SystemInfo.deviceName, keepConnectionAlive: false);
				}
				break;
			case 1:
			{
				if (!service.PublicChannels.TryGetValue("global-chat", out var value2))
				{
					break;
				}
				Queue<IChatSendable> messages = value2.Messages;
				GUILayout.Label("Chat messages: " + messages.Count);
				GUILayout.BeginVertical("", GUI.skin.window);
				foreach (IChatSendable item in messages)
				{
					GUILayout.BeginHorizontal("", GUI.skin.box);
					GUILayout.Label(string.Format("{0} : {1} at {2}", item.IsMine ? "Me:" : item.SenderId, item.Body, PhotonUtils.TimeAgo(item.Date)));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.BeginHorizontal();
				newChatMessage = GUILayout.TextField(newChatMessage);
				if (GUILayout.Button("Send Message"))
				{
					service.SendPublicMessage("global-chat", newChatMessage);
					newChatMessage = "";
				}
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Mark as Read"))
				{
					service.MarkChannelAsRead("global-chat");
				}
				break;
			}
			case 2:
			{
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Receiver Id: ");
				if (string.IsNullOrEmpty(receiverId))
				{
					unknownReceiverId = GUILayout.TextField(unknownReceiverId, GUILayout.Width(200f));
				}
				else
				{
					GUILayout.Label(receiverId);
					unknownReceiverId = receiverId;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Message: ");
				newChatMessage = GUILayout.TextField(newChatMessage, GUILayout.Width(300f));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Send Message"))
				{
					service.SendPrivateMessage(unknownReceiverId, newChatMessage, "", "", Color.white);
					newChatMessage = "";
				}
				GUILayout.EndVertical();
				if (service.PrivateChannels.Count <= 0)
				{
					break;
				}
				ChatRoom value = service.PrivateChannels.First().Value;
				receiverId = value.ID.Split(':')[1];
				GUILayout.Label("Chat messages: " + value.Messages.Count);
				GUILayout.BeginVertical("", GUI.skin.window);
				foreach (PrivateChatMessage message in value.Messages)
				{
					GUILayout.BeginHorizontal("", GUI.skin.box);
					GUILayout.Label(string.Format("{0} : {1} at {2}", message.IsMine ? "Me:" : message.SenderId, message.Body, PhotonUtils.TimeAgo(message.Date)));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				break;
			}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
