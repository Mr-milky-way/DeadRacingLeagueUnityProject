using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLMapEditorCollabItem : NotificationView<DRLApp>
	{
		public FadeResizeComponent photoFade;

		public RawImage photoImage;

		public Image photoColor;

		public Text usernameField;

		public DRLPlayerProfileData data;

		private WebAsyncRequest m_photo_loader;

		public void Set(Texture2D p_photo, Color p_color, string p_name)
		{
			float p_to = (p_photo ? 1f : 0f);
			photoFade.Fade(p_to, 0.3f);
			photoImage.texture = p_photo;
			Color color = photoImage.color;
			color.a = (p_photo ? 1f : 0f);
			photoImage.color = color;
			photoColor.color = p_color;
			usernameField.text = p_name;
		}

		public void Set(string p_photo, Color p_color, string p_name)
		{
			Set((Texture2D)null, p_color, p_name);
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			m_photo_loader = base.app.model.service.GetPlayerAvatar(p_photo, delegate(Texture2D p_result)
			{
				if (base.validContext)
				{
					Set(p_result, p_color, p_name);
				}
			});
		}

		public void Set(DRLPlayerProfileData p_data)
		{
			data = p_data;
			if (data == null)
			{
				Set((Texture2D)null, Color.clear, "NULL");
			}
			else
			{
				Set(data.playerId, data.profileColor, data.profileName.ToUpper());
			}
		}

		public void OnButtonClick(Button p_button)
		{
			string text = (p_button ? p_button.name : "");
			if (text != null && text == "delete")
			{
				Notify(notification, text);
			}
		}
	}
}
