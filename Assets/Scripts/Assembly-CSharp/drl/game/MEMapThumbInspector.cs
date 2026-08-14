using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEMapThumbInspector : View<DRLApp>
	{
		public RawImage defaultImage;

		public RawImage photoImage;

		public FadeComponent photoFade;

		public SwitcherComponent buttonState;

		public FadeResizeComponent buttonFade;

		private string m_last_url = "";

		public bool isSaving
		{
			get
			{
				return buttonState.index == 1;
			}
			set
			{
				buttonState.index = (value ? 1 : 0);
				buttonFade.Fade(value ? 1f : 0f, 0.4f);
			}
		}

		public void SetDefaultImage(Texture p_texture)
		{
			defaultImage.texture = p_texture;
			defaultImage.enabled = p_texture != null;
		}

		public void SetImage(Texture p_texture)
		{
			photoFade.FadeOut(0.25f);
			if ((bool)p_texture)
			{
				Activity.RunOnce(delegate
				{
					photoImage.texture = p_texture;
					photoFade.FadeIn(0.25f);
				}, 0.26f);
			}
		}

		public void LoadImage(string p_url, bool p_fade = false)
		{
			if (string.IsNullOrEmpty(p_url) || m_last_url == p_url)
			{
				return;
			}
			m_last_url = p_url;
			if (p_fade)
			{
				photoFade.alpha = -0.1f;
			}
			base.app.model.service.GetImage(p_url, 480, -1, delegate(Texture2D p_res)
			{
				if ((bool)this && (bool)p_res)
				{
					SetImage(p_res);
				}
			});
		}
	}
}
