using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;

namespace drl.game
{
	public class UIVideoPlayerView : UIScreenView
	{
		public bool imageOnly;

		public VideoPlayer videoPlayer;

		public Image backgroundImageField;

		public FadeComponent backgroundImageFade;

		public FadeComponent videoFade;

		public FadeComponent spinnerFade;

		public ImageClip spinnerClip;

		public Text statusText;

		public string VideoURL;

		public string ImageURL;

		public GameObject backButton;

		public GameObject closeButton;

		public Texture2D backgroundImage
		{
			set
			{
				if (value != null)
				{
					backgroundImageField.sprite = Sprite.Create(value, new Rect(0f, 0f, value.width, value.height), new Vector2(0.5f, 0.5f));
					backgroundImageField.color = Color.white;
				}
				else
				{
					backgroundImageField.color = Color.black;
					backgroundImageField.sprite = null;
				}
			}
		}

		public void PlaySpinner()
		{
			if ((bool)spinnerClip)
			{
				spinnerFade.FadeIn(0.2f);
				spinnerClip.Play();
			}
		}

		public void StopSpinner()
		{
			if ((bool)spinnerClip)
			{
				spinnerClip.Stop();
				spinnerFade.FadeOut(0.2f);
			}
		}

		public void ToggleBackCloseButton()
		{
			backButton.SetActive(!backButton.activeInHierarchy);
			closeButton.SetActive(!closeButton.activeInHierarchy);
		}
	}
}
