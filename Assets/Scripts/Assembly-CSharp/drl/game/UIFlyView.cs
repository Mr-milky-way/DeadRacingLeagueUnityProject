using thelab.core;

namespace drl.game
{
	public class UIFlyView : UIScreenView
	{
		public ListComponent listField;

		public void ActivateAllTiles(bool p_activate)
		{
			if (listField.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < listField.Count; i++)
			{
				UICardButtonLarge uICardButtonLarge = listField.Get<UICardButtonLarge>(i);
				if ((bool)uICardButtonLarge)
				{
					uICardButtonLarge.gameObject.SetActive(p_activate);
				}
			}
		}
	}
}
