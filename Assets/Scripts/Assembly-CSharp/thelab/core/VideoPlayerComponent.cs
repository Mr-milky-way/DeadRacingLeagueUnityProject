using UnityEngine;
using UnityEngine.Video;

namespace thelab.core
{
	[RequireComponent(typeof(VideoPlayer))]
	public class VideoPlayerComponent : VideoComponent<VideoPlayer>
	{
		private bool m_will_loop;

		public new bool loop
		{
			get
			{
				if (!video)
				{
					return false;
				}
				return video.isLooping;
			}
		}

		protected override void Initialize()
		{
			video = GetComponent<VideoPlayer>();
			m_will_loop = false;
			if ((bool)video)
			{
				video.loopPointReached -= OnVideoLoop;
				video.loopPointReached += OnVideoLoop;
			}
		}

		protected void OnVideoLoop(VideoPlayer p_video)
		{
			m_will_loop = true;
		}

		protected override void PlayVideo()
		{
			if ((bool)video)
			{
				video.Play();
			}
		}

		protected override void StopVideo()
		{
			if ((bool)video)
			{
				video.Stop();
			}
		}

		protected override void PlayAudio()
		{
			if ((bool)base.audio && base.audio.enabled)
			{
				m_audio.Play();
			}
		}

		protected override void StopAudio()
		{
			if ((bool)base.audio && base.audio.enabled)
			{
				m_audio.Stop();
			}
		}

		protected override void LoopVideo()
		{
			m_will_loop = false;
			_ = loop;
		}

		protected override bool IsPlaying()
		{
			if (!video)
			{
				return false;
			}
			if (!video)
			{
				return false;
			}
			if (video.isPlaying)
			{
				return !m_will_loop;
			}
			return false;
		}
	}
}
