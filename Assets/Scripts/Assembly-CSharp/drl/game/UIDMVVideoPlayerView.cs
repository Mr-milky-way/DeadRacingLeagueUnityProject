using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;

namespace drl.game
{
	public class UIDMVVideoPlayerView : UIScreenView
	{
		public VideoPlayer videoPlayer;

		public FadeComponent videoFade;

		public Text statusText;

		public void ShowVideo()
		{
			if (videoPlayer.clip == null)
			{
				statusText.text = "NO CLIP!";
				return;
			}
			videoFade.FadeIn(0f);
			videoPlayer.Play();
		}

		public void StopVideo()
		{
			if (videoPlayer.isPlaying)
			{
				videoFade.FadeOut(0.2f);
				videoPlayer.Stop();
			}
		}
	}
}
