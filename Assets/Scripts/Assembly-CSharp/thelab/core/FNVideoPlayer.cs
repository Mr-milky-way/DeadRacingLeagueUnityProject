using System;
using UnityEngine;
using UnityEngine.Video;
using drl.sim;

namespace thelab.core
{
	[Serializable]
	public class FNVideoPlayer : FlowNode
	{
		public string URL = "";

		public VideoClip clip;

		public string description = "Play a fullscreen UI video with given URL.";

		private FlowStatus m_status = FlowStatus.Running;

		[SerializeField]
		private SimulationFlowModule m_module;

		public SimulationFlowModule module
		{
			get
			{
				if (!m_module)
				{
					return Hierarchy.FindReverse<SimulationFlowModule>(base.transform);
				}
				return m_module;
			}
		}

		internal override void OnInitialize()
		{
			if ((bool)module && (bool)module.ui)
			{
				if (clip != null)
				{
					module.ui.Notify("ui.screen.video-player@open", 0f, clip);
				}
				else if (!string.IsNullOrEmpty(URL))
				{
					module.ui.Notify("ui.screen.video-player@open", 0f, URL);
				}
				else
				{
					m_status = FlowStatus.Complete;
				}
			}
		}

		internal override FlowStatus OnUpdate()
		{
			return m_status;
		}

		internal override void OnMessage(string p_event, params object[] p_data)
		{
			if (!(p_event == "fn.mission.video-player@end") || p_data.Length == 0)
			{
				return;
			}
			VideoClip videoClip = p_data[0] as VideoClip;
			if (videoClip != null)
			{
				if (videoClip == clip)
				{
					m_status = FlowStatus.Complete;
				}
				return;
			}
			string text = (string)p_data[0];
			if (!string.IsNullOrEmpty(text) && text == URL)
			{
				m_status = FlowStatus.Complete;
			}
		}
	}
}
