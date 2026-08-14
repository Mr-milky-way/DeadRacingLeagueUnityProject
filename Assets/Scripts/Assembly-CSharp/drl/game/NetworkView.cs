using thelab.mvc;

namespace drl.game
{
	public class NetworkView : View<DRLApp>
	{
		protected void Awake()
		{
		}

		public void OnPersistency()
		{
			base.app.view.network = this;
		}
	}
}
