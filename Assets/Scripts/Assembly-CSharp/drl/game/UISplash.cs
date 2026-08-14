using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UISplash : UIScreenView
	{
		public CanvasGroup content;

		public CanvasGroup logoField;

		public FadeComponent videoFade;

		public VideoComponent video;

		public Text versionField;

		public FadeComponent spinnerFade;

		public Image spinnerTimeoutProgressField;

		public Text spinnerField;

		public ImageClip spinnerClip;

		public FadeComponent quitButton;

		public FadeComponent offlineButton;

		public FadeComponent connectionCheckButton;

		public GameObject connectionCheckXboxIcon;

		public GameObject connectionCheckPlaystationIcon;

		public FadeComponent connectionRetryButton;

		public GameObject connectionRetryXboxIcon;

		public GameObject connectionRetryPlaystationIcon;

		public float spinnerTimeoutProgress
		{
			get
			{
				if (!spinnerTimeoutProgressField)
				{
					return 0f;
				}
				return spinnerTimeoutProgressField.fillAmount;
			}
			set
			{
				if ((bool)spinnerTimeoutProgressField)
				{
					spinnerTimeoutProgressField.fillAmount = value;
				}
			}
		}

		public string spinnerText
		{
			set
			{
				if ((bool)spinnerField)
				{
					spinnerField.text = value;
				}
			}
		}

		public string version
		{
			set
			{
				versionField.text = value;
			}
		}

		public void StopClip()
		{
			if ((bool)spinnerClip)
			{
				spinnerClip.loop = false;
				spinnerClip.infinite = false;
			}
		}

		public void PlayClip()
		{
			if ((bool)spinnerClip)
			{
				spinnerClip.loop = true;
				spinnerClip.infinite = true;
				spinnerClip.Stop();
				spinnerClip.Play();
			}
		}

		protected void Awake()
		{
		}

		public void SetErrorLayout(bool p_flag)
		{
			connectionCheckButton.gameObject.SetActive(p_flag);
			connectionRetryButton.gameObject.SetActive(p_flag);
			quitButton.Fade(p_flag ? 1f : (-0.1f), 1f, 0.1f, Cubic.Out);
			connectionCheckButton.Fade(p_flag ? 1f : (-0.1f), 1f, 0.1f, Cubic.Out);
			connectionRetryButton.Fade(p_flag ? 1f : (-0.1f), 1f, 0.1f, Cubic.Out);
			connectionCheckXboxIcon.SetActive(value: true);
			connectionRetryXboxIcon.SetActive(value: true);
			bool flag = true;
			quitButton.gameObject.SetActive(p_flag && flag);
		}

		public void SetOfflineLayout(bool p_flag)
		{
			quitButton.gameObject.SetActive(p_flag);
			quitButton.Fade(p_flag ? 1f : (-0.1f), 1f, 0.1f, Cubic.Out);
			offlineButton.gameObject.SetActive(p_flag);
			offlineButton.Fade(p_flag ? 1f : (-0.1f), 1f, 0.1f, Cubic.Out);
		}
	}
}
