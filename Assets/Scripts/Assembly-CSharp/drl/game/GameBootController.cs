using System.Collections.Generic;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class GameBootController : Controller<DRLApp>
	{
		public bool ready;

		public List<string> steps;

		public List<string> boot;

		public GameModel model => AssertLocal<GameModel>("model");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "scene.start":
				boot = new List<string>(new string[4] { "scene.game.scenes@complete", "boot@complete", "game.level.load@complete", "game.track.load@complete" });
				steps = new List<string>(boot);
				break;
			case "game.simulation.load@complete":
				Debug.Log("GameBootController> SimulationLoadComplete");
				Notify("game.ready");
				break;
			}
			OnBootStepNotification(p_event);
		}

		protected void OnBootStepNotification(string p_event)
		{
			if (steps.Contains(p_event) && !ready)
			{
				steps.Remove(p_event);
				Debug.Log("GameBootController> OnBootStepNotification / Step - step[" + p_event + "] count[" + steps.Count + "]");
				if (steps.Count <= 0)
				{
					ready = true;
					Debug.Log("GameBootController> OnBootStepNotification / Boot Complete");
					Notify(0.01f, "game.boot");
				}
			}
		}
	}
}
