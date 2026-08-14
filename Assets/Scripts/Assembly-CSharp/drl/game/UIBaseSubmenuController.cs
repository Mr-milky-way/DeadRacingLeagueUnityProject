using thelab.mvc;

namespace drl.game
{
	public abstract class UIBaseSubmenuController : Controller<DRLApp>
	{
		public UIBaseSubmenuView view => AssertLocal<UIBaseSubmenuView>("view");

		public bool IsOpen => view.IsOpen;

		public virtual void Setup<T>(T configData) where T : UISubmenuData
		{
			view.Setup(configData);
		}

		public void Fold(float duration = 0.3f)
		{
			view.SubmenuFold(duration);
		}

		public void Unfold(float duration = 0.3f)
		{
			view.SubmenuUnfold(duration);
		}
	}
}
