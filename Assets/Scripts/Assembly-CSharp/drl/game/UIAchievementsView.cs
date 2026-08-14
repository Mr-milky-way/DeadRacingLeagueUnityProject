using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievementsView : UIScreenView
	{
		public ListComponent listField;

		[SerializeField]
		private List<DRLAchievementData> achievementDataList = new List<DRLAchievementData>();

		public DRLPagePickerView pageField;

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public GameObject feedback;

		public void Clear()
		{
			listField.Clear();
			achievementDataList = new List<DRLAchievementData>();
		}

		public void UpdateAchievementList(List<DRLAchievementData> p_achievements, int p_page, int p_page_length, int p_pages_count = -1, bool p_allow_search = false)
		{
			List<DRLAchievementData> collection = ((p_achievements == null) ? new List<DRLAchievementData>() : p_achievements);
			collection = new List<DRLAchievementData>(collection);
			int num = ((p_page_length > 0) ? ((collection.Count - 1) / p_page_length) : 0) + 1;
			if (p_pages_count > 0)
			{
				num = p_pages_count;
			}
			int num2 = Mathf.Clamp(p_page, 0, num - 1);
			List<DRLAchievementData> list = new List<DRLAchievementData>();
			int num3 = ((collection.Count > p_page_length) ? Mathf.Max(0, num2 * p_page_length) : 0);
			for (int i = 0; i < p_page_length; i++)
			{
				if (num3 >= collection.Count)
				{
					break;
				}
				DRLAchievementData item = collection[num3];
				list.Add(item);
				num3++;
			}
			List<DRLAchievementData> list2 = new List<DRLAchievementData>();
			List<DRLAchievementData> list3 = new List<DRLAchievementData>();
			if (achievementDataList == null)
			{
				achievementDataList = new List<DRLAchievementData>();
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!ContainsAchievement(achievementDataList, list[j]))
				{
					achievementDataList.Add(list[j]);
				}
			}
			for (int k = 0; k < achievementDataList.Count; k++)
			{
				if (!ContainsAchievement(list, achievementDataList[k]))
				{
					list3.Add(achievementDataList[k]);
				}
			}
			for (int l = 0; l < list3.Count; l++)
			{
				RemoveAchievement(list3[l]);
			}
			for (int m = 0; m < list2.Count; m++)
			{
				if (achievementDataList.Count < p_page_length)
				{
					AddAchievement(list2[m]);
				}
			}
			for (int n = 0; n < list.Count; n++)
			{
				int achievementIndex = GetAchievementIndex(list[n]);
				if (achievementIndex >= 0)
				{
					achievementDataList[achievementIndex] = list[n];
				}
			}
			for (int num4 = 0; num4 < achievementDataList.Count; num4++)
			{
				UpdateAchievement(achievementDataList[num4]);
			}
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			if (num > 1)
			{
				fade.FadeIn(0.3f);
			}
			else
			{
				fade.FadeOut(0.3f);
			}
			UpdateNavigation(num);
			pageField.Set(num);
			pageField.index = num2;
		}

		public void AddAchievement(DRLAchievementData p_data)
		{
			achievementDataList.Add(p_data);
			AchievementCardView achievementCardView = listField.Push<AchievementCardView>();
			achievementCardView.GetComponentInChildren<UIAchievementButtonView>().AchievementID = p_data.id;
			achievementCardView.Set(p_data);
		}

		public void RemoveAchievement(DRLAchievementData p_data)
		{
			for (int i = 0; i < achievementDataList.Count; i++)
			{
				if (achievementDataList[i].id == p_data.id)
				{
					achievementDataList.RemoveAt(i);
					break;
				}
			}
			for (int j = 0; j < listField.Count; j++)
			{
				AchievementCardView achievementCardView = listField.Get<AchievementCardView>(j);
				if ((bool)achievementCardView && achievementCardView.Id.ToString() == p_data.id)
				{
					listField.Remove(j);
					break;
				}
			}
		}

		public bool ContainsAchievement(List<DRLAchievementData> p_list, DRLAchievementData p_achievement)
		{
			if (p_achievement == null)
			{
				return false;
			}
			if (p_list == null)
			{
				return false;
			}
			if (p_list.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i].id == p_achievement.id)
				{
					return true;
				}
			}
			return false;
		}

		public int GetAchievementIndex(DRLAchievementData p_achievement)
		{
			for (int i = 0; i < achievementDataList.Count; i++)
			{
				if (achievementDataList[i].id == p_achievement.id)
				{
					return i;
				}
			}
			return -1;
		}

		public void UpdateAchievement(DRLAchievementData p_data)
		{
			AchievementCardView byAchievementId = GetByAchievementId(p_data.id);
			if ((bool)byAchievementId)
			{
				byAchievementId.AchievementData = p_data;
				byAchievementId.Set(p_data);
			}
		}

		public AchievementCardView GetByAchievementId(string p_id)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				AchievementCardView achievementCardView = listField.Get<AchievementCardView>(i);
				if (!(achievementCardView == null) && achievementCardView.Id.ToString() == p_id)
				{
					return achievementCardView;
				}
			}
			return null;
		}

		protected void UpdateNavigation(int p_totalPages)
		{
			_ = listField;
			List<UINavigation> entry_navs = new List<UINavigation>();
			List<UINavigation> fly_navs = new List<UINavigation>();
			List<UINavigation> add_navs = new List<UINavigation>();
			new List<UINavigation>();
			List<UINavigation> del_navs = new List<UINavigation>();
			new List<UINavigation>();
			new List<UINavigation>();
			new List<UINavigation>();
			UINavigation page_nav = ((p_totalPages > 1) ? pageField.GetComponent<UINavigation>() : null);
			if (p_totalPages <= 1)
			{
				return;
			}
			((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (pageField.selection == null)
				{
					return true;
				}
				page_nav = pageField.selection.GetComponent<UINavigation>();
				int count = entry_navs.Count;
				if (count > 0)
				{
					entry_navs[count - 1].down = page_nav;
					fly_navs[count - 1].down = page_nav;
					add_navs[count - 1].down = page_nav;
					del_navs[count - 1].down = page_nav;
				}
				return false;
			}, 0f);
		}

		public void SetFeedback(bool p_state, float p_delay)
		{
			float feedback_alpha = (p_state ? 1f : (-0.1f));
			float content_alpha = (p_state ? (-0.1f) : 1f);
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0f, 0f, Cubic.Out);
				feedback.SetActive(value: true);
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}
	}
}
