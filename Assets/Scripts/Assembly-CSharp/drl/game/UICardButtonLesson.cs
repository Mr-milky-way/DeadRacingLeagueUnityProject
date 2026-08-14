using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICardButtonLesson : UICardView
	{
		public Text title0Field;

		public Text title1Field;

		public Text title2Field;

		public RawImage previewField;

		public RawImage imageField;

		public FadeComponent descriptionFade;

		public Text scoreField;

		private float m_scoreValue;

		public Image stripe;

		public Color32 lowProgressColor;

		public Color32 mediumProgressColor;

		public Color32 highProgressColor;

		public DRLQuest questData;

		public new DRLMission data;

		public override UICardType type => UICardType.ButtonMission;

		public UINavigation navigation => AssertLocal<UINavigation>("navigation");

		public float scoreValue
		{
			get
			{
				return m_scoreValue;
			}
			set
			{
				m_scoreValue = value;
				int num = Mathf.FloorToInt(m_scoreValue * 100f);
				scoreField.text = num + "/100";
			}
		}

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

		public void Set(DRLMission p_data, DRLQuest p_quest)
		{
			if (!p_data)
			{
				return;
			}
			title0 = p_data.title.ToUpper();
			title1 = p_data.description;
			image = p_data.image;
			data = p_data;
			questData = p_quest;
			if (!questData.tags.Contains(GameFlag.DMVQuest) || !(title2Field != null))
			{
				return;
			}
			DRLMissionScore component = data.GetComponent<DRLMissionScore>();
			if (component != null && component.tasks.Count > 0)
			{
				float num = (component.tasks[0].max - component.tasks[0].min) * (1f - (float)MissionController.passingScore / 100f) + component.tasks[0].min;
				if (questData.testMission != null && questData.testMission == data)
				{
					title2Field.text = "You must complete this test in under " + num + " seconds in order to pass.";
				}
				else
				{
					title2Field.text = "You must complete this course in under " + num + " seconds in order to pass.";
				}
			}
		}

		public void SetScore(float p_score)
		{
			p_score = Mathf.Clamp01(p_score);
			Tween.Kill(this, "scoreValue");
			Tween.Add(this, "scoreValue", p_score, 0.4f, Cubic.InOut);
			int num = Mathf.FloorToInt(p_score * 100f);
			if ((float)num < (float)MissionController.passingScore / 3f)
			{
				stripe.color = lowProgressColor;
			}
			else if (num < MissionController.passingScore)
			{
				Tween.Kill(stripe, "color");
				Tween.Add((object)stripe, "color", (Color)mediumProgressColor, 0.4f, (Easing)Cubic.InOut);
			}
			else
			{
				Tween.Kill(stripe, "color");
				Tween.Add((object)stripe, "color", (Color)highProgressColor, 0.4f, (Easing)Cubic.InOut);
			}
		}

		public override void Build()
		{
			base.Build();
		}

		private void OnDisable()
		{
			Tween.Kill(stripe, "color");
			Tween.Kill(this, "scoreValue");
		}
	}
}
