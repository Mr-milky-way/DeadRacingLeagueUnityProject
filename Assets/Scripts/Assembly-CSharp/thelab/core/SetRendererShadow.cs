using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace thelab.core
{
	public class SetRendererShadow : MonoBehaviour
	{
		public ShadowCastingMode flag;

		public List<Transform> targets;

		public List<Transform> ignore;

		public bool includeChildren = true;

		public bool playOnAwake = true;

		protected void Awake()
		{
			if (playOnAwake)
			{
				Apply();
			}
		}

		public void Apply()
		{
			List<Transform> tl = new List<Transform>();
			List<Transform> il = new List<Transform>();
			for (int i = 0; i < targets.Count; i++)
			{
				Transform transform = targets[i];
				if (!transform)
				{
					continue;
				}
				if (!includeChildren)
				{
					tl.Add(transform);
					continue;
				}
				Hierarchy.Traverse(transform, delegate(Transform p_item)
				{
					tl.Add(p_item);
				});
			}
			for (int num = 0; num < ignore.Count; num++)
			{
				Transform transform2 = ignore[num];
				if (!transform2)
				{
					continue;
				}
				if (!includeChildren)
				{
					il.Add(transform2);
					continue;
				}
				Hierarchy.Traverse(transform2, delegate(Transform p_item)
				{
					il.Add(p_item);
				});
			}
			for (int num2 = 0; num2 < tl.Count; num2++)
			{
				Transform transform3 = tl[num2];
				if (!il.Contains(transform3))
				{
					MeshRenderer component = transform3.GetComponent<MeshRenderer>();
					if ((bool)component)
					{
						component.shadowCastingMode = flag;
					}
				}
			}
		}
	}
}
