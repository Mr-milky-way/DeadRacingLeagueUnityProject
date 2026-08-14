using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class RawImageUVScroll : MonoBehaviour
	{
		[SerializeField]
		private RawImage m_target;

		[SerializeField]
		private Vector2 m_speed;

		[SerializeField]
		private Vector2 m_offset;

		public bool useTimescale = true;

		private Rect m_uv;

		private bool m_has_target;

		public RawImage target
		{
			get
			{
				RawImage rawImage = (m_target ? m_target : (m_target = GetComponent<RawImage>()));
				m_has_target = rawImage != null;
				return rawImage;
			}
			set
			{
				m_target = value;
				m_has_target = value != null;
			}
		}

		public Vector2 speed
		{
			get
			{
				return m_speed;
			}
			set
			{
				m_speed = value;
			}
		}

		public Vector2 offset
		{
			get
			{
				return m_offset;
			}
			set
			{
				m_offset = value;
			}
		}

		protected void Awake()
		{
			RawImage rawImage = target;
			m_uv = new Rect(0f, 0f, 1f, 1f);
			if ((bool)rawImage)
			{
				m_uv = rawImage.uvRect;
			}
			m_has_target = rawImage != null;
		}

		protected void Update()
		{
			if (base.enabled && base.gameObject.activeInHierarchy && m_has_target)
			{
				float num = (useTimescale ? Time.deltaTime : Time.unscaledDeltaTime);
				Vector2 vector = new Vector2(m_uv.x, m_uv.y);
				vector += offset;
				vector -= speed * num;
				m_uv.x = vector.x;
				m_uv.y = vector.y;
				target.uvRect = m_uv;
			}
		}
	}
}
