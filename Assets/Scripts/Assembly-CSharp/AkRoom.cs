using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

[AddComponentMenu("Wwise/AkRoom")]
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class AkRoom : AkTriggerHandler
{
	public class PriorityList
	{
		private class CompareByPriority : IComparer<AkRoom>
		{
			public virtual int Compare(AkRoom a, AkRoom b)
			{
				int num = a.priority.CompareTo(b.priority);
				if (num == 0 && a != b)
				{
					return 1;
				}
				return -num;
			}
		}

		private static readonly CompareByPriority s_compareByPriority = new CompareByPriority();

		private List<AkRoom> rooms = new List<AkRoom>();

		public int Count => rooms.Count;

		public AkRoom this[int index] => rooms[index];

		public ulong GetHighestPriorityActiveAndEnabledRoomID()
		{
			AkRoom highestPriorityActiveAndEnabledRoom = GetHighestPriorityActiveAndEnabledRoom();
			if (!(highestPriorityActiveAndEnabledRoom == null))
			{
				return highestPriorityActiveAndEnabledRoom.GetID();
			}
			return INVALID_ROOM_ID;
		}

		public AkRoom GetHighestPriorityActiveAndEnabledRoom()
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].isActiveAndEnabled)
				{
					return rooms[i];
				}
			}
			return null;
		}

		public void Clear()
		{
			rooms.Clear();
		}

		public void Add(AkRoom room)
		{
			int num = BinarySearch(room);
			if (num < 0)
			{
				rooms.Insert(~num, room);
			}
		}

		public void Remove(AkRoom room)
		{
			rooms.Remove(room);
		}

		public bool Contains(AkRoom room)
		{
			if ((bool)room)
			{
				return rooms.Contains(room);
			}
			return false;
		}

		public int BinarySearch(AkRoom room)
		{
			if (!room)
			{
				return -1;
			}
			return rooms.BinarySearch(room, s_compareByPriority);
		}
	}

	public static ulong INVALID_ROOM_ID;

	[Tooltip("Higher number has a higher priority")]
	public int priority;

	public AuxBus reverbAuxBus = new AuxBus();

	[Range(0f, 1f)]
	public float reverbLevel = 1f;

	[Range(0f, 1f)]
	public float wallOcclusion = 1f;

	public AK.Wwise.Event roomToneEvent = new AK.Wwise.Event();

	[Range(0f, 1f)]
	[Tooltip("Send level for sounds that are posted on the room game object; adds reverb to ambience and room tones. Valid range: (0.f-1.f). A value of 0 disables the aux send.")]
	public float roomToneAuxSend;

	private List<AkRoomAwareObject> roomAwareObjectsEntered = new List<AkRoomAwareObject>();

	private List<AkRoomAwareObject> roomAwareObjectsDetectedWhileDisabled = new List<AkRoomAwareObject>();

	public static int RoomCount { get; private set; }

	public static ulong GetAkRoomID(AkRoom room)
	{
		if (!(room == null))
		{
			return room.GetID();
		}
		return INVALID_ROOM_ID;
	}

	public bool TryEnter(AkRoomAwareObject roomAwareObject)
	{
		if ((bool)roomAwareObject)
		{
			if (base.isActiveAndEnabled)
			{
				if (!roomAwareObjectsEntered.Contains(roomAwareObject))
				{
					roomAwareObjectsEntered.Add(roomAwareObject);
				}
				return true;
			}
			if (!roomAwareObjectsDetectedWhileDisabled.Contains(roomAwareObject))
			{
				roomAwareObjectsDetectedWhileDisabled.Add(roomAwareObject);
			}
			return false;
		}
		return false;
	}

	public void Exit(AkRoomAwareObject roomAwareObject)
	{
		if ((bool)roomAwareObject)
		{
			roomAwareObjectsEntered.Remove(roomAwareObject);
			roomAwareObjectsDetectedWhileDisabled.Remove(roomAwareObject);
		}
	}

	public ulong GetID()
	{
		return AkSoundEngine.GetAkGameObjectID(base.gameObject);
	}

	private void OnEnable()
	{
		AkRoomParams in_roomParams = new AkRoomParams
		{
			Up = base.transform.up,
			Front = base.transform.forward,
			ReverbAuxBus = reverbAuxBus.Id,
			ReverbLevel = reverbLevel,
			WallOcclusion = wallOcclusion,
			RoomGameObj_AuxSendLevelToSelf = roomToneAuxSend,
			RoomGameObj_KeepRegistered = roomToneEvent.IsValid()
		};
		RoomCount++;
		AkSoundEngine.SetRoom(GetID(), in_roomParams, base.name);
		AkRoomManager.RegisterRoomUpdate(this);
		for (int i = 0; i < roomAwareObjectsDetectedWhileDisabled.Count; i++)
		{
			AkRoomAwareManager.ObjectEnteredRoom(roomAwareObjectsDetectedWhileDisabled[i], this);
		}
		roomAwareObjectsDetectedWhileDisabled.Clear();
	}

	private void OnDisable()
	{
		for (int i = 0; i < roomAwareObjectsEntered.Count; i++)
		{
			roomAwareObjectsEntered[i].ExitedRoom(this);
			AkRoomAwareManager.RegisterRoomAwareObjectForUpdate(roomAwareObjectsEntered[i]);
			roomAwareObjectsDetectedWhileDisabled.Add(roomAwareObjectsEntered[i]);
		}
		roomAwareObjectsEntered.Clear();
		AkRoomManager.RegisterRoomUpdate(this);
		RoomCount--;
		AkSoundEngine.RemoveRoom(GetID());
	}

	private void OnTriggerEnter(Collider in_other)
	{
		AkRoomAwareManager.ObjectEnteredRoom(in_other, this);
	}

	private void OnTriggerExit(Collider in_other)
	{
		AkRoomAwareManager.ObjectExitedRoom(in_other, this);
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		if (roomToneEvent.IsValid())
		{
			AkSoundEngine.PostEventOnRoom(roomToneEvent.Id, GetID());
		}
	}
}
