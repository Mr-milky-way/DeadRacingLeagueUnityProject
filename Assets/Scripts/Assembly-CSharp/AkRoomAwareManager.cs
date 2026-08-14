using System.Collections.Generic;
using UnityEngine;

public static class AkRoomAwareManager
{
	private static readonly HashSet<AkRoomAwareObject> m_RoomAwareObjects = new HashSet<AkRoomAwareObject>();

	private static readonly HashSet<AkRoomAwareObject> m_RoomAwareObjectToUpdate = new HashSet<AkRoomAwareObject>();

	public static void RegisterRoomAwareObject(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjects.Add(roomAwareObject);
		RegisterRoomAwareObjectForUpdate(roomAwareObject);
	}

	public static void UnregisterRoomAwareObject(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjects.Remove(roomAwareObject);
		m_RoomAwareObjectToUpdate.Remove(roomAwareObject);
	}

	public static void RegisterRoomAwareObjectForUpdate(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjectToUpdate.Add(roomAwareObject);
	}

	public static void ObjectEnteredRoom(Collider collider, AkRoom room)
	{
		if ((bool)collider)
		{
			ObjectEnteredRoom(AkRoomAwareObject.GetAkRoomAwareObjectFromCollider(collider), room);
		}
	}

	public static void ObjectEnteredRoom(AkRoomAwareObject roomAwareObject, AkRoom room)
	{
		if ((bool)roomAwareObject && (bool)room && room.TryEnter(roomAwareObject))
		{
			roomAwareObject.EnteredRoom(room);
			RegisterRoomAwareObjectForUpdate(roomAwareObject);
		}
	}

	public static void ObjectExitedRoom(Collider collider, AkRoom room)
	{
		if ((bool)collider)
		{
			ObjectExitedRoom(AkRoomAwareObject.GetAkRoomAwareObjectFromCollider(collider), room);
		}
	}

	public static void ObjectExitedRoom(AkRoomAwareObject roomAwareObject, AkRoom room)
	{
		if ((bool)roomAwareObject && (bool)room)
		{
			room.Exit(roomAwareObject);
			roomAwareObject.ExitedRoom(room);
			RegisterRoomAwareObjectForUpdate(roomAwareObject);
		}
	}

	public static void UpdateRoomAwareObjects()
	{
		foreach (AkRoomAwareObject item in m_RoomAwareObjectToUpdate)
		{
			if (m_RoomAwareObjects.Contains(item))
			{
				item.SetGameObjectInHighestPriorityActiveAndEnabledRoom();
			}
		}
		m_RoomAwareObjectToUpdate.Clear();
	}
}
