using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICardButtonDmvTest : UICardView
	{
		[Space]
		[Header("Folded content:")]
		public Text lessonCountFieldFolded;

		public Text questNameFieldFolded;

		public FadeComponent fadeFolded;

		public Text scoreField;

		public GameObject unlockInfo;

		private float m_scoreValue;

		[Space]
		[Header("Unfolded content:")]
		public Text questNameFieldUnfolded;

		public Text questDescriptionField;

		public Text lessonCountFieldUnfolded;

		public FadeComponent fadeUnfolded;

		[Space]
		public UINavigation navigation;

		public UINavigation startButtonNavigation;

		public GameObject checkmark;

		public GameObject lockTimer;

		public Text lockTimerField;

		public GameObject lockOverlay;

		public RectTransform footer;

		public Image cardProgressStripe;

		public Color completeProgressColor;

		public Color middleProgressColor;

		public Color startingProgressColor;

		private bool m_animatingSubmenu;

		private bool m_dataReceived;

		public new DRLQuest data;

		public GameObject startTestButton;

		[Space]
		[Header("Lessons menu:")]
		public int maxColumnCount = 4;

		public GameObject lessonsMenu;

		public ListComponent lessonsMenuFirstRow;

		public ListComponent lessonsMenuSecondRow;

		public List<UICardButtonLesson> lessons = new List<UICardButtonLesson>();

		public bool menuOpened;

		private MonoActivity m_lockdownTimer;

		private bool m_hardLock;

		public override UICardType type => UICardType.DmvTest;

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

		public bool lockedOnTimer { get; set; }

		public bool completed { get; set; }

		public LayoutElement layout => AssertLocal<LayoutElement>("layout");

		public void Set(DRLQuest p_data)
		{
			data = p_data;
			if ((bool)data)
			{
				questNameFieldFolded.text = data.levelTitle.ToUpper();
				questNameFieldUnfolded.text = data.title.ToUpper();
				questDescriptionField.text = data.description;
				LockCard();
				if (data.missions.Count == 1)
				{
					lessonCountFieldFolded.text = "1 " + base.app.model.storage.locale.Get("dmv-test-menu.lesson-title", "LESSON");
				}
				else
				{
					lessonCountFieldFolded.text = data.missions.Count + " " + base.app.model.storage.locale.Get("dmv-test-menu.lessons-title", "LESSONS");
				}
				lessonCountFieldUnfolded.text = lessonCountFieldFolded.text;
				SetupLessonsMenu();
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
			((RectTransform)base.transform).sizeDelta = focusResize.min;
		}

		public void StartTest()
		{
			Notify("missions.test-card@click", this);
		}

		public void MarkComplete(bool p_active = true)
		{
			checkmark.SetActive(p_active);
			completed = p_active;
		}

		public void LockCard(float p_period = 0f)
		{
			m_hardLock = !(p_period > 0f);
			SetInteractable();
			if (menuOpened)
			{
				startTestButton.SetActive(value: false);
			}
			if (m_hardLock)
			{
				lockTimerField.text = "00:00:00";
				lockTimer.SetActive(value: false);
				return;
			}
			float duration = p_period;
			if (!(p_period > 0f))
			{
				return;
			}
			lockedOnTimer = true;
			unlockInfo.SetActive(value: true);
			lockTimer.SetActive(value: true);
			if (m_lockdownTimer != null && m_lockdownTimer.IsRunning)
			{
				m_lockdownTimer.Stop();
				m_lockdownTimer = null;
			}
			m_lockdownTimer = Run((Func<bool>)delegate
			{
				duration -= Time.deltaTime;
				int num = (int)(duration / 3600f);
				int num2 = (int)(duration / 60f % 60f);
				int num3 = (int)(duration % 60f);
				lockTimerField.text = num + ":" + num2 + ":" + num3;
				if (duration <= 0f)
				{
					UnlockCard();
					ResetTestAttempts();
				}
				return duration > 0f;
			}, 0f, false);
		}

		public void UnlockCard()
		{
			SetInteractable(p_flag: true);
			lockTimer.SetActive(value: false);
			lockedOnTimer = false;
			m_hardLock = false;
			if (menuOpened)
			{
				startTestButton.SetActive(value: true);
			}
			unlockInfo.SetActive(value: false);
			if (m_lockdownTimer != null)
			{
				m_lockdownTimer.Stop();
				m_lockdownTimer = null;
			}
		}

		private void SetInteractable(bool p_flag = false)
		{
			lockOverlay.SetActive(!p_flag);
		}

		private void SetupLessonsMenu()
		{
			ClearLessonsMenu();
			if ((bool)data)
			{
				List<DRLMission> missions = data.missions;
				for (int i = 0; i < missions.Count; i++)
				{
					AddMission(missions[i], i / maxColumnCount);
				}
				AddDummies();
			}
		}

		private void ClearLessonsMenu()
		{
			if ((bool)lessonsMenuFirstRow)
			{
				lessonsMenuFirstRow.Clear();
				if ((bool)lessonsMenuSecondRow)
				{
					lessonsMenuSecondRow.Clear();
				}
			}
		}

		private void AddMission(DRLMission p_item, int p_row)
		{
			if ((bool)lessonsMenuFirstRow && (p_row != 1 || (bool)lessonsMenuSecondRow) && (bool)p_item)
			{
				UICardButtonLesson uICardButtonLesson = ((p_row != 0) ? lessonsMenuSecondRow.Push<UICardButtonLesson>() : lessonsMenuFirstRow.Push<UICardButtonLesson>());
				uICardButtonLesson.notification = "missions.mission-card";
				uICardButtonLesson.Set(p_item, data);
				lessons.Add(uICardButtonLesson);
			}
		}

		private void AddDummies()
		{
			int num = lessonsMenuFirstRow.transform.childCount - lessonsMenuSecondRow.transform.childCount;
			if (num != 0 && lessonsMenuSecondRow.transform.childCount != 0)
			{
				for (int i = 0; i < num; i++)
				{
					GameObject obj = new GameObject("dummy-space");
					obj.transform.parent = lessonsMenuSecondRow.transform;
					obj.AddComponent<LayoutElement>().flexibleWidth = 1f;
					obj.GetComponent<LayoutElement>().preferredWidth = 265f;
					obj.GetComponent<RectTransform>().localScale = Vector3.one;
				}
			}
		}

		public void FoldLessonsMenu(float p_duration = 0.4f)
		{
			if (m_hardLock || !lessonsMenuFirstRow || (lessonsMenuFirstRow.Count == 0 && data.testMission == null) || m_animatingSubmenu || (lessonsMenuFirstRow.Count == 0 && lockedOnTimer))
			{
				return;
			}
			m_animatingSubmenu = true;
			if (lessonsMenuFirstRow.Count == 0 && data.testMission != null)
			{
				startTestButton.SetActive(value: false);
				if (!lockedOnTimer)
				{
					fadeFolded.FadeIn(p_duration);
					fadeUnfolded.FadeOut(p_duration);
					menuOpened = false;
					Notify("test.submenu@closed", p_duration);
					RunOnce(p_duration, delegate
					{
						m_animatingSubmenu = false;
					});
				}
				return;
			}
			List<UICardButtonLesson> list = lessonsMenuFirstRow.GetList<UICardButtonLesson>();
			List<UICardButtonLesson> list2 = lessonsMenuSecondRow.GetList<UICardButtonLesson>();
			foreach (UICardButtonLesson item in list)
			{
				item.descriptionFade.FadeOut(0.2f);
			}
			foreach (UICardButtonLesson item2 in list2)
			{
				item2.descriptionFade.FadeOut(0.2f);
			}
			RunOnce(0.2f, delegate
			{
				startTestButton.SetActive(value: false);
				if (!lockedOnTimer)
				{
					fadeFolded.FadeIn(p_duration);
					fadeUnfolded.FadeOut(p_duration);
				}
				Tween.Kill(layout, "minWidth");
				Tween tween = Tween.Add(layout, "minWidth", 420f, p_duration, Cubic.InOut);
				tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
				{
					lessonsMenu.SetActive(value: false);
					m_animatingSubmenu = false;
				});
			});
			menuOpened = false;
			Notify("test.submenu@closed", p_duration);
		}

		public void UnfoldLessonsMenu(float p_duration = 0.4f)
		{
			if (m_hardLock || !lessonsMenuFirstRow || (lessonsMenuFirstRow.Count == 0 && data.testMission == null) || (lessonsMenuFirstRow.Count == 0 && lockedOnTimer) || m_animatingSubmenu)
			{
				return;
			}
			m_animatingSubmenu = true;
			if (!lockedOnTimer && m_dataReceived)
			{
				fadeFolded.FadeOut(p_duration);
				fadeUnfolded.FadeIn(p_duration);
				startTestButton.SetActive(!lockedOnTimer && data.testMission != null);
				menuOpened = true;
				RunOnce(p_duration, delegate
				{
					m_animatingSubmenu = false;
				});
			}
			if (lessonsMenuFirstRow.Count == 0 && data.testMission != null)
			{
				Notify("test.submenu@opened", p_duration);
				return;
			}
			lessonsMenu.SetActive(value: true);
			float p_to = (float)lessonsMenuFirstRow.Count * 265f + (float)lessonsMenuFirstRow.Count * 10f + 420f;
			Tween.Kill(layout, "minWidth");
			Tween tween = Tween.Add(layout, "minWidth", p_to, p_duration, Cubic.InOut);
			tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
			{
				List<UICardButtonLesson> list = lessonsMenuFirstRow.GetList<UICardButtonLesson>();
				List<UICardButtonLesson> list2 = lessonsMenuSecondRow.GetList<UICardButtonLesson>();
				foreach (UICardButtonLesson item in list)
				{
					item.descriptionFade.FadeIn(0.2f);
				}
				foreach (UICardButtonLesson item2 in list2)
				{
					item2.descriptionFade.FadeIn(0.2f);
				}
				Activity.RunOnce(delegate
				{
					m_animatingSubmenu = false;
				}, 0.2f);
			});
			menuOpened = true;
			Notify("test.submenu@opened", p_duration);
		}

		public void ToggleLessonsMenu(float p_duration = 0.4f)
		{
			if (menuOpened)
			{
				FoldLessonsMenu(p_duration);
			}
			else
			{
				UnfoldLessonsMenu(p_duration);
			}
		}

		public void SetScore(float p_score)
		{
			m_dataReceived = true;
			p_score = Mathf.Clamp01(p_score);
			Tween.Kill(this, "scoreValue");
			Tween.Add(this, "scoreValue", p_score, 0.4f, Cubic.InOut);
			int num = Mathf.FloorToInt(p_score * 100f);
			if ((float)num < (float)MissionController.passingScore / 3f)
			{
				cardProgressStripe.color = startingProgressColor;
			}
			else if (num < MissionController.passingScore)
			{
				Tween.Kill(cardProgressStripe, "color");
				Tween.Add(cardProgressStripe, "color", middleProgressColor, 0.4f, Cubic.InOut);
			}
			else
			{
				Tween.Kill(cardProgressStripe, "color");
				Tween.Add(cardProgressStripe, "color", completeProgressColor, 0.4f, Cubic.InOut);
			}
		}

		public void ClearDataReceived()
		{
			m_dataReceived = false;
		}

		public void UpdateSubmenuNavigation(UINavigation p_rightTest)
		{
			if (!menuOpened)
			{
				navigation.right = p_rightTest;
				navigation.down = null;
				if (p_rightTest != null)
				{
					p_rightTest.left = navigation;
				}
				return;
			}
			navigation.down = startButtonNavigation;
			startButtonNavigation.up = navigation;
			if (lessonsMenuFirstRow.Count == 0)
			{
				return;
			}
			List<UICardButtonLesson> list = lessonsMenuFirstRow.GetList<UICardButtonLesson>();
			List<UICardButtonLesson> list2 = lessonsMenuSecondRow.GetList<UICardButtonLesson>();
			navigation.right = list[0].navigation;
			list[0].navigation.left = navigation;
			if (list.Count > 1)
			{
				list[0].navigation.right = list[1].navigation;
				if (list2.Count > 0)
				{
					list2[0].navigation.up = list[0].navigation;
					list[0].navigation.down = list2[0].navigation;
					list2[0].navigation.left = navigation;
				}
				if (list2.Count > 1)
				{
					list2[0].navigation.right = list2[1].navigation;
				}
				for (int i = 1; i < list.Count; i++)
				{
					list[i].navigation.left = list[i - 1].navigation;
					if (i < list.Count - 1)
					{
						list[i].navigation.right = list[i + 1].navigation;
						if (i >= list2.Count && list2.Count > 0)
						{
							list[i].navigation.down = list2[list2.Count - 1];
						}
					}
					if (i < list2.Count)
					{
						list2[i].navigation.left = list2[i - 1].navigation;
						list[i].navigation.down = list2[i].navigation;
						list2[i].navigation.up = list[i].navigation;
					}
					if (i < list2.Count - 1)
					{
						list2[i].navigation.right = list2[i + 1].navigation;
					}
				}
				list[list.Count - 1].navigation.right = p_rightTest;
				if (p_rightTest != null)
				{
					p_rightTest.left = list[list.Count - 1].navigation;
				}
				if (list2.Count > 0)
				{
					list2[list2.Count - 1].navigation.right = p_rightTest;
				}
			}
			else
			{
				list[0].navigation.right = p_rightTest;
				if (p_rightTest != null)
				{
					p_rightTest.left = list[0].navigation;
				}
			}
		}

		public void ResetTestAttempts()
		{
			if (!data || !data.testMission)
			{
				return;
			}
			ServiceModel sm = base.app.model.service;
			if (!sm)
			{
				return;
			}
			sm.GetLeaderboardQuest(data.testMission, delegate(DRLLeaderboardData[] p_r)
			{
				int score = p_r[0].score;
				sm.SetLeaderboardMission(data.testMission, score, p_force: true, delegate(DRLLeaderboardData p_result)
				{
					if (this == null || p_result == null)
					{
						Debug.LogWarning("DmvTest " + data.testMission.name + "> SetLeaderboard - Failed to send results!");
					}
				}, 0);
			});
		}

		private void OnDisable()
		{
			if (m_lockdownTimer != null)
			{
				m_lockdownTimer.Stop();
				m_lockdownTimer = null;
			}
		}
	}
}
