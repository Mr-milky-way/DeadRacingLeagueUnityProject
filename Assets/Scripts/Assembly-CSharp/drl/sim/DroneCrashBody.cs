using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneCrashBody : MonoBehaviour
	{
		public List<DroneCrashNode> nodes;

		internal DroneCrashNode node_sort_target;

		[SerializeField]
		private Vector3 position;

		[SerializeField]
		private Quaternion rotation;

		public void Link()
		{
			nodes = Hierarchy.FindAll<DroneCrashNode>(base.transform);
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].CalculateCenterOfMass();
			}
			Link(nodes);
			ReverseLink(nodes);
		}

		public void SetFixData()
		{
			position = base.transform.localPosition;
			rotation = base.transform.localRotation;
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].SetFixData();
			}
		}

		public void FixSnap()
		{
			base.transform.localPosition = position;
			base.transform.localRotation = rotation;
			if (nodes == null)
			{
				return;
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				if ((bool)nodes[i])
				{
					nodes[i].FixSnap();
				}
			}
		}

		public void Link(DroneCrashNode p_target)
		{
			if (!p_target || p_target.links == null)
			{
				return;
			}
			List<DroneCrashNode> list = new List<DroneCrashNode>();
			for (int i = 0; i < p_target.links.Count; i++)
			{
				CrashNodeType p_type = p_target.links[i];
				list.Clear();
				for (int j = 0; j < nodes.Count; j++)
				{
					DroneCrashNode droneCrashNode = nodes[j];
					if (!(droneCrashNode == p_target))
					{
						if (!droneCrashNode)
						{
							nodes.RemoveAt(j--);
							break;
						}
						if (droneCrashNode.Match(p_type))
						{
							list.Add(droneCrashNode);
						}
					}
				}
				if (list.Count > 0)
				{
					node_sort_target = p_target;
					list.Sort(DistanceSort);
					node_sort_target = null;
					for (int k = 0; k < list.Count; k++)
					{
						_ = list[k];
					}
					if (!p_target.siblings.Contains(list[0]))
					{
						p_target.siblings.Add(list[0]);
					}
					if (!p_target.children.Contains(list[0]))
					{
						p_target.children.Add(list[0]);
					}
				}
			}
		}

		private void ReverseLink(List<DroneCrashNode> p_nodes)
		{
			for (int i = 0; i < p_nodes.Count; i++)
			{
				DroneCrashNode droneCrashNode = p_nodes[i];
				if (droneCrashNode == null || droneCrashNode.siblings == null)
				{
					continue;
				}
				for (int j = 0; j < droneCrashNode.siblings.Count; j++)
				{
					DroneCrashNode droneCrashNode2 = droneCrashNode.siblings[j];
					if (!(droneCrashNode2 == null) && (droneCrashNode2.siblings == null || !droneCrashNode2.siblings.Contains(droneCrashNode)))
					{
						if (droneCrashNode2.siblings == null)
						{
							droneCrashNode2.siblings = new List<DroneCrashNode>();
						}
						droneCrashNode2.siblings.Add(droneCrashNode);
					}
				}
			}
		}

		public void Link(List<DroneCrashNode> p_targets)
		{
			for (int i = 0; i < p_targets.Count; i++)
			{
				Link(p_targets[i]);
			}
		}

		protected int DistanceSort(DroneCrashNode a, DroneCrashNode b)
		{
			DroneCrashNode droneCrashNode = node_sort_target;
			if (!droneCrashNode)
			{
				return 0;
			}
			Vector3 worldCenterOfMass = droneCrashNode.worldCenterOfMass;
			Vector3 worldCenterOfMass2 = a.worldCenterOfMass;
			Vector3 worldCenterOfMass3 = b.worldCenterOfMass;
			float num = Vector3.Distance(worldCenterOfMass2, worldCenterOfMass);
			float num2 = Vector3.Distance(worldCenterOfMass3, worldCenterOfMass);
			if (!(num < num2))
			{
				return 1;
			}
			return -1;
		}

		private void DebugLinks()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				DroneCrashNode droneCrashNode = nodes[i];
				Vector3 worldCenterOfMass = droneCrashNode.worldCenterOfMass;
				for (int j = 0; j < droneCrashNode.siblings.Count; j++)
				{
					Vector3 worldCenterOfMass2 = droneCrashNode.siblings[j].worldCenterOfMass;
					Debug.DrawLine(worldCenterOfMass, worldCenterOfMass2, Color.magenta, 20f, depthTest: false);
				}
			}
		}

		private void Update()
		{
		}
	}
}
