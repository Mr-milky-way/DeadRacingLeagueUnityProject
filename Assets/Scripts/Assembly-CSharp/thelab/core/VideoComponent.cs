using UnityEngine;

namespace thelab.core
{
	public class VideoComponent : MonoBehaviour
	{
		public bool playOnAwake;

		public bool allowSkip;

		public float skipDelay;

		public bool loop;

		public VideoComponentCallback OnEvent;

		protected bool m_is_playing;

		[SerializeField]
		protected AudioSource m_audio;

		public AudioSource audio
		{
			get
			{
				if (!m_audio)
				{
					return m_audio = GetComponent<AudioSource>();
				}
				return m_audio;
			}
		}

		public virtual void Play()
		{
		}

		public virtual void Stop()
		{
		}

		protected virtual void Initialize()
		{
		}

		protected virtual void PlayVideo()
		{
		}

		protected virtual void StopVideo()
		{
		}

		protected virtual void LoopVideo()
		{
		}

		protected virtual void PlayAudio()
		{
		}

		protected virtual void StopAudio()
		{
		}

		protected virtual bool IsPlaying()
		{
			return false;
		}

		protected void Awake()
		{
			m_is_playing = false;
			Initialize();
			if (playOnAwake)
			{
				PlayVideo();
			}
		}

		public void Skip(float p_delay = 0f)
		{
			if (m_is_playing)
			{
				m_is_playing = false;
				OnEvent.Invoke(VideoEventType.Skip);
				if (p_delay <= 0f)
				{
					Stop();
				}
				else
				{
					Activity.RunOnce(Stop, p_delay);
				}
			}
		}

		protected virtual void Update()
		{
			if (Input.anyKeyDown && allowSkip && m_is_playing)
			{
				Skip(skipDelay);
			}
			else if (IsPlaying())
			{
				if (!m_is_playing)
				{
					m_is_playing = true;
					OnEvent.Invoke(VideoEventType.Start);
				}
				else
				{
					OnEvent.Invoke(VideoEventType.Update);
				}
			}
			else if (m_is_playing)
			{
				m_is_playing = false;
				LoopVideo();
				OnEvent.Invoke(VideoEventType.Complete);
			}
		}
	}
	public class VideoComponent<T> : VideoComponent where T : Object
	{
		public T video;

		public override void Play()
		{
			StopVideo();
			StopAudio();
			PlayVideo();
			PlayAudio();
		}

		public override void Stop()
		{
			if (m_is_playing)
			{
				m_is_playing = false;
				StopVideo();
				StopAudio();
				OnEvent.Invoke(VideoEventType.Stop);
			}
		}

		protected override void Update()
		{
			if (!video)
			{
				if (m_is_playing)
				{
					Stop();
				}
			}
			else
			{
				base.Update();
			}
		}
	}
}
