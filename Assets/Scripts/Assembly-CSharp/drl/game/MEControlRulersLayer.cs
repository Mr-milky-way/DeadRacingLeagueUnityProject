using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MEControlRulersLayer : MEGraphLayer
	{
		public void Set(List<MAEntity> p_anchors, List<Transform> p_targets)
		{
			Set(p_anchors);
			List<MERulersMetricWidget> list = GetNodes<MERulersMetricWidget>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Set(p_anchors[i], p_targets);
			}
		}

		public void Clear()
		{
			List<MERulersMetricWidget> list = GetNodes<MERulersMetricWidget>();
			nodes.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Clear();
			}
		}
	}
}
