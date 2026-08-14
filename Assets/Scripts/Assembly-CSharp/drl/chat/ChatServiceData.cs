using UnityEngine;

namespace drl.chat
{
	[CreateAssetMenu(fileName = "New ChatServiceData", menuName = "ScriptableObjects/Network/ChatServiceData")]
	public class ChatServiceData : ScriptableObject, ISerializationCallbackReceiver
	{
		public ChatRoomData Global = new ChatRoomData();

		public ChatRoomData Notifications = new ChatRoomData();

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Global = new ChatRoomData();
			Notifications = new ChatRoomData();
		}
	}
}
