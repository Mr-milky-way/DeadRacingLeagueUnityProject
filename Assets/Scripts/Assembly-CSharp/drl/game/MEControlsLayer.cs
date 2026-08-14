using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEControlsLayer : View<DRLApp>
	{
		private MapEditorController m_editor;

		public MEGraphLayer gates;

		public MEGraphLayer podiums;

		public MEControlRulersLayer rulers;

		public MEControlLayoutLayer layout;

		private CanvasGroup m_cgroup;

		private FadeComponent m_fade;

		public MapEditorController editor
		{
			get
			{
				return m_editor;
			}
			set
			{
				m_editor = value;
				gates.editor = value;
				podiums.editor = value;
				rulers.editor = value;
				layout.editor = value;
			}
		}

		public CanvasGroup cgroup
		{
			get
			{
				if (!m_cgroup)
				{
					return m_cgroup = GetComponent<CanvasGroup>();
				}
				return m_cgroup;
			}
		}

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = GetComponent<FadeComponent>();
				}
				return m_fade;
			}
		}

		public bool inputEnabled
		{
			get
			{
				if ((bool)cgroup)
				{
					return cgroup.blocksRaycasts;
				}
				return false;
			}
			set
			{
				if ((bool)cgroup)
				{
					if (cgroup.blocksRaycasts != value)
					{
						cgroup.blocksRaycasts = value;
					}
					if (cgroup.interactable != value)
					{
						cgroup.interactable = value;
					}
				}
			}
		}

		protected void Awake()
		{
			gates.alpha = -0.1f;
			podiums.alpha = -0.1f;
			rulers.alpha = -0.1f;
			layout.alpha = -0.1f;
		}

		public void SetGatesGraph(List<MAGate> p_targets)
		{
			gates.Set(p_targets);
			bool flag = p_targets.Count > 0 && p_targets[p_targets.Count - 1].isFinish;
			List<DRLNumberFieldView> nodes = gates.GetNodes<DRLNumberFieldView>();
			new List<string>();
			for (int i = 0; i < p_targets.Count; i++)
			{
				MAGate mAGate = p_targets[i];
				nodes[i].minValue = 1f;
				nodes[i].maxValue = p_targets.Count;
				nodes[i].value = p_targets[i].index + 1;
				if (!mAGate.isFinish && flag)
				{
					nodes[i].maxValue -= 1f;
				}
				bool flag2 = mAGate.isLapStart || mAGate.isLapEnd;
				bool interactable = !mAGate.isFinish;
				Color color = (mAGate.isFinish ? Color.red : (flag2 ? DRLColor.yellow : DRLColor.green));
				nodes[i].enabled = interactable;
				nodes[i].input.interactable = interactable;
				nodes[i].label.transform.GetChild(0).GetComponent<Image>().color = color;
				nodes[i].label.transform.GetChild(1).GetComponent<Image>().enabled = mAGate.isLapStart;
				nodes[i].label.transform.GetChild(2).GetComponent<Image>().enabled = mAGate.isLapEnd;
			}
		}

		public void SetPodiumsGraph(List<MAPodium> p_targets)
		{
			podiums.spline.SetEnabled(p_flag: false);
			podiums.Set(p_targets);
			List<DRLNumberFieldView> nodes = podiums.GetNodes<DRLNumberFieldView>();
			new List<string>();
			for (int i = 0; i < p_targets.Count; i++)
			{
				nodes[i].minValue = 1f;
				nodes[i].maxValue = p_targets.Count;
				nodes[i].value = p_targets[i].index + 1;
			}
		}
	}
}
