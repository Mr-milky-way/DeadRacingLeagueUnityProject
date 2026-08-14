using System.Collections.Generic;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class MEControlLayoutLayer : MEGraphLayer
	{
		public VerticalLayoutGroup layout;

		public void Set(List<MARenderer> p_anchors, List<MARenderer> p_targets)
		{
			Set(p_anchors);
			MEControlLayoutLayerController component = GetComponent<MEControlLayoutLayerController>();
			List<MELayoutWidget> list = GetNodes<MELayoutWidget>();
			if ((bool)layout)
			{
				layout.enabled = false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				MELayoutWidget mELayoutWidget = list[i];
				MARenderer p_anchor = p_anchors[i];
				mELayoutWidget.instanceCount = 10;
				mELayoutWidget.controller = component;
				mELayoutWidget.Set(p_anchor, p_targets);
				mELayoutWidget.Generate(p_rebuild: true);
			}
			this.TimerRunOnce(delegate
			{
				if ((bool)layout)
				{
					layout.enabled = true;
				}
			}, 1f / 60f);
		}

		public void Clear()
		{
			List<MELayoutWidget> list = GetNodes<MELayoutWidget>();
			nodes.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Clear();
			}
		}
	}
}
