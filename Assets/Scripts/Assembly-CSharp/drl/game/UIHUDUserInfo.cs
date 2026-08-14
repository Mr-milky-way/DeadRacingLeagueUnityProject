using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHUDUserInfo : View<DRLApp>
	{
		public Text profileNameField;

		public Image backgroundField;

		public RawImage profilePhotoField;

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public string profileName
		{
			get
			{
				return profileNameField.text;
			}
			set
			{
				profileNameField.text = value;
			}
		}

		public Texture2D profilePhoto
		{
			set
			{
				profilePhotoField.texture = value;
				profilePhotoField.enabled = value != null;
			}
		}

		public Color profileColor
		{
			set
			{
				backgroundField.color = value;
			}
		}

		public void Set(GamePlayerData p_player)
		{
			if (p_player == null || string.IsNullOrEmpty(p_player.name) || p_player.photo == null)
			{
				fade.alpha = -0.1f;
				return;
			}
			profileName = p_player.name.ToUpper();
			profileColor = p_player.color;
			profilePhoto = p_player.photo;
		}
	}
}
