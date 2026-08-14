using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITryoutsOnboardingView : UIScreenView
	{
		public DRLToggleView acceptTermsToggle;

		public UINavigation nextButtonNav;

		public GameObject qualifiedButton;

		public UINavigation registerButtonNav;

		public UINavigation rulesButtonNav;

		public UINavigation resultsButtonNav;

		public FadeComponent registerButtonFade;

		[Header("Links")]
		public string tryoutsURL = "https://thedroneracingleague.com/tryouts/";

		public string tryoutsBannerTopURL = "https://thedroneracingleague.com/tryouts/";

		public string tryoutsBannerBottomURL = "https://thedroneracingleague.com/tryouts/";

		[Header("Banners")]
		public FadeComponent bannerTopFade;

		public RawImage bannerTopField;

		public RectTransform bannerTopSpinner;

		public FadeComponent bannerBottomFade;

		public RawImage bannerBottomField;

		public RectTransform bannerBottomSpinner;

		public UIElementView playButtonView;

		public DRLCampaign data;

		public string VideoURL;

		public VideoPlayer videoPlayer;

		public FadeComponent videoFade;

		public bool isAcceptTerms
		{
			get
			{
				if (!acceptTermsToggle.toggle)
				{
					return true;
				}
				return acceptTermsToggle.toggle.isOn;
			}
			set
			{
				if ((bool)acceptTermsToggle.toggle)
				{
					acceptTermsToggle.toggle.isOn = value;
				}
			}
		}

		public void Set(DRLCampaign p_data)
		{
		}

		public void EnableRegisterButton(bool p_enable)
		{
			if ((bool)registerButtonFade)
			{
				registerButtonFade.Fade(p_enable ? 1f : 0.1f, 0f);
			}
		}

		public bool IsRegisterButtonEnabled()
		{
			return registerButtonFade.alpha > 0.9f;
		}

		public void SetRegisterAvailable(bool p_flag)
		{
			registerButtonNav.gameObject.SetActive(!p_flag);
			acceptTermsToggle.gameObject.SetActive(!p_flag);
			RefreshNavigation();
		}

		public void SetTryoutsEnabled(bool p_flag)
		{
			nextButtonNav.gameObject.SetActive(p_flag);
			registerButtonNav.gameObject.SetActive(p_flag);
			acceptTermsToggle.gameObject.SetActive(p_flag);
			RefreshNavigation();
		}

		public void SetPlayEnabled(bool p_flag)
		{
			nextButtonNav.GetComponent<UIElementView>().interactable = p_flag;
		}

		public void RefreshNavigation()
		{
			bool activeInHierarchy = registerButtonNav.gameObject.activeInHierarchy;
			rulesButtonNav.down = (activeInHierarchy ? registerButtonNav : resultsButtonNav);
			resultsButtonNav.up = (activeInHierarchy ? registerButtonNav : rulesButtonNav);
		}
	}
}
