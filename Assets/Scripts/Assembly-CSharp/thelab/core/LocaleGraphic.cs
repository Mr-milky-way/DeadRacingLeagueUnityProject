using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class LocaleGraphic : LocaleElement
	{
		[SerializeField]
		private Graphic m_target;

		private static Dictionary<string, Sprite> m_sprite_cache;

		public Graphic target
		{
			get
			{
				return m_target ?? (m_target = GetComponent<Graphic>());
			}
			set
			{
				m_target = value;
			}
		}

		public override void OnLocaleRefresh()
		{
			if (!target || keys.Count <= 0)
			{
				return;
			}
			if (m_sprite_cache == null)
			{
				m_sprite_cache = new Dictionary<string, Sprite>();
			}
			string text = keys[0];
			Image image = target as Image;
			RawImage rawImage = target as RawImage;
			if ((bool)image)
			{
				Image image2 = image;
				Sprite sprite = image2.sprite;
				Texture texture = (sprite ? sprite.texture : null);
				Texture2D texture2D = base.manager.Get(text, texture as Texture2D);
				if (m_sprite_cache.ContainsKey(text))
				{
					image2.sprite = m_sprite_cache[text];
				}
				else
				{
					image2.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
					m_sprite_cache[text] = image2.sprite;
				}
			}
			if ((bool)rawImage)
			{
				Texture texture = rawImage.texture;
				Texture2D texture2 = base.manager.Get(text, texture as Texture2D);
				rawImage.texture = texture2;
			}
		}
	}
}
