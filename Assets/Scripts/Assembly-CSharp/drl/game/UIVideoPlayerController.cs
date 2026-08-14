using UnityEngine;
using UnityEngine.Video;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIVideoPlayerController : Controller<DRLApp>
	{
		private bool m_videoIsPreparing;

		private WebAsyncRequest m_photo_loader;

		public UIVideoPlayerView view => AssertLocal<UIVideoPlayerView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@close":
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.pathFade.FadeIn(0.2f);
				}
				if ((bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.Show(0.2f);
				}
				if (view.imageOnly)
				{
					view.imageOnly = false;
					view.backgroundImage = null;
				}
				else
				{
					StopBuffering();
					StopVideo();
				}
				break;
			case "ui.screen@open":
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				view.videoFade.FadeOut(0f);
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.pathFade.FadeOut(0.2f);
				}
				if ((bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.Hide(0.2f);
				}
				if (view.imageOnly)
				{
					LoadBackgroundImage();
					view.videoPlayer.enabled = false;
					break;
				}
				view.videoPlayer.enabled = true;
				m_videoIsPreparing = false;
				view.PlaySpinner();
				if (string.IsNullOrEmpty(view.VideoURL))
				{
					view.statusText.text = "URL INCORRECT...";
					Activity.RunOnce(delegate
					{
						view.videoFade.FadeOut(0.2f);
						base.app.view.ui.screens.Return();
					}, 2f);
				}
				else
				{
					Load(view.VideoURL);
				}
				break;
			case "ui.screen.return@click":
				if (view.imageOnly)
				{
					view.imageOnly = false;
					view.backgroundImage = null;
				}
				else
				{
					StopBuffering();
					StopVideo();
				}
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen.video-player-close@click":
				CloseVideoPlayerScreen();
				break;
			}
		}

		public void Load(string mp4_url)
		{
			if (!m_videoIsPreparing)
			{
				view.videoPlayer.errorReceived -= videoErrorHandler;
				view.videoPlayer.errorReceived += videoErrorHandler;
				view.videoPlayer.prepareCompleted -= prepareCompleted;
				view.videoPlayer.prepareCompleted += prepareCompleted;
				view.videoPlayer.loopPointReached -= EndReached;
				view.videoPlayer.loopPointReached += EndReached;
				view.videoPlayer.url = mp4_url;
				view.videoPlayer.Prepare();
				view.statusText.text = base.app.model.storage.locale.Get("ui.common.loading-w-dots", "LOADING...");
				m_videoIsPreparing = true;
			}
		}

		private void EndReached(VideoPlayer vp)
		{
			CloseVideoPlayerScreen();
		}

		private void prepareCompleted(VideoPlayer p_source)
		{
			view.StopSpinner();
			ShowVideo();
			m_videoIsPreparing = false;
		}

		private void videoErrorHandler(VideoPlayer p_source, string msg)
		{
			view.statusText.text = "COULD NOT LOAD VIDEO";
			Activity.RunOnce(delegate
			{
				StopBuffering();
				StopVideo();
				base.app.view.ui.screens.Return();
			}, 2f);
		}

		private void StopBuffering()
		{
			if (m_videoIsPreparing)
			{
				m_videoIsPreparing = false;
				view.videoPlayer.Stop();
			}
		}

		public void ShowVideo()
		{
			view.videoFade.FadeIn(0.2f);
			view.videoPlayer.Play();
		}

		public void StopVideo()
		{
			if (view.videoPlayer.isPlaying)
			{
				view.videoFade.FadeOut(0.2f);
				view.videoPlayer.Stop();
			}
		}

		private void CloseVideoPlayerScreen()
		{
			StopBuffering();
			StopVideo();
			base.app.view.ui.screens.Return();
			base.app.view.ui.SetDark(p_flag: false);
			if (base.app.level.IsLevelLoaded("game"))
			{
				base.app.controller.game.FadeBlur(0f, 0f);
			}
			Notify("ui.screen.video-player@end", view.VideoURL);
		}

		private void LoadBackgroundImage()
		{
			if (string.IsNullOrEmpty(view.ImageURL))
			{
				return;
			}
			view.backgroundImageFade.Fade(0f, 0.001f);
			m_photo_loader = Web.Load(view.ImageURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_result == null))
				{
					view.backgroundImageFade.FadeIn();
					view.backgroundImage = p_result;
				}
			});
		}
	}
}
