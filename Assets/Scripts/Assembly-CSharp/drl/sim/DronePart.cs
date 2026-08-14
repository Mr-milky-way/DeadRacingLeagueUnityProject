using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DronePart : DRLAsset
	{
		protected Drone m_drone;

		protected bool m_attached;

		private DroneAssetTag m_tags;

		public float weight;

		public Drone drone
		{
			get
			{
				if (m_attached)
				{
					return m_drone;
				}
				if ((bool)m_drone)
				{
					m_attached = true;
					return m_drone;
				}
				m_drone = GetComponent<Drone>();
				if ((bool)m_drone)
				{
					m_attached = true;
					return m_drone;
				}
				m_drone = Hierarchy.FindReverse<Drone>(base.transform);
				if ((bool)m_drone)
				{
					m_attached = true;
					return m_drone;
				}
				return null;
			}
		}

		public bool attached => m_attached;

		public List<DroneAssetTagType> tags
		{
			get
			{
				m_tags = CheckTags(m_tags);
				if (!m_tags)
				{
					return new List<DroneAssetTagType>();
				}
				return m_tags.tags;
			}
		}

		public DroneAssetTagType category
		{
			get
			{
				m_tags = CheckTags(m_tags);
				if (!m_tags)
				{
					return DroneAssetTagType.None;
				}
				return m_tags.category;
			}
		}

		private void Start()
		{
			Renderer componentInChildren = GetComponentInChildren<Renderer>();
			DroneCrashNode component = GetComponent<DroneCrashNode>();
			DroneCrashBody component2 = GetComponent<DroneCrashBody>();
			if (componentInChildren != null)
			{
				if (!component && !component2 && !(this is DroneFrame))
				{
					DroneCrashNode droneCrashNode = base.gameObject.AddComponent<DroneCrashNode>();
					switch (category)
					{
					case DroneAssetTagType.Antenna:
						droneCrashNode.tags = new List<CrashNodeType>();
						droneCrashNode.tags.Add(CrashNodeType.Antenna0);
						break;
					case DroneAssetTagType.CameraRF:
						droneCrashNode.tags = new List<CrashNodeType>();
						droneCrashNode.tags.Add(CrashNodeType.Camera0);
						break;
					}
				}
			}
			else if ((bool)component)
			{
				Object.Destroy(component);
			}
		}
	}
}
