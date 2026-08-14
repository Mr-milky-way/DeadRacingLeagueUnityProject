using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.level
{
	[Serializable]
	public class OcclusionNode
	{
		public string path;

		public Renderer renderer;

		public List<OcclusionArea> areas;

		public bool IsActive(List<OcclusionArea> p_areas)
		{
			for (int i = 0; i < p_areas.Count; i++)
			{
				OcclusionArea item = p_areas[i];
				if (areas.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}
}
