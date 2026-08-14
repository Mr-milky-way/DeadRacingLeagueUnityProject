using System.Collections.Generic;

public class AkRoomManager
{
	private readonly List<AkRoomPortal> m_Portals = new List<AkRoomPortal>();

	private readonly List<AkRoomPortal> m_PortalsToUpdate = new List<AkRoomPortal>();

	private readonly List<AkSurfaceReflector> m_Reflectors = new List<AkSurfaceReflector>();

	private readonly List<AkSurfaceReflector> m_ReflectorsToUpdate = new List<AkSurfaceReflector>();

	private static AkRoomManager m_Instance;

	public static void Init()
	{
		if (m_Instance == null)
		{
			m_Instance = new AkRoomManager();
		}
	}

	public static void Terminate()
	{
		if (m_Instance != null)
		{
			m_Instance = null;
		}
	}

	public static void RegisterPortal(AkRoomPortal portal)
	{
		if (m_Instance != null)
		{
			if (!m_Instance.m_Portals.Contains(portal))
			{
				m_Instance.m_Portals.Add(portal);
			}
			if (!m_Instance.m_PortalsToUpdate.Contains(portal))
			{
				m_Instance.m_PortalsToUpdate.Add(portal);
			}
		}
	}

	public static void UnregisterPortal(AkRoomPortal portal)
	{
		if (m_Instance != null)
		{
			m_Instance.m_Portals.Remove(portal);
			m_Instance.m_PortalsToUpdate.Remove(portal);
		}
	}

	public static void RegisterReflector(AkSurfaceReflector reflector)
	{
		if (m_Instance != null)
		{
			if (!m_Instance.m_Reflectors.Contains(reflector))
			{
				m_Instance.m_Reflectors.Add(reflector);
			}
			if (!m_Instance.m_ReflectorsToUpdate.Contains(reflector))
			{
				m_Instance.m_ReflectorsToUpdate.Add(reflector);
			}
		}
	}

	public static void UnregisterReflector(AkSurfaceReflector reflector)
	{
		if (m_Instance != null)
		{
			m_Instance.m_Reflectors.Remove(reflector);
			m_Instance.m_ReflectorsToUpdate.Remove(reflector);
		}
	}

	public static void RegisterPortalUpdate(AkRoomPortal portal)
	{
		if (m_Instance != null && m_Instance.m_Portals.Contains(portal) && !m_Instance.m_PortalsToUpdate.Contains(portal))
		{
			m_Instance.m_PortalsToUpdate.Add(portal);
		}
	}

	public static void RegisterRoomUpdate(AkRoom room)
	{
		if (m_Instance == null)
		{
			return;
		}
		for (int i = 0; i < m_Instance.m_Portals.Count; i++)
		{
			AkRoomPortal akRoomPortal = m_Instance.m_Portals[i];
			if (!m_Instance.m_PortalsToUpdate.Contains(akRoomPortal) && (room == akRoomPortal.frontRoom || room == akRoomPortal.backRoom || akRoomPortal.Overlaps(room)))
			{
				m_Instance.m_PortalsToUpdate.Add(akRoomPortal);
			}
		}
		for (int j = 0; j < m_Instance.m_Reflectors.Count; j++)
		{
			AkSurfaceReflector akSurfaceReflector = m_Instance.m_Reflectors[j];
			if (!m_Instance.m_ReflectorsToUpdate.Contains(akSurfaceReflector) && akSurfaceReflector.AssociatedRoom == room)
			{
				m_Instance.m_ReflectorsToUpdate.Add(akSurfaceReflector);
			}
		}
	}

	public static void Update()
	{
		if (m_Instance != null)
		{
			for (int i = 0; i < m_Instance.m_PortalsToUpdate.Count; i++)
			{
				m_Instance.m_PortalsToUpdate[i].UpdateRoomPortal();
			}
			m_Instance.m_PortalsToUpdate.Clear();
			for (int j = 0; j < m_Instance.m_ReflectorsToUpdate.Count; j++)
			{
				m_Instance.m_ReflectorsToUpdate[j].UpdateGeometry();
			}
			m_Instance.m_ReflectorsToUpdate.Clear();
		}
	}
}
