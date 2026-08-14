using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UITryoutsLoginView : UIScreenView
	{
		public FadeComponent photoFade;

		public RawImage photoImage;

		public DRLInputFieldView handleField;

		public GameObject waitField;

		public FadeComponent submitFade;

		public DRLColorPickerView colorField;

		public FadeComponent boxErrorFade;

		private Activity m_photo_timer;

		private AsyncRequest m_photo_loader;

		[SerializeField]
		private bool m_is_disabled;

		public bool isWaiting
		{
			get
			{
				return waitField.activeInHierarchy;
			}
			set
			{
				waitField.SetActive(value);
			}
		}

		public bool isError
		{
			get
			{
				return boxErrorFade.alpha > 0f;
			}
			set
			{
				boxErrorFade.Fade(value ? 1f : (-0.1f), 0.2f);
			}
		}

		public bool isDisabled
		{
			get
			{
				return m_is_disabled;
			}
			set
			{
				submitFade.Fade((m_is_disabled = value) ? 0.1f : 1f, 0.2f);
			}
		}

		public void SetPhoto(Texture p_photo)
		{
			if (m_photo_timer != null)
			{
				m_photo_timer.Stop();
			}
			photoFade.FadeOut(0.2f);
			m_photo_timer = Activity.RunOnce(delegate
			{
				photoImage.texture = p_photo;
				if ((bool)p_photo)
				{
					photoFade.FadeIn(0.2f);
				}
			}, 0.25f);
		}

		public void SetPhoto(string p_url)
		{
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			m_photo_loader = Web.Get(p_url, delegate(Texture2D p_data, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f)
				{
					SetPhoto(p_data);
				}
			});
		}
	}
}
