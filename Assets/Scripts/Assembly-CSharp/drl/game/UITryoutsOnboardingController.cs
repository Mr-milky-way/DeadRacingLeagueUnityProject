using UnityEngine;
using UnityEngine.Video;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITryoutsOnboardingController : Controller<DRLApp>
	{
		private bool m_preparingVideo = true;

		public string mapGuid;

		public DroneRigData droneRig;

		public string podiumGuid;

		private DRLCircuitData m_circuitData;

		public UITryoutsOnboardingView view => AssertLocal<UITryoutsOnboardingView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				DRLCampaign data = view.data;
				view.Set(data);
				CampaignRegisterInfo registerInfo = base.app.model.storage.state.player.results.campaign.GetRegisterInfo(data);
				if (registerInfo != null)
				{
					_ = registerInfo.guid != data.guid;
				}
				view.nextButtonNav.gameObject.SetActive(value: true);
				view.RefreshNavigation();
				view.bannerTopSpinner.gameObject.SetActive(value: false);
				if (!view.bannerTopField.texture)
				{
					view.bannerTopSpinner.gameObject.SetActive(value: true);
					Web.Get("tryouts-banner-top", view.tryoutsBannerTopURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
					{
						if (!(p_progress < 1f) && (bool)p_result)
						{
							view.bannerTopSpinner.gameObject.SetActive(value: false);
							view.bannerTopField.texture = p_result;
							((RectTransform)view.bannerTopField.transform.parent).sizeDelta = new Vector2(p_result.width, p_result.height);
							view.bannerTopFade.FadeIn(1f, 0.2f);
						}
					});
				}
				view.bannerBottomSpinner.gameObject.SetActive(value: false);
				if (!view.bannerBottomField.texture)
				{
					view.bannerBottomSpinner.gameObject.SetActive(value: true);
					Web.Get("tryouts-banner-bottom", view.tryoutsBannerBottomURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
					{
						if (!(p_progress < 1f))
						{
							view.bannerBottomSpinner.gameObject.SetActive(value: false);
							view.bannerBottomField.texture = p_result;
							((RectTransform)view.bannerBottomField.transform.parent).sizeDelta = new Vector2(p_result.width, p_result.height);
							view.bannerBottomFade.FadeIn(1f, 0.2f);
						}
					});
				}
				m_circuitData = base.app.model.storage.state.player.circuits.GetTryoutsCircuit();
				view.playButtonView.interactable = m_circuitData != null || DRLApp.offline;
				view.videoPlayer.url = view.VideoURL;
				break;
			}
			case "campaign.tryouts.leaders@click":
			{
				UITryoutsLeadersView uITryoutsLeadersView2 = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaders-screen");
				uITryoutsLeadersView2.data = view.data;
				uITryoutsLeadersView2.AllowNext(p_flag: false);
				StopVideo();
				break;
			}
			case "campaign.tryouts.results@click":
				WebBrowser.OpenURL("https://thedroneracingleague.com/drl-sim-3/tryouts", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "campaign.tryouts.onboarding.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "campaign.tryouts.onboarding.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "ui.screen.nav-right@click":
				switch ((p_target as UIElementView).name)
				{
				case "next":
					RunOnce(1f / 30f, delegate
					{
						UITryoutsLeadersView uITryoutsLeadersView3 = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaders-screen");
						uITryoutsLeadersView3.data = view.data;
						uITryoutsLeadersView3.AllowNext(p_flag: true);
					});
					break;
				case "register":
					if (view.IsRegisterButtonEnabled())
					{
						base.app.view.ui.screens.Open<UITryoutsRegisterView>("tryouts-register-screen").data = view.data;
					}
					break;
				case "leaders":
				{
					UITryoutsLeadersView uITryoutsLeadersView = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaders-screen");
					uITryoutsLeadersView.data = view.data;
					uITryoutsLeadersView.AllowNext(p_flag: true);
					break;
				}
				case "rules":
					WebBrowser.OpenURL("https://thedroneracingleague.com/official-rules-tryouts-2020/", (base.app != null) ? base.app.model.service.platform : null);
					break;
				case "play":
				{
					if (m_circuitData == null || DRLApp.offline)
					{
						view.playButtonView.interactable = false;
						break;
					}
					UICircuitOverviewView uICircuitOverviewView = base.app.view.ui.screens.Open<UICircuitOverviewView>("circuits-overview-screen");
					if (m_circuitData != null)
					{
						uICircuitOverviewView.circuitData = m_circuitData;
					}
					uICircuitOverviewView.caller = this;
					break;
				}
				}
				break;
			case "ui.screen.return@click":
				StopVideo();
				base.app.view.ui.screens.Return();
				break;
			}
		}

		private void VideoPrepared(VideoPlayer source)
		{
			m_preparingVideo = false;
		}

		private void StartVideo()
		{
			view.videoPlayer.Play();
			view.videoFade.FadeIn();
		}

		private void StopVideo()
		{
			view.videoFade.FadeOut();
			view.videoPlayer.Stop();
		}

		protected void OnFormNotification(Object p_target, bool p_is_change)
		{
			string text = p_target.name;
			CampaignResultsModel campaign = base.app.model.storage.state.player.results.campaign;
			switch (text)
			{
			case "tryouts-banner-top":
				WebBrowser.OpenURL(view.tryoutsURL, (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "accept-terms":
				campaign.SetTermsAccept(view.data, view.isAcceptTerms);
				view.EnableRegisterButton(view.isAcceptTerms);
				break;
			case "video-player":
				if (view.videoPlayer.isPlaying)
				{
					StopVideo();
				}
				else
				{
					StartVideo();
				}
				break;
			case "video-caption":
				WebBrowser.OpenURL("https://www.youtube.com/watch?v=Xri08Ya1Zhg", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "tryouts-banner-bottom":
				break;
			}
		}
	}
}
