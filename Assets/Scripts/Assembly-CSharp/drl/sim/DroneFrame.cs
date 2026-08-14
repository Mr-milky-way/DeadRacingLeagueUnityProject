using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DroneFrame : DronePart
	{
		[SerializeField]
		private DroneRFCamera m_camera;

		private bool m_hasCamera;

		[SerializeField]
		private DroneCrashBody m_crash;

		public List<DroneESC> escs;

		public List<DroneBattery> batteries;

		public bool batteryBeneath;

		public List<FrameLayoutType> layouts;

		public List<DroneAttachmentType> attachments;

		public List<Collider> colliders;

		public Vector2 tilt = new Vector2(0f, 90f);

		public float size;

		public float propLimit;

		public Vector2 cD = new Vector2(0.8f, 1.6f);

		public Vector2 cL = new Vector2(0.2f, 0.8f);

		public Vector3 surfaceArea;

		public GATechLookupData gatechDragData;

		public Vector3 dragScaling;

		public float extraWeight;

		private Dictionary<string, Transform> m_nodes;

		private float[] rpmRatios = new float[0];

		public DroneRFCamera camera
		{
			get
			{
				if (m_hasCamera)
				{
					return m_camera;
				}
				if ((bool)m_camera)
				{
					m_hasCamera = true;
					return m_camera;
				}
				m_camera = GetComponentInChildren<DroneRFCamera>();
				if ((bool)m_camera)
				{
					m_hasCamera = true;
					return m_camera;
				}
				return null;
			}
			set
			{
				m_camera = value;
				m_hasCamera = m_camera != null;
			}
		}

		public bool hasCamera => m_hasCamera;

		public DroneCrashBody crash
		{
			get
			{
				if (!m_crash)
				{
					return m_crash = GetComponent<DroneCrashBody>();
				}
				return m_crash;
			}
		}

		protected override void OnInitialize()
		{
			escs = new List<DroneESC>();
			batteries = new List<DroneBattery>();
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].name.IndexOf("node-esc") == 0)
				{
					escs.Add(null);
				}
				if (nodes[i].name.IndexOf("node-battery") == 0)
				{
					batteries.Add(null);
				}
			}
			if (colliders == null)
			{
				colliders = new List<Collider>();
			}
			colliders.Clear();
			Transform transform = base.transform.Find("colliders");
			if ((bool)transform)
			{
				for (int j = 0; j < transform.childCount; j++)
				{
					Collider component = transform.GetChild(j).GetComponent<Collider>();
					if ((bool)component)
					{
						if (!colliders.Contains(component))
						{
							colliders.Add(component);
						}
						switch (component.name)
						{
						case "gate":
							component.gameObject.layer = 21;
							component = Object.Instantiate(component, component.transform.parent);
							component.name = "actions";
							component.gameObject.layer = 27;
							colliders.Add(component);
							break;
						}
					}
				}
			}
			Transform transform2 = base.transform.Find("render");
			if (transform2 != null)
			{
				for (int k = 0; k < transform2.childCount; k++)
				{
					Transform child = transform2.GetChild(k);
					if (child.GetComponentInChildren<Renderer>() != null && child.GetComponent<DroneCrashNode>() == null)
					{
						child.gameObject.AddComponent<DroneCrashNode>();
					}
				}
			}
			DroneCrashNode component2 = GetComponent<DroneCrashNode>();
			if ((bool)component2)
			{
				Object.Destroy(component2);
			}
		}

		public DroneFrame Clone(Transform p_parent)
		{
			DroneFrame droneFrame = Object.Instantiate(this, p_parent);
			droneFrame.name = base.drone.name + "-" + info.name + "-instance";
			for (int i = 0; i < droneFrame.escs.Count; i++)
			{
				if (droneFrame.escs[i] == null || !droneFrame.escs[i].hasMotor)
				{
					continue;
				}
				DroneMotor motor = droneFrame.escs[i].motor;
				if (!(motor == null))
				{
					motor.rpm = 0f;
					if (motor.hasAnimation)
					{
						motor.animation.rpm = 0f;
					}
				}
			}
			return droneFrame;
		}

		public DroneFrame Clone()
		{
			return Clone(base.drone.transform.parent);
		}

		public float[] GetRPMRatios()
		{
			if (rpmRatios.Length != escs.Count)
			{
				rpmRatios = new float[escs.Count];
			}
			for (int i = 0; i < escs.Count; i++)
			{
				rpmRatios[i] = escs[i].motor.rpmRatio;
			}
			return rpmRatios;
		}

		public override string GetPrefix()
		{
			return "F";
		}

		public void SpinProps(bool p_spin)
		{
			if (escs == null || escs.Count == 0)
			{
				return;
			}
			for (int i = 0; i < escs.Count; i++)
			{
				DroneESC droneESC = escs[i];
				if ((bool)droneESC && droneESC.hasMotor)
				{
					DroneMotor motor = droneESC.motor;
					motor.rpm = (p_spin ? 21000f : 0f);
					motor.animation.ForceUpdate();
				}
			}
		}

		public Transform GetPartNode(string p_name)
		{
			if (m_nodes == null || m_nodes.Count != nodes.Count)
			{
				m_nodes = new Dictionary<string, Transform>(nodes.Count);
				foreach (Transform node in nodes)
				{
					m_nodes.Add(node.name, node);
				}
			}
			if (m_nodes.ContainsKey(p_name))
			{
				return m_nodes[p_name];
			}
			return null;
		}
	}
}
