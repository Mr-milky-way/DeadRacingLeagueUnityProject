using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
[DisallowMultipleComponent]
public class AkRoomAwareObject : MonoBehaviour
{
	private static readonly Dictionary<Collider, AkRoomAwareObject> ColliderToRoomAwareObjectMap = new Dictionary<Collider, AkRoomAwareObject>();

	public Collider m_Collider;

	private readonly AkRoom.PriorityList roomPriorityList = new AkRoom.PriorityList();

	public static AkRoomAwareObject GetAkRoomAwareObjectFromCollider(Collider collider)
	{
		AkRoomAwareObject value = null;
		if (!ColliderToRoomAwareObjectMap.TryGetValue(collider, out value))
		{
			return null;
		}
		return value;
	}

	private void Awake()
	{
		m_Collider = GetComponent<Collider>();
		if (m_Collider != null)
		{
			ColliderToRoomAwareObjectMap.Add(m_Collider, this);
		}
	}

	private void OnEnable()
	{
		AkRoomAwareManager.RegisterRoomAwareObject(this);
		for (int i = 0; i < roomPriorityList.Count; i++)
		{
			roomPriorityList[i].TryEnter(this);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < roomPriorityList.Count; i++)
		{
			roomPriorityList[i].Exit(this);
		}
		AkRoomAwareManager.UnregisterRoomAwareObject(this);
		SetGameObjectInRoom(null);
	}

	private void OnDestroy()
	{
		ColliderToRoomAwareObjectMap.Remove(m_Collider);
	}

	public void SetGameObjectInHighestPriorityActiveAndEnabledRoom()
	{
		SetGameObjectInRoom(roomPriorityList.GetHighestPriorityActiveAndEnabledRoom());
	}

	private void SetGameObjectInRoom(AkRoom room)
	{
		AkSoundEngine.SetGameObjectInRoom(base.gameObject, (room == null) ? AkRoom.INVALID_ROOM_ID : room.GetID());
	}

	public void EnteredRoom(AkRoom room)
	{
		roomPriorityList.Add(room);
	}

	public void ExitedRoom(AkRoom room)
	{
		roomPriorityList.Remove(room);
	}
}
