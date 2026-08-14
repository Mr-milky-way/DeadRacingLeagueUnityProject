using thelab.mvc;

namespace drl.game
{
	public class UIUserItemController : Controller<DRLApp>
	{
		public virtual UIUserItemView view => AssertLocal<UIUserItemView>("view");

		public virtual void Populate(GameFriendData p_data)
		{
			view.Set(p_data);
		}
	}
}
