using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UISubmenuData
	{
		public UINavigation parentNav;

		public Component parentNext;

		public Image submenuToggleImg;

		public Sprite foldedIcon;

		public Sprite unFoldedIcon;

		public UISubmenuData(UINavigation parentNav, Component parentNext, Image submenuToggleImg = null, Sprite foldedIcon = null, Sprite unFoldedIcon = null)
		{
			this.parentNav = parentNav;
			this.parentNext = parentNext;
			this.submenuToggleImg = submenuToggleImg;
			this.foldedIcon = foldedIcon;
			this.unFoldedIcon = unFoldedIcon;
		}
	}
}
