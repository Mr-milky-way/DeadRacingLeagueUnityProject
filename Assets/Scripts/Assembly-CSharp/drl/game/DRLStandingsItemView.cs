using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class DRLStandingsItemView : MonoBehaviour
	{
		public Image backgroundField;

		public Image darkField;

		public Text positionField;

		public RectTransform photoRT;

		public RawImage photoField;

		public Text profileNameField;

		public Text timeField;

		public FadeComponent contentFade;

		public UITruncateText profileNameFieldTruncate;

		public Font regularFont;

		public Font boldFont;

		public GameObject damageIcon;

		public GameObject damageSpace;

		private string m_player_id;

		private int m_position;

		private float m_time;

		[SerializeField]
		private float m_photo_preferred_w = -1f;

		public string playerId
		{
			get
			{
				return m_player_id;
			}
			set
			{
				m_player_id = value;
			}
		}

		public Color backgroundColor
		{
			get
			{
				return backgroundField.color;
			}
			set
			{
				backgroundField.color = value;
			}
		}

		public int position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
				positionField.text = (m_position + 1).ToString();
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
				if (value > 0f)
				{
					timeField.text = Format.SecondsToMMSSFFF(value);
					return;
				}
				timeField.text = "";
				m_time = 0f;
			}
		}

		public string timeLabel
		{
			set
			{
				timeField.text = value;
			}
		}

		public Texture profilePhoto
		{
			get
			{
				return photoField.texture;
			}
			set
			{
				photoField.texture = value;
				photoField.enabled = value != null;
			}
		}

		public string profileName
		{
			set
			{
				profileNameField.text = value;
				if ((bool)profileNameFieldTruncate)
				{
					profileNameFieldTruncate.TruncateText();
				}
			}
		}

		public bool bold
		{
			set
			{
				profileNameField.font = (value ? boldFont : regularFont);
				timeField.font = (value ? boldFont : regularFont);
				positionField.font = (value ? boldFont : regularFont);
			}
		}

		public float photoPreferredWidth
		{
			get
			{
				if (m_photo_preferred_w > 0f)
				{
					return m_photo_preferred_w;
				}
				LayoutElement component = photoRT.GetComponent<LayoutElement>();
				return m_photo_preferred_w = (component ? component.preferredWidth : 56f);
			}
		}

		public bool hasPosition
		{
			get
			{
				return positionField.gameObject.activeInHierarchy;
			}
			set
			{
				positionField.gameObject.SetActive(value);
			}
		}

		public void SetVisible(bool p_flag)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			RectTransform obj = darkField.transform as RectTransform;
			float num = (p_flag ? 10f : 0f);
			obj.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, num, rectTransform.sizeDelta.x - num + 1f);
			obj.localScale = new Vector3(p_flag ? 1f : 0f, 1f, 1f);
			(backgroundField.transform as RectTransform).localScale = new Vector3(p_flag ? 1f : 0f, 1f, 1f);
			contentFade.alpha = (p_flag ? 1f : 0f);
			LayoutElement component = photoRT.GetComponent<LayoutElement>();
			float num2 = photoPreferredWidth;
			component.preferredWidth = (p_flag ? num2 : 0f);
		}

		public void Fade(bool p_flag, float p_duration, float p_delay = 0f)
		{
			float num = p_delay;
			float num2 = photoPreferredWidth;
			SetVisible(!p_flag);
			if (p_flag)
			{
				RectTransform p_target = darkField.transform as RectTransform;
				Tween.Kill(p_target);
				Tween.Add(p_target, "localScale", new Vector3(p_flag ? 1f : 0f, 1f, 1f), p_duration, num, Cubic.Out);
				Tween.Add(p_target, "anchoredPosition", new Vector2(p_flag ? 10f : 0f, 0f), p_duration, num + p_duration, Cubic.In);
				RectTransform p_target2 = backgroundField.transform as RectTransform;
				Tween.Kill(p_target2);
				Tween.Add(p_target2, "localScale", new Vector3(p_flag ? 1f : 0f, 1f, 1f), p_duration, num, Cubic.In);
				num += p_duration;
				contentFade.Fade(p_flag ? 1f : 0f, p_duration * 0.5f, num, Cubic.Out);
				num += 0.1f;
				LayoutElement component = photoRT.GetComponent<LayoutElement>();
				Tween.Kill(component);
				Tween.Add(component, "preferredWidth", p_flag ? num2 : 0f, p_duration * 0.5f, num, Cubic.Out);
			}
			else
			{
				LayoutElement component2 = photoRT.GetComponent<LayoutElement>();
				Tween.Kill(component2);
				Tween.Add(component2, "preferredWidth", p_flag ? num2 : 0f, p_duration * 0.5f, num, Cubic.Out);
				num += p_duration * 0.25f;
				contentFade.Fade(p_flag ? 1f : 0f, p_duration * 0.5f, num, Cubic.Out);
				RectTransform p_target3 = darkField.transform as RectTransform;
				Tween.Kill(p_target3);
				Tween.Add(p_target3, "anchoredPosition", new Vector2(p_flag ? 10f : 0f, 0f), 0.3f, num, Cubic.In);
				num += 0.3f;
				Tween.Add(p_target3, "localScale", new Vector3(p_flag ? 1f : 0f, 1f, 1f), p_duration, num, Cubic.Out);
				RectTransform p_target4 = backgroundField.transform as RectTransform;
				Tween.Kill(p_target4);
				Tween.Add(p_target4, "localScale", new Vector3(p_flag ? 1f : 0f, 1f, 1f), p_duration, num, Cubic.Out);
			}
		}

		public void SetDamageIndicator(bool p_flag)
		{
			if (!(damageIcon == null) && !(damageSpace == null))
			{
				damageIcon.SetActive(p_flag);
				damageSpace.SetActive(p_flag);
			}
		}
	}
}
