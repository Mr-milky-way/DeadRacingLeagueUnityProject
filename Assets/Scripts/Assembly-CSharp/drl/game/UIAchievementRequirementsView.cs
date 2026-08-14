using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UIAchievementRequirementsView : UIScreenView
	{
		[SerializeField]
		private GameObject achievementRequisiteItem;

		[SerializeField]
		private GameObject flightMasterRequisiteItem;

		[SerializeField]
		private Transform achievementDetailGroup;

		[SerializeField]
		private GridLayoutGroup gridLayoutGroup;

		[SerializeField]
		private UINavigation backButtonNavigation;

		[SerializeField]
		private List<UINavigation> navigationItems = new List<UINavigation>();

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public GameObject feedback;

		public DRLAchievementData data;

		public void UpdateAchievementRequirementsList(List<DRLAchievementRequirementsData> p_achievement_requirements, int p_column_count)
		{
			gridLayoutGroup.constraintCount = p_column_count;
			navigationItems.Clear();
			List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks();
			List<DRLMap> allMaps = base.app.model.storage.GetAllMaps();
			foreach (DRLAchievementRequirementsData p_achievement_requirement in p_achievement_requirements)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(achievementRequisiteItem, achievementDetailGroup);
				navigationItems.Add(gameObject.GetComponent<UINavigation>());
				bool p_isComplete = false;
				if (p_achievement_requirement.progression >= 1f || p_achievement_requirement.completed == 1f)
				{
					p_isComplete = true;
				}
				gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, p_achievement_requirement.lbEntries, p_isComplete);
				foreach (DRLMapTrack item in mapTracks)
				{
					if (p_achievement_requirement.guid == item.m_guid)
					{
						string p_title = Regex.Replace(item.map.title + " / " + item.title, "\\t|\\n|\\r", "").ToUpper();
						gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_title, p_achievement_requirement.lbEntries, p_isComplete, item);
					}
				}
				if (p_achievement_requirement.isCustomMap)
				{
					MapData mapData = base.app.model.storage.maps.FindByGUID(p_achievement_requirement.guid);
					string text = "";
					foreach (DRLMap item2 in allMaps)
					{
						if (mapData != null && mapData.mapId == item2.guid)
						{
							text = item2.title;
						}
					}
					if (mapData != null)
					{
						string p_title2 = Regex.Replace(text + " / " + mapData.mapTitle, "\\t|\\n|\\r", "").ToUpper();
						gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_title2, p_achievement_requirement.lbEntries, p_isComplete, p_achievement_requirement.guid, "community-maps.item.fly");
					}
				}
				if (p_achievement_requirement.title == "3\" Drone")
				{
					gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, "", p_achievement_requirement.progression == 1f, "community-drones.create-new3");
				}
				if (p_achievement_requirement.title == "4\" Drone")
				{
					gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, "", p_achievement_requirement.progression == 1f, "community-drones.create-new4");
				}
				if (p_achievement_requirement.title == "5\" Drone")
				{
					gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, "", p_achievement_requirement.progression == 1f, "community-drones.create-new5");
				}
				if (p_achievement_requirement.title == "6\" Drone")
				{
					gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, "", p_achievement_requirement.progression == 1f, "community-drones.create-new6");
				}
				if (p_achievement_requirement.title == "7\" Drone")
				{
					gameObject.GetComponent<UIAchievementRequirementListItemView>().Set(p_achievement_requirement.title, "", p_achievement_requirement.progression == 1f, "community-drones.create-new7");
				}
			}
			ConfigureNavigation();
		}

		private void ConfigureNavigation()
		{
			for (int num = navigationItems.Count - 1; num >= 0; num--)
			{
				if (num % 2 == 0)
				{
					navigationItems[num].left = backButtonNavigation;
					if (num + 1 < navigationItems.Count)
					{
						navigationItems[num].right = navigationItems[num + 1];
						navigationItems[num + 1].left = navigationItems[num];
					}
					if (num + 2 < navigationItems.Count)
					{
						navigationItems[num].down = navigationItems[num + 2];
						navigationItems[num + 2].up = navigationItems[num];
					}
				}
				else
				{
					if (num > 1)
					{
						navigationItems[num].up = navigationItems[num - 2];
					}
					if (num + 2 < navigationItems.Count)
					{
						navigationItems[num].down = navigationItems[num + 2];
					}
				}
			}
			backButtonNavigation.right = navigationItems[0];
		}

		public void Reset()
		{
			foreach (Transform item in achievementDetailGroup)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
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
