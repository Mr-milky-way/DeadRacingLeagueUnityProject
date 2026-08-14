using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIUserItemView : View<DRLApp>
	{
		internal Dictionary<string, Texture> m_cache;

		public RawImage avatar;

		public new Text name;

		public Sprite defaultAvatarImage;

		private GameFriendData m_friendData;

		internal Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public GameFriendData gameFriendData => m_friendData;

		public virtual void Set(Texture p_avatar, string p_username)
		{
			avatar.texture = p_avatar;
			name.text = p_username.ToUpper();
		}

		public virtual void Set(string p_avatarUrl, string p_username)
		{
			name.text = p_username.ToUpper();
			LoadPhoto(p_avatarUrl);
		}

		public virtual void Set(string p_username)
		{
			name.text = p_username.ToUpper();
			avatar.texture = defaultAvatarImage.texture;
		}

		public virtual void Set(GameFriendData p_userItem)
		{
			m_friendData = p_userItem;
			if (p_userItem.photo != null)
			{
				Set(p_userItem.photo, p_userItem.name);
			}
			else
			{
				Set(p_userItem.name);
			}
		}

		public virtual void LoadPhoto(string p_url)
		{
			if (cache.ContainsKey(p_url))
			{
				avatar.texture = cache[p_url];
				return;
			}
			if (avatar.texture != null)
			{
				Object.DestroyImmediate(avatar.texture, allowDestroyingAssets: true);
			}
			base.app.model.service.GetPlayerAvatar(p_url, delegate(Texture2D p_result)
			{
				if ((bool)p_result)
				{
					Dictionary<string, Texture> dictionary = cache;
					string key = p_url;
					Texture value = (avatar.texture = p_result);
					dictionary[key] = value;
				}
			});
		}
	}
}
