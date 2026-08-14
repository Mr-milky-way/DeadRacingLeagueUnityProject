using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIChatSubmenuData : UISubmenuData
	{
		public string steamId;

		public string userName;

		public bool isFriend;

		public bool isOnline;

		public bool isPrivate;

		public string photoURL;

		public Color color;

		public string platform;

		public UIChatSubmenuData(string steamId, string userName, bool isFriend, bool isOnline, bool inPrivate, string photoURL, Color color, string p_platform, UINavigation parentNav, Component parentNextDown, Image submenuToggleImg = null, Sprite foldedIcon = null, Sprite unFoldedIcon = null)
			: base(parentNav, parentNextDown, submenuToggleImg, foldedIcon, unFoldedIcon)
		{
			this.steamId = steamId;
			this.userName = userName;
			this.isFriend = isFriend;
			this.isOnline = isOnline;
			isPrivate = inPrivate;
			this.photoURL = photoURL;
			this.color = color;
			platform = p_platform;
		}

		public bool IsXbox()
		{
			if (!string.IsNullOrEmpty(platform))
			{
				return platform == "xbox";
			}
			return false;
		}
	}
}
