using System;
using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(ImageLayout))]
	public class AnimateImageLayout : MonoBehaviour
	{
		public enum AnimationType
		{
			None = 0,
			OscilateVertical = 1
		}

		public AnimationType animationType;

		public Vector2 range = new Vector2(-0.5f, 0.5f);

		public float speed = 1f;

		public float scale = 10f;

		private ImageLayout m_layout;

		private float m_animation_step;

		public ImageLayout layout
		{
			get
			{
				if (!m_layout)
				{
					return m_layout = GetComponent<ImageLayout>();
				}
				return m_layout;
			}
		}

		protected void Start()
		{
			m_layout = layout;
		}

		protected void Update()
		{
			AnimationType animationType = this.animationType;
			if (animationType != AnimationType.None && animationType == AnimationType.OscilateVertical)
			{
				m_animation_step += Time.deltaTime * speed;
				float t = Mathf.Sin(Mathf.Max(0f, m_animation_step) * ((float)Math.PI / 180f) * scale) * 0.5f + 0.5f;
				layout.offset.y = Mathf.Lerp(range.x, range.y, t);
			}
		}

		public void ResetAnimation()
		{
			m_animation_step = 0f;
			layout.offset = Vector2.zero;
			animationType = AnimationType.None;
		}
	}
}
