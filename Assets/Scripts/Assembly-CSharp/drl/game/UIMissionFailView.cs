using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIMissionFailView : UIScreenView
	{
		public DRLQuest quest;

		public DRLMission mission;

		public float score;

		public RawImage questBackgroundField;

		public Text questNameField;

		public UICardButtonMission missionCard;

		public UIFlareProgressGroup stars;

		public UINavigation nextNavBtn;

		public UINavigation questsNavBtn;

		public UINavigation restartNavBtn;

		public RectTransform feedbackContainer;

		public DRLTextAssetStepperView feedbackPilotExp;

		public DRLTextAssetStepperView feedbackEnjoyedMission;

		public DRLTextAssetStepperView feedbackImprovedSkills;

		public DRLInputFieldView feedbackMessage;

		private Activity m_score_timer;

		public void Set(DRLQuest p_quest, DRLMission p_mission)
		{
			quest = p_quest;
			mission = p_mission;
			score = 0f;
			int count = base.app.model.storage.library.FindAll<DRLQuest>().Count;
			int num = 0;
			using (List<DRLQuest>.Enumerator enumerator = base.app.model.storage.library.FindAll<DRLQuest>().GetEnumerator())
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
			if (num3 < 0 || (num3 + 1 >= num2 && num + 1 >= count))
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
			float num = 5f;
			int total_score = Mathf.FloorToInt(p_score * num);
			float frac = Mathf.Round((p_score * num - (float)total_score) * 10f) / 10f;
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
	}
}
