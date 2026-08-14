using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class ReplayRecorderController : Controller<DRLApp>
	{
		public ReplayRecorderModel model => AssertLocal<ReplayRecorderModel>("model");

		public GameController game => base.app.controller.game;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "game.pause":
				Pause();
				break;
			case "game.unpause":
				Unpause();
				break;
			}
		}

		public void Clear()
		{
			model.Clear();
		}

		public void Record()
		{
			model.paused = false;
		}

		public void Pause()
		{
			model.paused = true;
		}

		public void Unpause()
		{
			model.paused = false;
		}

		public void Stop()
		{
			model.Stop();
		}

		protected void LateUpdate()
		{
			if (!model.paused)
			{
				float deltaTime = Time.deltaTime;
				model.UpdateDrones(deltaTime);
			}
		}
	}
}
