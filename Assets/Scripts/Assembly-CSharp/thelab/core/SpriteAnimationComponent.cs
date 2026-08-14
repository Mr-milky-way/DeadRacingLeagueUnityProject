using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[RequireComponent(typeof(Image))]
	public class SpriteAnimationComponent : MonoBehaviour
	{
		public Sprite[] sprites;

		public float animationDuration;

		public bool loop;

		public bool playOnAwake;

		private bool m_running;

		private float m_frameDuration;

		private float m_slideShowTimer;

		private int m_frameIndex;

		private Image preview;

		private Activity m_slideShowActivity;

		private void OnEnable()
		{
			preview = GetComponent<Image>();
			if (playOnAwake)
			{
				StartAnimation();
			}
		}

		private void OnDisable()
		{
			m_running = false;
			m_slideShowActivity.Stop();
		}

		public void StartAnimation()
		{
			preview.enabled = true;
			if (!sprites.Any())
			{
				Debug.LogWarning("<SpriteAnimationComponent> No sprites assigned!");
				return;
			}
			m_frameDuration = animationDuration / (float)sprites.Length;
			m_running = true;
			Play();
		}

		public void StopAnimation()
		{
			m_running = false;
			preview.enabled = false;
		}

		private void Play()
		{
			float timer = 0f;
			m_slideShowActivity = Activity.Run(delegate(Activity a)
			{
				if (!m_running || (m_frameIndex == sprites.Length && !loop))
				{
					a.Stop();
				}
				if (m_frameIndex == sprites.Length)
				{
					m_frameIndex = 0;
				}
				preview.sprite = sprites[m_frameIndex];
				timer += Time.deltaTime;
				if (timer > m_frameDuration)
				{
					timer = 0f;
					m_frameIndex++;
				}
			});
		}
	}
}
