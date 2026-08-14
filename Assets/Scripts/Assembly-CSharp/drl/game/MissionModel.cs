using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MissionModel : Model<DRLApp>
	{
		public GameObject root;

		public DRLQuest quest;

		public DRLMission mission;

		public SimulationFlowModule module;

		public bool completed;

		public float score;

		public float EvaluateScore()
		{
			DataFlow p_data = (module ? module.data : null);
			return score = (mission ? mission.Evaluate(p_data) : 1f);
		}
	}
}
