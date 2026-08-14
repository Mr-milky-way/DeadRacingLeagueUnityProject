using System;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UILeaderboardCardView : UICardView, ILocaleElement
	{
		public Text positionField;

		public Text profileNameField;

		public Text timeField;

		public RawImage profilePhotoField;

		private ImageLayout m_profilePhotoLayout;

		public RectTransform profilePhotoRT;

		public FadeComponent bodyFade;

		private float m_time;

		public float photoWidth = 320f;

		public Texture2D customBackground;

		private int position;

		private WebAsyncRequest m_photo_loader;

		private bool m_oscilate;

		private float m_oscilate_angle;

		public ImageLayout profilePhotoLayout
		{
			get
			{
				if (!m_profilePhotoLayout)
				{
					return m_profilePhotoLayout = profilePhotoField.GetComponent<ImageLayout>();
				}
				return m_profilePhotoLayout;
			}
		}

		public float time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
				if ((bool)timeField)
				{
					timeField.text = Format.SecondsToMMSSFFF(value);
				}
			}
		}

		public void Set(DRLLeaderboardData p_data, float p_duration = 0.4f)
		{
			if (this == null || !profilePhotoRT)
			{
				return;
			}
			Vector2 sd = (profilePhotoRT ? profilePhotoRT.sizeDelta : Vector2.zero);
			sd.x = 0f;
			if ((bool)profilePhotoRT)
			{
				profilePhotoRT.sizeDelta = sd;
			}
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			profileNameField.text = string.Empty;
			positionField.text = string.Empty;
			timeField.text = string.Empty;
			profilePhotoField.texture = null;
			bodyFade.FadeOut(0.0001f);
			if (p_data == null)
			{
				return;
			}
			if (!positionField.supportRichText)
			{
				positionField.supportRichText = true;
			}
			position = p_data.position;
			SetLeadboardPositionText();
			profileNameField.text = p_data.profileName.ToUpper();
			UITruncateText uITruncateText = (profileNameField ? profileNameField.GetComponent<UITruncateText>() : null);
			if ((bool)uITruncateText)
			{
				uITruncateText.TruncateText();
			}
			time = 0f;
			Tween.Add(this, "time", p_data.scoreSeconds, 0.2f, 0.5f, Cubic.Out);
			m_photo_loader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(this == null))
				{
					profilePhotoField.texture = p_result;
					sd.x = photoWidth;
					Tween.Add(profilePhotoRT, "sizeDelta", sd, p_duration, 0f, Cubic.Out);
					bodyFade.FadeIn(p_duration, p_duration);
					m_oscilate_angle = -1f;
					m_oscilate = p_result != null;
				}
			});
		}

		public void Set(DRLCircuitLeaderboardData p_data, float p_duration = 0.4f)
		{
			if (this == null || !profilePhotoRT)
			{
				return;
			}
			Vector2 sd = (profilePhotoRT ? profilePhotoRT.sizeDelta : Vector2.zero);
			sd.x = 0f;
			if ((bool)profilePhotoRT)
			{
				profilePhotoRT.sizeDelta = sd;
			}
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			profileNameField.text = string.Empty;
			positionField.text = string.Empty;
			timeField.text = string.Empty;
			profilePhotoField.texture = null;
			bodyFade.FadeOut(0.0001f);
			if (p_data == null)
			{
				return;
			}
			if (!positionField.supportRichText)
			{
				positionField.supportRichText = true;
			}
			position = 1;
			SetLeadboardPositionText();
			profileNameField.text = p_data.profileName.ToUpper();
			UITruncateText uITruncateText = (profileNameField ? profileNameField.GetComponent<UITruncateText>() : null);
			if ((bool)uITruncateText)
			{
				uITruncateText.TruncateText();
			}
			time = 0f;
			Tween.Add(this, "time", p_data.scoreSeconds, 0.2f, 0.5f, Cubic.Out);
			m_photo_loader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(this == null))
				{
					profilePhotoField.texture = p_result;
					sd.x = photoWidth;
					Tween.Add(profilePhotoRT, "sizeDelta", sd, p_duration, 0f, Cubic.Out);
					bodyFade.FadeIn(p_duration, p_duration);
					m_oscilate_angle = -1f;
					m_oscilate = p_result != null;
				}
			});
		}

		private void SetLeadboardPositionText()
		{
			if (!(base.app == null))
			{
				Localization locale = base.app.model.storage.locale;
				positionField.text = "#" + position + " " + locale.Get("campaign.overview.onleaderboard", "<size=21>ON LEADERBOARD</size>");
			}
		}

		protected void Start()
		{
			Localization.Add(this);
		}

		private void Update()
		{
			if (m_oscilate)
			{
				m_oscilate_angle += Time.deltaTime;
				float y = Mathf.Sin(Mathf.Max(0f, m_oscilate_angle) * ((float)Math.PI / 180f) * 10f) * 0.5f;
				Vector2 offset = profilePhotoLayout.offset;
				offset.y = y;
				profilePhotoLayout.offset = offset;
			}
		}

		void ILocaleElement.OnLocaleRefresh()
		{
			SetLeadboardPositionText();
		}
	}
}
