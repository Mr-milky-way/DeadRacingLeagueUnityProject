using thelab.mvc;

namespace drl.game
{
	public class ReplayPlayerController : Controller<DRLApp>
	{
		public ReplayPlayerModel model => AssertLocal<ReplayPlayerModel>("model");

		protected void Update()
		{
			if (model.playing && !model.paused)
			{
				model.Step();
			}
			else
			{
				model.UpdateDrones();
			}
		}
	}
}
