using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class UIUVScroll : ActivityBehaviour, IUpdateable
	{
		[SerializeField]
		private Graphic m_target;

		[SerializeField]
		private Vector2 m_speed;

		[SerializeField]
		private Vector2 m_offset;

		public bool useTimescale = true;

		private Rect m_uv;

		private Vector2 m_uv_pos;

		private bool m_has_target;

		public Graphic target
		{
			get
			{
				Graphic graphic = (m_target ? m_target : (m_target = GetComponent<Graphic>()));
				m_has_target = graphic != null;
				return graphic;
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
			Graphic graphic = target;
			m_uv = GetRect();
			m_uv_pos = new Vector2(m_uv.x, m_uv.y);
			m_has_target = graphic != null;
		}

		protected Rect GetRect()
		{
			Graphic graphic = target;
			if (!graphic)
			{
				return new Rect(0f, 0f, 1f, 1f);
			}
			if (graphic is RawImage)
			{
				return ((RawImage)graphic).uvRect;
			}
			if (graphic is ImageHDR)
			{
				return ((ImageHDR)graphic).bloomTextureRect;
			}
			return new Rect(0f, 0f, 1f, 1f);
		}

		protected void SetRect(Rect r)
		{
			Graphic graphic = target;
			if ((bool)graphic)
			{
				if (graphic is RawImage)
				{
					((RawImage)graphic).uvRect = r;
				}
				if (graphic is ImageHDR)
				{
					((ImageHDR)graphic).bloomTextureRect = r;
				}
			}
		}

		public void OnUpdate()
		{
			if (base.gameObject.activeInHierarchy && m_has_target)
			{
				float num = (useTimescale ? Time.deltaTime : Time.unscaledDeltaTime);
				Rect uv = m_uv;
				uv.x += offset.x;
				uv.y += offset.y;
				uv.x -= m_uv_pos.x;
				uv.y -= m_uv_pos.y;
				m_uv_pos += speed * num;
				SetRect(uv);
			}
		}
	}
}
