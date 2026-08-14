using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICardButtonQuest : UICardView
	{
		[SerializeField]
		private Text m_mission_count_field;

		[SerializeField]
		private Text m_quest_title_field;

		[SerializeField]
		private Text m_quest_name_field;

		[SerializeField]
		private RawImage m_previewField;

		[SerializeField]
		private RawImage m_imageField;

		public RectTransform footer;

		public Image footerBg;

		public Image footerStripe0;

		public Image footerStripe1;

		public Image footerStripe2;

		public Text footerText;

		public Color footerBeginnerColor = new Color32(90, 130, 50, byte.MaxValue);

		public Color footerIntermediateColor = new Color32(140, 110, 50, byte.MaxValue);

		public Color footerProColor = new Color32(150, 40, 40, byte.MaxValue);

		public new DRLQuest data;

		public UIFlareProgressGroup stars;

		public override UICardType type => UICardType.ButtonQuest;

		public Text missionCountField
		{
			get
			{
				if (!m_mission_count_field)
				{
					return m_mission_count_field = Find<Text>("content.body.title-1");
				}
				return m_mission_count_field;
			}
		}

		public Text questTitleField
		{
			get
			{
				if (!m_quest_title_field)
				{
					return m_quest_title_field = Find<Text>("content.body.title-2");
				}
				return m_quest_title_field;
			}
		}

		public Text questNameField
		{
			get
			{
				if (!m_quest_name_field)
				{
					return m_quest_name_field = Find<Text>("content.body.title-3");
				}
				return m_quest_name_field;
			}
		}

		public RawImage previewField
		{
			get
			{
				if (!m_previewField)
				{
					return m_previewField = Find<RawImage>("backgrounds.preview");
				}
				return m_previewField;
			}
		}

		public RawImage imageField
		{
			get
			{
				if (!m_imageField)
				{
					return m_imageField = Find<RawImage>("backgrounds.image");
				}
				return m_imageField;
			}
		}

		public string missionText { get; set; }

		public int missionCount
		{
			set
			{
				missionCountField.enabled = value > 0;
				UIReflection.Set(missionCountField, value + missionText);
			}
		}

		public string questTitle
		{
			set
			{
				UIReflection.Set(questTitleField, value);
			}
		}

		public string questName
		{
			set
			{
				UIReflection.Set(questNameField, value);
			}
		}

		public Texture preview
		{
			set
			{
				UIReflection.Set(previewField, value);
			}
		}

		public Texture image
		{
			set
			{
				UIReflection.Set(imageField, value);
			}
		}

		public void Set(DRLQuest p_data)
		{
			data = p_data;
			if ((bool)data)
			{
				questName = data.title.ToUpper();
				questTitle = "";
				missionCount = 0;
				preview = GetPreview(p_data);
				if ((bool)previewField)
				{
					previewField.enabled = previewField.texture;
				}
				List<FCMode> flightModes = p_data.flightModes;
				if (flightModes.Count > 0)
				{
					bool p_intermediate = flightModes.Contains(FCMode.Intermediate);
					bool p_pro = flightModes.Contains(FCMode.Pro);
					bool p_beginner = flightModes.Contains(FCMode.Beginner);
					SetupFooter(p_beginner, p_intermediate, p_pro);
				}
				else
				{
					EnableFooter(p_enable: false);
				}
			}
		}

		private Texture GetPreview(DRLQuest p_data)
		{
			Texture texture = null;
			DRLMap dRLMap = ((data.missions.Count <= 0) ? null : data.missions[0].map);
			texture = p_data.image;
			if (texture == null && dRLMap != null)
			{
				texture = dRLMap.blur;
			}
			return texture;
		}

		private void EnableFooter(bool p_enable)
		{
			if ((bool)footer)
			{
				footer.gameObject.SetActive(p_enable);
			}
		}

		private void SetupFooter(bool p_beginner, bool p_intermediate, bool p_pro)
		{
			if (!footerBg || !footerStripe0 || !footerStripe1 || !footerStripe2 || !footerText)
			{
				return;
			}
			bool flag = true;
			if (!p_beginner && !p_intermediate && !p_pro)
			{
				flag = false;
			}
			EnableFooter(flag);
			if (flag)
			{
				footerBg.gameObject.SetActive(value: true);
				footerStripe1.gameObject.SetActive(value: true);
				if (p_beginner)
				{
					footerBg.color = footerBeginnerColor;
					footerStripe1.color = footerBeginnerColor;
					footerStripe0.gameObject.SetActive(value: false);
					footerStripe2.gameObject.SetActive(value: false);
					footerText.text = base.app.model.storage.locale.Get("quests.card-footer.begginer-mode", "BEGINNER MODE");
				}
				else if (p_intermediate && !p_pro)
				{
					footerBg.color = footerIntermediateColor;
					footerStripe1.color = footerIntermediateColor;
					footerStripe0.gameObject.SetActive(value: false);
					footerStripe2.gameObject.SetActive(value: false);
					footerText.text = base.app.model.storage.locale.Get("quests.card-footer.intermediate-mode", "INTERMEDIATE MODE");
				}
				else if (p_pro && !p_intermediate)
				{
					footerBg.color = footerProColor;
					footerStripe1.color = footerProColor;
					footerStripe0.gameObject.SetActive(value: false);
					footerStripe2.gameObject.SetActive(value: false);
					footerText.text = base.app.model.storage.locale.Get("quests.card-footer.pro-mode", "PRO MODE");
				}
				else if (p_intermediate && p_pro)
				{
					footerBg.gameObject.SetActive(value: false);
					footerStripe1.gameObject.SetActive(value: false);
					footerStripe0.gameObject.SetActive(value: true);
					footerStripe0.color = footerIntermediateColor;
					footerStripe2.gameObject.SetActive(value: true);
					footerStripe2.color = footerProColor;
					footerText.text = base.app.model.storage.locale.Get("quests.card-footer.intermediate-pro-mode", "INTERMEDIATE + PRO MODE");
				}
			}
		}

		public override void Build()
		{
			base.Build();
			FocusResize focusResize = GetComponent<FocusResize>();
			if (!focusResize)
			{
				focusResize = base.gameObject.AddComponent<FocusResize>();
			}
			focusResize.enabled = true;
			focusResize.min = new Vector2(420f, 540f);
			focusResize.max = new Vector2(500f, 650f);
			focusResize.duration = 0.1f;
			missionCount = 0;
			questTitle = base.app.model.storage.locale.Get("quests.card-title.quest", "QUEST") + " 00:";
			questName = "TITLE XYZ";
			preview = null;
			image = null;
			((RectTransform)base.transform).sizeDelta = focusResize.min;
		}
	}
}
