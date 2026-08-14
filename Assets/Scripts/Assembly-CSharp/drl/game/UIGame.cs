using thelab.mvc;

namespace drl.game
{
	public class UIGame : View<DRLApp>
	{
		public bool preventFooter;

		public UIHUD hud => AssertFind<UIHUD>("hud");
	}
}
