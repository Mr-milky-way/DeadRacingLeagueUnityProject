using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIMissionCompleteView : UIScreenView
	{
		public enum TrainingType
		{
			Mission = 0,
			Lesson = 1,
			Test = 2
		}

		public DRLQuest quest;

		public DRLMission mission;

		public TrainingType type;

		public Text attempts;

		public FadeComponent fadeAttempts;

		public Text testCompleteFeedback;

		public float score;

		public RawImage questBackgroundField;

		public Text questNameField;

		public UICardButtonMission missionCard;

		public UIFlareProgressGroup stars;

		public Image radialBar;

		public Text scoreField;

		private float m_scoreFieldValue;

		public UINavigation nextNavBtn;

		public UINavigation questsNavBtn;

		public UINavigation restartNavBtn;

		public UINavigation menuNavBtn;

		public UINavigation exitNavBtn;

		public UINavigation onboardingNavBtn;

		public RectTransform feedbackContainer;

		public DRLTextAssetStepperView feedbackPilotExp;

		public DRLTextAssetStepperView feedbackEnjoyedMission;

		public DRLTextAssetStepperView feedbackImprovedSkills;

		public DRLInputFieldView feedbackMessage;

		private Activity m_score_timer;

		private float scoreFieldValue
		{
			get
			{
				return m_scoreFieldValue;
			}
			set
			{
				m_scoreFieldValue = value;
				scoreField.text = (int)(m_scoreFieldValue * 100f) + "/100";
			}
		}

		public void Set(DRLQuest p_quest, DRLMission p_mission)
		{
			quest = p_quest;
			mission = p_mission;
			score = 0f;
			List<DRLQuest> quests = base.app.model.storage.GetQuests((type == TrainingType.Mission) ? GameFlag.Training : GameFlag.DMVQuest);
			int count = quests.Count;
			int num = 0;
			using (List<DRLQuest>.Enumerator enumerator = quests.GetEnumerator())
			{
				while (enumerator.MoveNext() && !(enumerator.Current == quest))
				{
					num++;
				}
			}
			DRLMap dRLMap = (mission ? mission.map : null);
			Texture texture = (dRLMap ? dRLMap.blur : null);
			questNameField.text = (quest ? quest.title.ToUpper() : "");
			questBackgroundField.texture = texture;
			questBackgroundField.enabled = texture != null;
			missionCard.Set(mission);
			int num2 = (p_quest ? p_quest.missions.Count : 0);
			int num3 = (p_quest ? p_quest.missions.IndexOf(p_mission) : 0);
			Debug.Log("UIMissionCompleteView> Set - current[" + num3 + "] total[" + num2 + "]");
			if (type == TrainingType.Test)
			{
				return;
			}
			if (!((type != TrainingType.Mission) ? (num3 >= 0 && num3 + 1 < num2) : (num3 >= 0 && (num3 + 1 < num2 || num + 1 < count))))
			{
				if ((bool)nextNavBtn)
				{
					nextNavBtn.gameObject.SetActive(value: false);
				}
				if ((bool)questsNavBtn)
				{
					questsNavBtn.right = restartNavBtn;
				}
			}
			else
			{
				if ((bool)nextNavBtn)
				{
					nextNavBtn.gameObject.SetActive(value: true);
				}
				if ((bool)questsNavBtn)
				{
					questsNavBtn.right = nextNavBtn;
				}
			}
		}

		public void SetScore(float p_score, float p_delay = 0f)
		{
			score = p_score;
			if (m_score_timer != null)
			{
				m_score_timer.Stop();
			}
			float num = ((type == TrainingType.Mission) ? 5f : 100f);
			int total_score = Mathf.FloorToInt(p_score * num);
			float frac = Mathf.Round((p_score * num - (float)total_score) * 10f) / 10f;
			if (radialBar != null)
			{
				radialBar.fillAmount = 0f;
				scoreFieldValue = 0f;
				Tween.Kill(radialBar, "fillAmount");
				Tween.Kill(this, "scoreFieldValue");
				Tween.Add(radialBar, "fillAmount", score, 2f, 1f, Cubic.InOut);
				Tween.Add(this, "scoreFieldValue", score, 2f, 1f, Cubic.InOut);
				return;
			}
			bool is_fullscore = p_score >= 1f;
			float item_delay = 0.25f;
			float star_delay = 0.2f;
			AudioView av = base.app.view.audio;
			m_score_timer = Activity.RunOnce(delegate
			{
				if ((bool)stars)
				{
					float p_progress = p_score * (float)stars.list.Count;
					stars.FadeProgress(p_progress, item_delay);
					if (total_score <= 0)
					{
						RunOnce(star_delay, delegate
						{
							if ((bool)av)
							{
								av.PlayUIStarNone();
							}
						});
					}
					else
					{
						for (int num2 = 0; num2 < total_score; num2++)
						{
							RunOnce(star_delay, delegate
							{
								if ((bool)av)
								{
									av.PlayUIStar();
								}
							});
							star_delay += 0.2f;
						}
						if (frac >= 0.5f)
						{
							RunOnce(star_delay, delegate
							{
								if ((bool)av)
								{
									av.PlayUIStarHalf();
								}
							});
							star_delay += 0.2f;
						}
						star_delay += 1.2f;
						if (is_fullscore)
						{
							RunOnce(star_delay, delegate
							{
								if ((bool)av)
								{
									av.PlayUIStarFull();
								}
							});
						}
					}
				}
			}, p_delay);
		}

		public void SetAttempts(int p_crashes, int p_total, float p_score)
		{
			if ((bool)attempts && (bool)fadeAttempts)
			{
				if (p_score / 10f > (float)MissionController.passingScore)
				{
					attempts.text = "You completed this test.";
				}
				else
				{
					int num = p_total - p_crashes;
					attempts.text = num + "/" + p_total;
				}
				fadeAttempts.FadeIn();
			}
		}

		public void SetTestCompleteFeedback(int p_level, float p_score)
		{
			if (!(testCompleteFeedback == null))
			{
				if (p_level == 8)
				{
					testCompleteFeedback.text = "Congratulations! You have passed the Level 8 Test. This was the final test in this series and you have earned the Pro License. This certifies that you posses expert skill in operating a racing drone.";
					return;
				}
				string text = ((int)(p_score * 100f)).ToString();
				testCompleteFeedback.text = "Congratulations! You have passed the Level " + p_level + " Test with a score of " + text + ". You are now ready to move on to the next level.";
			}
		}
	}
}
