using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatBlockedUserView : View<DRLApp>
	{
		internal static Dictionary<string, Texture> m_cache;

		[SerializeField]
		public Image background;

		public GameObject focus;

		public GameObject photoContainer;

		public RawImage photoField;

		public Image colorField;

		public Text usernameField;

		private string m_title;

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		private Texture photo
		{
			set
			{
				if (!(photoField == null))
				{
					photoField.texture = value;
				}
			}
		}

		public Color userColor
		{
			set
			{
				if (!(colorField == null))
				{
					colorField.color = value;
				}
			}
		}

		public string title
		{
			get
			{
				if (!usernameField)
				{
					return m_title;
				}
				return usernameField.text;
			}
			set
			{
				if (usernameField != null)
				{
					usernameField.text = value;
				}
				m_title = value;
			}
		}

		public void LoadPhoto(string p_player_id)
		{
			if (cache.ContainsKey(p_player_id))
			{
				photo = cache[p_player_id];
				return;
			}
			Texture2D tpx = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false, QualitySettings.activeColorSpace == ColorSpace.Linear);
			tpx.SetPixel(0, 0, Colorf.transparent);
			tpx.Apply();
			Dictionary<string, Texture> dictionary = cache;
			Texture value = (photo = tpx);
			dictionary[p_player_id] = value;
			Action<Texture2D> on_texture_load = delegate(Texture2D p_result)
			{
				if ((bool)p_result)
				{
					tpx.Resize(p_result.width, p_result.width, p_result.format, hasMipMap: false);
					tpx.LoadRawTextureData(p_result.GetRawTextureData());
					tpx.Apply();
					photo = tpx;
				}
			};
			if (p_player_id != null && p_player_id == "drl-sim-info-message")
			{
				Web.Get(DRLService.baseUri + "/images/avatar/drl-avatar.png", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f))
					{
						on_texture_load(p_result);
					}
				});
			}
			else
			{
				base.app.model.service.GetPlayerAvatar(p_player_id, on_texture_load);
			}
		}

		public void ShowPhoto(bool p_show)
		{
			if (!(photoContainer == null))
			{
				photoContainer.SetActive(p_show);
			}
		}
	}
}
