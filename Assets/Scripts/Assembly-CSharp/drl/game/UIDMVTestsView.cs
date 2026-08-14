using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIDMVTestsView : UIScreenView
	{
		public ListComponent listField;

		public Image progressBar;

		public Text progressField;

		public Text totalTime;

		public UINavigation backButtonNavigation;

		public UINavigation exitButtonNavigation;

		public GameObject exitButton;

		private float m_progressBarValue;

		private int m_totalTimeValue;

		private int m_currentLevel;

		public float progressBarValue
		{
			get
			{
				return m_progressBarValue;
			}
			set
			{
				m_progressBarValue = value;
				progressBar.fillAmount = m_progressBarValue;
				progressField.text = (int)(m_progressBarValue * 100f) + "%";
			}
		}

		public int totalTimeValue
		{
			get
			{
				return m_totalTimeValue;
			}
			set
			{
				m_totalTimeValue = value;
				totalTime.text = m_totalTimeValue / 60 + " HOURS, " + m_totalTimeValue % 60 + " MINUTES";
			}
		}

		public int currentLevel
		{
			get
			{
				return m_currentLevel;
			}
			set
			{
				m_currentLevel = value;
				LockCardsByLevel(m_currentLevel);
			}
		}

		public void ClearQuests()
		{
			if ((bool)listField)
			{
				listField.Clear();
			}
		}

		public void AddQuest(DRLQuest p_item)
		{
			if ((bool)listField && (bool)p_item)
			{
				UICardButtonDmvTest uICardButtonDmvTest = listField.Push<UICardButtonDmvTest>();
				uICardButtonDmvTest.Set(p_item);
				uICardButtonDmvTest.notification = "missions.quest-card";
			}
		}

		public void SetProgress(float p_progress)
		{
			Tween.Kill(this, "progressBarValue");
			Tween.Add(this, "progressBarValue", p_progress, 1f, Cubic.InOut);
		}

		public void SetTotalTime(int p_time)
		{
			int p_to = p_time / 60;
			Tween.Kill(this, "totalTimeValue");
			Tween.Add(this, "totalTimeValue", p_to, 1f, Cubic.InOut);
		}

		public void ClearInfo()
		{
			progressBarValue = 0f;
			totalTimeValue = 0;
		}

		public void LockCardsByLevel(int p_currentLevel)
		{
			List<UICardButtonDmvTest> list = listField.GetList<UICardButtonDmvTest>();
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].lockedOnTimer)
				{
					if (i <= p_currentLevel)
					{
						list[i].UnlockCard();
					}
					else
					{
						list[i].LockCard();
					}
				}
			}
			list[6].LockCard(1500f);
		}

		public void CheckLessonSetCompleteTestUnlock()
		{
			List<UICardButtonDmvTest> list = listField.GetList<UICardButtonDmvTest>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].lockedOnTimer && HasFinishedAllLessons(i))
				{
					list[i].UnlockCard();
					list[i].ResetTestAttempts();
				}
			}
		}

		private bool HasFinishedAllLessons(int p_level)
		{
			List<UICardButtonDmvTest> list = listField.GetList<UICardButtonDmvTest>();
			if (list[p_level].lessons.Count == 0)
			{
				return false;
			}
			bool result = true;
			foreach (UICardButtonLesson lesson in list[p_level].lessons)
			{
				if (lesson.scoreValue < (float)MissionController.passingScore / 100f)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public void ResetAllScoring()
		{
			List<DRLMission> list = new List<DRLMission>();
			foreach (UICardButtonDmvTest item2 in listField.GetList<UICardButtonDmvTest>())
			{
				if (item2.data.missions.Count > 0)
				{
					foreach (DRLMission mission in item2.data.missions)
					{
						list.Add(mission);
					}
				}
				if ((bool)item2.data.testMission)
				{
					list.Add(item2.data.testMission);
					item2.ResetTestAttempts();
				}
			}
			foreach (DRLMission item3 in list)
			{
				Debug.Log(item3.name);
			}
			base.app.model.service.ResetLeaderboardQuest(list, delegate
			{
			});
			base.app.model.storage.state.player.userRank = -1;
		}
	}
}
