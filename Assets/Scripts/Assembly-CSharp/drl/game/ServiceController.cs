using thelab.mvc;

namespace drl.game
{
	public class ServiceController : Controller<DRLApp>
	{
		public ServiceModel model => AssertLocal<ServiceModel>("model");

		protected override void Start()
		{
		}

		public void OnPersistency()
		{
			base.app.controller.service = this;
		}
	}
}
