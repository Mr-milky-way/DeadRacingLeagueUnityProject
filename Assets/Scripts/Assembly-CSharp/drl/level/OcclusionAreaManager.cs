using System.Collections.Generic;
using UnityEngine;

namespace drl.level
{
	public class OcclusionAreaManager : MonoBehaviour
	{
		public List<OcclusionArea> areas;

		public List<OcclusionArea> active;

		public List<OcclusionNode> nodes;

		private bool will_refresh;

		private int refresh_iterator;

		protected void Awake()
		{
			for (int i = 0; i < areas.Count; i++)
			{
				areas[i].manager = this;
			}
		}

		[ContextMenu("Collect Nodes")]
		public void CollectNodes()
		{
			nodes = new List<OcclusionNode>();
			for (int i = 0; i < areas.Count; i++)
			{
				OcclusionArea occlusionArea = areas[i];
				if (!occlusionArea.enabled || !occlusionArea.gameObject.activeInHierarchy)
				{
					continue;
				}
				occlusionArea.CollectRenderers();
				for (int j = 0; j < occlusionArea.renderers.Count; j++)
				{
					Renderer it_r = occlusionArea.renderers[j];
					OcclusionNode occlusionNode = nodes.Find((OcclusionNode it_n) => it_n.renderer == it_r);
					if (occlusionNode == null)
					{
						occlusionNode = new OcclusionNode();
						occlusionNode.renderer = it_r;
						occlusionNode.path = "";
						GameObject parentTarget = occlusionArea.GetParentTarget(it_r);
						if ((bool)parentTarget)
						{
							OcclusionNode occlusionNode2 = occlusionNode;
							occlusionNode2.path = occlusionNode2.path + parentTarget.name + "/";
						}
						OcclusionNode occlusionNode3 = occlusionNode;
						occlusionNode3.path = occlusionNode3.path + it_r.name + " @";
						occlusionNode.areas = new List<OcclusionArea>();
					}
					if (!occlusionNode.areas.Contains(occlusionArea))
					{
						occlusionNode.areas.Add(occlusionArea);
						OcclusionNode occlusionNode4 = occlusionNode;
						occlusionNode4.path = occlusionNode4.path + " " + occlusionArea.name;
					}
					nodes.Add(occlusionNode);
				}
			}
		}

		[ContextMenu("Clear Nodes")]
		public void ClearNodes()
		{
			nodes = new List<OcclusionNode>();
		}

		public void OnAreaEnter(OcclusionArea p_target)
		{
			if (!active.Contains(p_target))
			{
				active.Add(p_target);
				refresh_iterator = 0;
			}
		}

		public void OnAreaExit(OcclusionArea p_target)
		{
			if (active.Contains(p_target))
			{
				active.Remove(p_target);
				refresh_iterator = 0;
			}
		}

		protected void RefreshNodes()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				OcclusionNode occlusionNode = nodes[i];
				bool flag = occlusionNode.IsActive(active);
				occlusionNode.renderer.enabled = flag;
			}
		}

		protected void Update()
		{
			will_refresh = refresh_iterator < nodes.Count;
			if (!will_refresh)
			{
				return;
			}
			for (int i = 0; i < 600; i++)
			{
				OcclusionNode occlusionNode = nodes[refresh_iterator];
				bool flag = occlusionNode.IsActive(active);
				if ((bool)occlusionNode.renderer)
				{
					occlusionNode.renderer.enabled = flag;
				}
				refresh_iterator++;
				if (refresh_iterator >= nodes.Count)
				{
					break;
				}
			}
		}
	}
}
