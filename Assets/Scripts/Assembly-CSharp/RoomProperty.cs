using UnityEngine;

[CreateAssetMenu(fileName = "New RoomProperty", menuName = "ScriptableObjects/Network/RoomProperty")]
public class RoomProperty : BaseProperty<NetworkRoomData>
{
	protected override void SetDefault()
	{
		m_Value = new NetworkRoomData();
	}
}
