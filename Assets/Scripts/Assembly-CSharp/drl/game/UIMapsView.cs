using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIMapsView : UIScreenView
	{
		public ListComponent listField;

		public LayoutElement largeTilesContainer;

		public UINavigation backButton;

		public GameObject raceBackground;

		public GameObject freestyleBackground;

		public GameObject simpleCourseMaps;

		public GameObject multiGPMaps;

		public GameObject communityMapsCard;

		public GameObject drlMapsCard;

		public GameObject usafScreensCard;

		public GameObject megaMapsCard;

		public GameObject maps;

		public GameObject drlMaps;

		public GameObject separator;

		public GameObject categoryLayout;

		public UINavigationScroll navScroll;

		private List<GameObject> usafCards = new List<GameObject>();

		public List<string> allowedMaps;

		private Dictionary<string, Tuple<UIMapCategory, GameObject>> m_mapCategories = new Dictionary<string, Tuple<UIMapCategory, GameObject>>();

		public void SetForGameType(GameFlag p_type, LayoutGroup rightLayoutGroup)
		{
			UINavigation.Link(rightLayoutGroup, backButton);
			backButton.right = rightLayoutGroup;
		}

		public void SetCategoriesEnabled(bool p_simple_courses = false, bool p_community_maps = false, bool p_multigp_maps = false, bool p_drl_maps = false, bool p_mega_maps = false, bool p_collectable_maps = false)
		{
			largeTilesContainer.gameObject.SetActive(p_simple_courses || p_community_maps || p_multigp_maps || p_drl_maps || p_mega_maps);
			simpleCourseMaps.SetActive(p_simple_courses);
			communityMapsCard.SetActive(p_community_maps);
			multiGPMaps.SetActive(p_multigp_maps);
			drlMapsCard.SetActive(p_drl_maps);
			megaMapsCard.SetActive(p_mega_maps);
		}

		public void SetRatingsAvailable(bool p_available)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				UICardButtonMap uICardButtonMap = listField.Get<UICardButtonMap>(i);
				if (uICardButtonMap != null && uICardButtonMap.stars != null)
				{
					uICardButtonMap.stars.fade.alpha = (p_available ? 1f : 0f);
					uICardButtonMap.stars.Clear();
					uICardButtonMap.stars.SetProgress(0f);
				}
			}
		}

		public void Clear()
		{
			listField.Clear();
			foreach (KeyValuePair<string, Tuple<UIMapCategory, GameObject>> mapCategory in m_mapCategories)
			{
				UnityEngine.Object.Destroy(mapCategory.Value.Item1.gameObject);
				UnityEngine.Object.Destroy(mapCategory.Value.Item2);
			}
			m_mapCategories.Clear();
			foreach (GameObject usafCard in usafCards)
			{
				UnityEngine.Object.Destroy(usafCard);
			}
			usafCards.Clear();
		}

		public void Add(DRLMap p_map)
		{
			if (!p_map)
			{
				Debug.LogWarning("UIMapsView> Add - Invalid Map");
				return;
			}
			UICardButtonMap uICardButtonMap = listField.Push<UICardButtonMap>();
			if (!(uICardButtonMap == null))
			{
				uICardButtonMap.notification = "fly.map-card";
				uICardButtonMap.Set(p_map);
				if (p_map.name == "map-usaf")
				{
					UICardButtonLarge component = usafScreensCard.GetComponent<UICardButtonLarge>();
					uICardButtonMap.image = component.previewField.texture;
					uICardButtonMap.preview = component.previewField.texture;
					uICardButtonMap.notification = component.notification;
				}
			}
		}

		public void Set(List<DRLMap> p_list)
		{
			Clear();
			for (int i = 0; i < p_list.Count; i++)
			{
				Add(p_list[i]);
			}
		}
	}
}
