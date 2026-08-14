using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[RequireComponent(typeof(RawImage))]
	[ExecuteInEditMode]
	public class ImageLayout : ActivityBehaviour, ILateUpdateable
	{
		public enum Aligment
		{
			TopLeft = 0,
			TopCenter = 1,
			TopRight = 2,
			Left = 3,
			Center = 4,
			Right = 5,
			BottomLeft = 6,
			BottomCenter = 7,
			BottomRight = 8,
			Custom = 9
		}

		public enum Mode
		{
			None = 0,
			Clip = 1,
			ScaleToFit = 2,
			Fit = 3
		}

		public Aligment align;

		public Mode mode;

		public Vector2 offset;

		public Vector2 scale = Vector2.one;

		public bool deferred = true;

		private RawImage m_image;

		private RectTransform _rt;

		private Texture m_last_texture;

		private Aligment m_last_align;

		private Mode m_last_mode;

		private Vector2 m_last_size;

		private Vector2 m_last_texsize;

		private Vector2 m_last_offset;

		private Vector2 m_last_scale;

		private Rect m_last_rect;

		private static int m_render_id;

		private int m_render_iterator;

		private int m_id;

		public RawImage image
		{
			get
			{
				if (!m_image)
				{
					return m_image = GetComponent<RawImage>();
				}
				return m_image;
			}
		}

		protected RectTransform m_rt
		{
			get
			{
				if (!_rt)
				{
					return _rt = GetComponent<RectTransform>();
				}
				return _rt;
			}
		}

		protected void Start()
		{
			m_image = image;
			Refresh(p_force: true);
			m_id = m_render_id;
			m_render_id = (m_render_id + 1) % 3;
		}

		public void OnLateUpdate()
		{
			if (Application.isPlaying)
			{
				m_render_iterator = (m_render_iterator + 1) % 3;
				if (!deferred || m_render_iterator == m_id)
				{
					Refresh();
				}
			}
		}

		public virtual void Refresh(bool p_force = false)
		{
			if (!base.gameObject || !base.gameObject.activeInHierarchy || (!Application.isPlaying && !image) || !m_image)
			{
				return;
			}
			RawImage rawImage = m_image;
			RectTransform rt = m_rt;
			Vector2 sizeDelta = rt.sizeDelta;
			if ((rt.anchorMin - rt.anchorMax).magnitude > 0f)
			{
				sizeDelta.x = rt.rect.width;
				sizeDelta.y = rt.rect.height;
			}
			Texture texture = ((!rawImage) ? null : (rawImage.texture ? rawImage.texture : null));
			Vector2 last_texsize = default(Vector2);
			if (texture != null)
			{
				last_texsize.x = texture.width;
				last_texsize.y = texture.height;
			}
			if (!p_force && m_last_align == align && m_last_mode == mode && m_last_texture == texture && Mathf.Abs(m_last_size.x - sizeDelta.x) <= 0f && Mathf.Abs(m_last_size.y - sizeDelta.y) <= 0f && Mathf.Abs(m_last_texsize.x - last_texsize.x) <= 0f && Mathf.Abs(m_last_texsize.y - last_texsize.y) <= 0f && Mathf.Abs(m_last_offset.x - offset.x) <= 0f && Mathf.Abs(m_last_offset.y - offset.y) <= 0f && Mathf.Abs(m_last_scale.x - scale.x) <= 0f && Mathf.Abs(m_last_scale.y - scale.y) <= 0f && Mathf.Abs(m_last_rect.xMin - rawImage.uvRect.xMin) <= 0f && Mathf.Abs(m_last_rect.xMax - rawImage.uvRect.xMax) <= 0f && Mathf.Abs(m_last_rect.yMin - rawImage.uvRect.yMin) <= 0f && Mathf.Abs(m_last_rect.yMax - rawImage.uvRect.yMax) <= 0f)
			{
				return;
			}
			m_last_align = align;
			m_last_mode = mode;
			m_last_texture = texture;
			m_last_rect = image.uvRect;
			m_last_size = sizeDelta;
			m_last_offset = offset;
			m_last_scale = scale;
			m_last_texsize = last_texsize;
			if (texture == null)
			{
				return;
			}
			Rect uvRect = new Rect(0f, 0f, 1f, 1f);
			float num = Mathf.Max(1f, sizeDelta.x);
			float num2 = Mathf.Max(1f, sizeDelta.y);
			float num3 = Mathf.Max(1, (!(texture == null)) ? texture.width : 0);
			float num4 = Mathf.Max(1, (!(texture == null)) ? texture.height : 0);
			float num5 = num / num3;
			float num6 = num2 / num4;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = num5;
			float num10 = num6;
			num7 = 1f - num5;
			num8 = 1f - num6;
			switch (mode)
			{
			case Mode.ScaleToFit:
				if (num7 < num8)
				{
					num9 = 1f;
					num10 = num6 / num5;
				}
				else
				{
					num10 = 1f;
					num9 = num5 / num6;
				}
				break;
			case Mode.Fit:
				if (num5 < num6)
				{
					num9 = 1f;
					num10 = num6 / num5;
				}
				else
				{
					num10 = 1f;
					num9 = num5 / num6;
				}
				break;
			case Mode.None:
				num9 = rawImage.uvRect.width;
				num10 = rawImage.uvRect.height;
				break;
			}
			float num11 = num9 * num;
			float num12 = num10 * num2;
			float num13 = 1f / num;
			float num14 = 1f / num2;
			float num15 = num11 - num;
			float num16 = num12 - num2;
			float num17 = num15 * num13;
			float num18 = num16 * num14;
			float num19 = 0f;
			float num20 = 0f;
			uvRect.width = num9;
			uvRect.height = num10;
			switch (align)
			{
			case Aligment.TopLeft:
				num19 = 0f;
				num20 = 1f;
				break;
			case Aligment.TopCenter:
				num19 = 0.5f;
				num20 = 1f;
				break;
			case Aligment.TopRight:
				num19 = 1f;
				num20 = 1f;
				break;
			case Aligment.Left:
				num19 = 0f;
				num20 = 0.5f;
				break;
			case Aligment.Center:
				num19 = 0.5f;
				num20 = 0.5f;
				break;
			case Aligment.Right:
				num19 = 1f;
				num20 = 0.5f;
				break;
			case Aligment.BottomLeft:
				num19 = 0f;
				num20 = 0f;
				break;
			case Aligment.BottomCenter:
				num19 = 0.5f;
				num20 = 0f;
				break;
			case Aligment.BottomRight:
				num19 = 1f;
				num20 = 0f;
				break;
			}
			uvRect.x = (0f - num17) * (num19 + offset.x);
			uvRect.y = (0f - num18) * (num20 + offset.y);
			float num21 = uvRect.xMin + uvRect.width * 0.5f;
			float num22 = uvRect.yMin + uvRect.height * 0.5f;
			uvRect.x -= num21;
			uvRect.y -= num22;
			float num23 = ((Mathf.Abs(scale.x) <= 1E-06f) ? 0f : (1f / scale.x));
			float num24 = ((Mathf.Abs(scale.y) <= 1E-06f) ? 0f : (1f / scale.y));
			uvRect.xMin *= num23;
			uvRect.xMax *= num23;
			uvRect.yMin *= num24;
			uvRect.yMax *= num24;
			uvRect.x += num21;
			uvRect.y += num22;
			rawImage.uvRect = uvRect;
		}
	}
}
