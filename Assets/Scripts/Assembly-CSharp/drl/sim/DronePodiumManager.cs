using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DronePodiumManager : DroneSimulationManager<DronePodium>
	{
		public DronePodium template;

		public Transform root;

		public void Build()
		{
			if ((bool)root)
			{
				int childCount = root.childCount;
				for (int i = 0; i < childCount; i++)
				{
					Push(template);
				}
			}
		}

		public DronePodium Push(DronePodium p_template, Transform p_anchor = null)
		{
			if (!root)
			{
				Debug.LogWarning("DronePodiumManager> Invalid root transform!");
				return null;
			}
			DronePodium p_template2 = (p_template ? p_template : template);
			DronePodium dronePodium = Instantiate(p_template2);
			if (!dronePodium)
			{
				return dronePodium;
			}
			int num = base.list.Count - 1;
			if (!p_anchor)
			{
				if (num < 0)
				{
					return null;
				}
				if (num >= root.childCount)
				{
					return null;
				}
			}
			Transform transform = (p_anchor ? p_anchor : root.GetChild(num));
			if ((bool)p_anchor)
			{
				num = dronePodium.transform.GetSiblingIndex();
			}
			dronePodium.name = num.ToString() ?? "";
			dronePodium.transform.position = transform.position;
			dronePodium.transform.rotation = transform.rotation;
			dronePodium.transform.localScale = transform.lossyScale;
			return dronePodium;
		}

		public DronePodium GetClosestToDrone(Drone p_drone)
		{
			if (!p_drone)
			{
				return null;
			}
			if (base.list.Count <= 0)
			{
				return null;
			}
			if (base.list.Count <= 1)
			{
				return base.list[0];
			}
			int index = 0;
			float num = Vector3.Distance(p_drone.position, base.list[0].spawn.position);
			for (int i = 1; i < base.list.Count; i++)
			{
				float num2 = Vector3.Distance(p_drone.position, base.list[i].spawn.position);
				if (!(num2 >= num))
				{
					num = num2;
					index = i;
				}
			}
			return base.list[index];
		}

		public TransformVector GetAverageTransform()
		{
			if (base.list.Count <= 0)
			{
				return TransformVector.identity;
			}
			Vector3 position = base.list[0].transform.position;
			Vector3 forward = base.list[0].transform.forward;
			if (base.list.Count <= 1)
			{
				return new TransformVector(position, Quaternion.LookRotation(forward, Vector3.up));
			}
			for (int i = 1; i < base.list.Count; i++)
			{
				position += base.list[i].transform.position;
				forward += base.list[i].transform.forward;
			}
			forward = ((forward.sqrMagnitude <= 0.0001f) ? Vector3.forward : forward);
			forward *= 1f / (float)base.list.Count;
			forward.Normalize();
			position *= 1f / (float)base.list.Count;
			return new TransformVector(position, Quaternion.LookRotation(forward, Vector3.up));
		}

		public override string GetContainerName()
		{
			return "podiums";
		}
	}
}
