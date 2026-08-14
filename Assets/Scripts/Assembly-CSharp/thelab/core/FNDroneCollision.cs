using System;

namespace thelab.core
{
	[Serializable]
	public class FNDroneCollision : FlowNode
	{
		public string description = "Drone collision waiting condition.";

		private bool m_detect = true;

		private string m_notification = "";

		private string m_detectionStop = "fn.missions.collision-detection.stop";

		private string m_detectionRun = "fn.missions.collision-detection.run";

		internal override void OnInitialize()
		{
			m_notification = "";
		}

		internal override FlowStatus OnUpdate()
		{
			if (!(m_notification == "fn.mission.drone@collision") || !m_detect)
			{
				return FlowStatus.Running;
			}
			return FlowStatus.Complete;
		}

		internal override void OnMessage(string p_event, params object[] p_data)
		{
			m_notification = p_event;
			if (p_event == m_detectionStop)
			{
				m_detect = false;
			}
			if (p_event == m_detectionRun)
			{
				m_detect = true;
			}
		}
	}
}
