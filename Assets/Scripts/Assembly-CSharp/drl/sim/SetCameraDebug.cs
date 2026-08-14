using System.Collections;
using UnityEngine;

namespace drl.sim
{
	public class SetCameraDebug : MonoBehaviour
	{
		private DroneCamera m_cam;

		private DroneSimulation m_sim;

		private DroneCamera cam
		{
			get
			{
				if (m_cam == null)
				{
					m_cam = Object.FindObjectOfType<DroneCamera>();
				}
				return m_cam;
			}
		}

		private DroneSimulation sim
		{
			get
			{
				if (m_sim == null)
				{
					m_sim = Object.FindObjectOfType<DroneSimulation>();
				}
				return m_sim;
			}
		}

		private void Start()
		{
			StartCoroutine(WaitAndSwitchCamera());
		}

		private IEnumerator WaitAndSwitchCamera()
		{
			yield return new WaitForSeconds(2f);
			cam.SetFPV(sim.drones.list[0]);
		}
	}
}
