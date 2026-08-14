using UnityEngine;
using UnityEngine.Video;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVVideoPlayerController : Controller<DRLApp>
	{
		public UIDMVVideoPlayerView view => AssertLocal<UIDMVVideoPlayerView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					if ((bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.pathFade.FadeOut(0.2f);
					}
					if ((bool)base.app.view.ui.footer)
					{
						base.app.view.ui.footer.Hide(0.2f);
					}
					view.videoPlayer.loopPointReached += EndReached;
					view.ShowVideo();
				}
				break;
			case "ui.screen.video-player-close@click":
				CloseVideoPlayerScreen();
				break;
			}
		}

		private void EndReached(VideoPlayer source)
		{
			CloseVideoPlayerScreen();
		}

		private void CloseVideoPlayerScreen()
		{
			view.screen.Hide();
			view.StopVideo();
			base.app.view.ui.SetDark(p_flag: false);
			base.app.controller.game.FadeBlur(0f, 0f);
			Notify("ui.screen.video-player@end", view.videoPlayer.clip);
		}

		private void OnDisable()
		{
			view.StopVideo();
		}
	}
}
