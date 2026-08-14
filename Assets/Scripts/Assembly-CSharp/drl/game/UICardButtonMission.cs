using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using thelab.core;

namespace drl.game
{
	public class UICardButtonMission : UICardView
	{
		public Text title0Field;

		public Text title1Field;

		public GameObject propContainer;

		public Text propField;

		public GameObject xpContainer;

		public Text xpField;

		public GameObject goldContainer;

		public Text goldField;

		public RawImage previewField;

		public RawImage imageField;

		public UIFlareProgressGroup stars;

		public int onboardinStep;

		public OnboardingCampaignMode onboardingCampaignMode;

		public List<GameObject> markers;

		public new DRLMission data;

		private MonoActivity m_score_timer;

		public override UICardType type => UICardType.ButtonMission;

		public string title0
		{
			set
			{
				UIReflection.Set(title0Field, value);
			}
		}

		public string title1
		{
			set
			{
				UIReflection.Set(title1Field, value);
			}
		}

		public int propCount
		{
			set
			{
				propContainer.SetActive(value > 0);
				UIReflection.Set(propField, value.ToString() ?? "");
			}
		}

		public int xpCount
		{
			set
			{
				xpContainer.SetActive(value > 0);
				UIReflection.Set(xpField, value.ToString() ?? "");
			}
		}

		public int goldCount
		{
			set
			{
				goldContainer.SetActive(value > 0);
				UIReflection.Set(goldField, value.ToString() ?? "");
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
				if ((bool)imageField)
				{
					imageField.enabled = value;
				}
			}
		}

		public void Set(DRLMission p_data)
		{
			if ((bool)p_data)
			{
				title0 = base.app.model.storage.locale.Get("mission-overview.mission-title", "MISSION") + " " + p_data.order.ToString("00") + ":";
				string input = p_data.title.Replace(" ", "-");
				input = Regex.Replace(input, "[^0-9a-zA-Z-]+", "");
				input = input.ToLower();
				string p_key = "mission-overview.mission-title.name." + input;
				title1 = base.app.model.storage.locale.Get(p_key, p_data.title).ToUpper();
				image = p_data.image;
				xpCount = 0;
				propCount = 0;
				goldCount = 0;
				data = p_data;
			}
		}

		public void SetRace(OnboardingStep onboardingStep, int raceIndex)
		{
			if (onboardingStep != null)
			{
				string text = "";
				text = (onboardingStep.trackGuid.IsNullOrEmpty() ? "WIP NO GUID" : ((!onboardingStep.trackGuid.StartsWith("CMP-")) ? base.app.model.storage.library.FindByGUID<DRLMapTrack>(onboardingStep.trackGuid).title : base.app.model.storage.maps.FindByGUID(onboardingStep.trackGuid).mapTitle));
				title0 = base.app.model.storage.locale.Get("mission-overview.mission-title", "MISSION") + " " + raceIndex.ToString("00") + ":";
				string input = text.Replace(" ", "-");
				input = Regex.Replace(input, "[^0-9a-zA-Z-]+", "");
				input = input.ToLower();
				string p_key = "mission-overview.mission-title.name." + input;
				title1 = base.app.model.storage.locale.Get(p_key, text).ToUpper();
				xpCount = 0;
				propCount = 0;
				goldCount = 0;
			}
		}

		public void SetScore(float p_score, float p_delay = 0f, float p_item_delay = 0.25f)
		{
			if (m_score_timer != null)
			{
				m_score_timer.Stop();
			}
			m_score_timer = RunOnce(delegate
			{
				float p_progress = p_score * (float)stars.list.Count;
				stars.FadeProgress(p_progress, p_item_delay);
			}, p_delay);
		}

		public GameObject GetRedMarker()
		{
			return markers[0];
		}

		public GameObject GetOrangeMarker()
		{
			return markers[1];
		}

		public GameObject GetGreenMarker()
		{
			return markers[2];
		}

		public GameObject GetGrayMarker()
		{
			return markers[3];
		}

		public GameObject GetWhiteMarker()
		{
			return markers[4];
		}

		public GameObject GetMarker(int index)
		{
			return markers[index];
		}

		public override void Build()
		{
			base.Build();
		}
	}
}
