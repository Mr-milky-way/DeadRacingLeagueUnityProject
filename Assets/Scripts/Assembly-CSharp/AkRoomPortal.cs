using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkRoomPortal")]
[RequireComponent(typeof(BoxCollider))]
[DisallowMultipleComponent]
public class AkRoomPortal : AkTriggerHandler
{
	public enum State
	{
		Closed = 0,
		Open = 1
	}

	public const int MAX_ROOMS_PER_PORTAL = 2;

	public State initialState;

	private bool active = true;

	public List<int> closePortalTriggerList = new List<int>();

	[SerializeField]
	private AkRoom[] rooms = new AkRoom[2];

	private AkRoom.PriorityList[] roomList = new AkRoom.PriorityList[2]
	{
		new AkRoom.PriorityList(),
		new AkRoom.PriorityList()
	};

	private AkTransform portalTransform;

	private BoxCollider portalCollider;

	private bool portalSet;

	public bool portalActive
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
			AkRoomManager.RegisterPortalUpdate(this);
		}
	}

	private ulong frontRoomID
	{
		get
		{
			if (!IsRoomActive(frontRoom))
			{
				return AkRoom.INVALID_ROOM_ID;
			}
			return frontRoom.GetID();
		}
	}

	private ulong backRoomID
	{
		get
		{
			if (!IsRoomActive(backRoom))
			{
				return AkRoom.INVALID_ROOM_ID;
			}
			return backRoom.GetID();
		}
	}

	public AkRoom frontRoom => rooms[1];

	public AkRoom backRoom => rooms[0];

	public bool IsValid => frontRoomID != backRoomID;

	public AkRoom GetRoom(int index)
	{
		return rooms[index];
	}

	private void SetRoomPortal()
	{
		if (!base.enabled)
		{
			return;
		}
		if (IsValid)
		{
			portalTransform.Set(portalCollider.bounds.center, base.transform.forward, base.transform.up);
			Vector3 extent = Vector3.Scale(portalCollider.size, base.transform.localScale) / 2f;
			AkSoundEngine.SetRoomPortal(GetID(), portalTransform, extent, active, frontRoomID, backRoomID);
			portalSet = true;
			return;
		}
		Debug.LogError(base.name + " has identical front and back rooms. It will not be sent to Spatial Audio.");
		if (portalSet)
		{
			AkSoundEngine.RemovePortal(GetID());
		}
		portalSet = false;
	}

	public void UpdateRoomPortal()
	{
		UpdateRooms();
		SetRoomPortal();
	}

	public bool Overlaps(AkRoom room)
	{
		FindOverlappingRooms(roomList);
		for (int i = 0; i < 2; i++)
		{
			if (roomList[i].Contains(room))
			{
				return true;
			}
		}
		return false;
	}

	public ulong GetID()
	{
		return (ulong)GetInstanceID();
	}

	protected override void Awake()
	{
		portalCollider = GetComponent<BoxCollider>();
		portalCollider.isTrigger = true;
		portalTransform = new AkTransform();
		portalActive = initialState != State.Closed;
		RegisterTriggers(closePortalTriggerList, ClosePortal);
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (closePortalTriggerList.Contains(1281810935))
		{
			ClosePortal(null);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		Open();
	}

	public void ClosePortal(GameObject in_gameObject)
	{
		Close();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnregisterTriggers(closePortalTriggerList, ClosePortal);
	}

	private void OnEnable()
	{
		UpdateRooms();
		AkRoomManager.RegisterPortal(this);
	}

	private void OnDisable()
	{
		AkRoomManager.UnregisterPortal(this);
		if (portalSet)
		{
			AkSoundEngine.RemovePortal(GetID());
		}
		portalSet = false;
	}

	private bool IsRoomActive(AkRoom in_room)
	{
		if (in_room != null)
		{
			return in_room.isActiveAndEnabled;
		}
		return false;
	}

	public void Open()
	{
		portalActive = true;
	}

	public void Close()
	{
		portalActive = false;
	}

	public void FindOverlappingRooms(AkRoom.PriorityList[] roomList)
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if (!(component == null))
		{
			float num = component.size.z / 2f;
			FillRoomList(Vector3.forward * (0f - num), roomList[0]);
			FillRoomList(Vector3.forward * num, roomList[1]);
		}
	}

	private void FillRoomList(Vector3 position, AkRoom.PriorityList list)
	{
		list.Clear();
		position = base.transform.TransformPoint(position);
		Collider[] array = Physics.OverlapSphere(position, 0f, -1, QueryTriggerInteraction.Collide);
		for (int i = 0; i < array.Length; i++)
		{
			AkRoom component = array[i].gameObject.GetComponent<AkRoom>();
			if (component != null && !list.Contains(component))
			{
				list.Add(component);
			}
		}
	}

	public void UpdateRooms()
	{
		FindOverlappingRooms(roomList);
		bool flag = false;
		for (int i = 0; i < 2; i++)
		{
			AkRoom highestPriorityActiveAndEnabledRoom = roomList[i].GetHighestPriorityActiveAndEnabledRoom();
			if (highestPriorityActiveAndEnabledRoom != rooms[i])
			{
				flag = true;
			}
			rooms[i] = highestPriorityActiveAndEnabledRoom;
		}
		if (flag)
		{
			AkRoomManager.RegisterPortalUpdate(this);
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public void SetRoom(int in_roomIndex, AkRoom in_room)
	{
		Debug.LogFormat("SetRoom is deprecated. Highest priority, active and enabled room will be automatically chosen. Make sure room priorities and game object placements are correct.");
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public void SetFrontRoom(AkRoom room)
	{
		Debug.LogFormat("SetFrontRoom is deprecated. Highest priority, active and enabled room will be automatically chosen. Make sure room priorities and game object placements are correct.");
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public void SetBackRoom(AkRoom room)
	{
		Debug.LogFormat("SetBackRoom is deprecated. Highest priority, active and enabled room will be automatically chosen. Make sure room priorities and game object placements are correct.");
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public void UpdateSoundEngineRoomIDs()
	{
		UpdateRoomPortal();
	}

	[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
	public void UpdateOverlappingRooms()
	{
		UpdateRooms();
	}
}
