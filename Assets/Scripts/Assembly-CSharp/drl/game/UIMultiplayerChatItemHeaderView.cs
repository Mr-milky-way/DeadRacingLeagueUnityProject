using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerChatItemHeaderView : View<DRLApp>
	{
		internal static Dictionary<string, Texture> m_cache;

		public GameObject leftContent;

		public GameObject rightContent;

		public RawImage leftPhotoField;

		public Text leftField;

		public RawImage rightPhotoField;

		public Text rightField;

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public Texture photo
		{
			get
			{
				if (leftPhotoField == null || rightPhotoField == null)
				{
					return null;
				}
				if (!isLeft)
				{
					return rightPhotoField.texture;
				}
				return leftPhotoField.texture;
			}
			set
			{
				if (leftPhotoField != null && rightPhotoField != null)
				{
					RawImage rawImage = leftPhotoField;
					Texture texture = (rightPhotoField.texture = value);
					rawImage.texture = texture;
				}
			}
		}

		public string title
		{
			set
			{
				Text text = leftField;
				string text2 = (rightField.text = value);
				text.text = text2;
			}
		}

		public bool isLeft
		{
			get
			{
				return leftContent.activeInHierarchy;
			}
			set
			{
				leftContent.SetActive(value);
				rightContent.SetActive(!value);
			}
		}

		public void LoadPhoto(string p_player_id)
		{
			if (leftPhotoField == null || rightPhotoField == null)
			{
				return;
			}
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			if (!string.IsNullOrEmpty(p_player_id) && profile.playerId == p_player_id && profile.photo != null)
			{
				photo = profile.photo;
				cache[p_player_id] = photo;
				return;
			}
			if (cache.ContainsKey(p_player_id))
			{
				photo = cache[p_player_id];
				return;
			}
			if (photo != null)
			{
				photo = null;
			}
			base.app.model.service.GetPlayerAvatar(p_player_id, delegate(Texture2D p_result)
			{
				if ((bool)p_result)
				{
					Dictionary<string, Texture> dictionary = cache;
					string key = p_player_id;
					Texture value = (photo = p_result);
					dictionary[key] = value;
				}
			});
		}
	}
}
