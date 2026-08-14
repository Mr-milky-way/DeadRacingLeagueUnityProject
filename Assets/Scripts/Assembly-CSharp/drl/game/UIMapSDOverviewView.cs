using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UIMapSDOverviewView : UIScreenView
	{
		public ListComponent listField;

		public UICardButtonMap card;

		[SerializeField]
		private UIStatusView m_statusView;

		[SerializeField]
		private FadeComponent m_listFade;

		public GameFlag category = GameFlag.MapDRL;

		public bool isFeatured;

		public bool usaf;

		public bool usafDay;

		public bool usafNight;

		public DRLMap data;

		public List<DRLCommunityMapData> communityMapData = new List<DRLCommunityMapData>();

		public GameObject tracks;

		public GameObject multiGPTracks;

		public GameObject multiGPCategories;

		public GameObject separator;

		private Dictionary<string, Tuple<UIMapCategory, GameObject>> m_trackCategories = new Dictionary<string, Tuple<UIMapCategory, GameObject>>();

		public bool openedFromRoom;

		private static string log = "";

		public void Clear()
		{
			if (!listField)
			{
				return;
			}
			listField.Clear();
			foreach (KeyValuePair<string, Tuple<UIMapCategory, GameObject>> trackCategory in m_trackCategories)
			{
				UnityEngine.Object.Destroy(trackCategory.Value.Item1.gameObject);
				UnityEngine.Object.Destroy(trackCategory.Value.Item2);
			}
			m_trackCategories.Clear();
			tracks.SetActive(value: true);
			multiGPTracks.SetActive(value: false);
		}

		public void Set(DRLMap p_data, List<MapData> p_content)
		{
			data = p_data;
			Clear();
			for (int i = 0; i < p_content.Count; i++)
			{
				Add(p_content[i]);
			}
			UINavigation.Link(tracks.GetComponent<GridLayoutGroup>(), base.leftNavigation);
		}

		private void Add(DRLMapTrack p_data)
		{
			if ((bool)listField && (bool)p_data)
			{
				ProgressionStateModel progression = base.app.model.storage.state.player.progression;
				string text = (p_data ? p_data.guid : "");
				int progression2 = ((!string.IsNullOrEmpty(text)) ? progression.GetTrackXP(text) : 0);
				UICardButtonMapTrack uICardButtonMapTrack = listField.Push<UICardButtonMapTrack>();
				uICardButtonMapTrack.notification = "fly.map-track-card";
				uICardButtonMapTrack.Set(p_data);
				uICardButtonMapTrack.SetProgression(progression2);
				bool favoriteToggleOn = base.app.model.storage.state.player.favoriteMaps.Any((DRLMapFavoriteData map) => map.mapId == p_data.map.guid && map.trackId == p_data.guid);
				uICardButtonMapTrack.SetFavoriteToggleOn(favoriteToggleOn);
			}
		}

		private void Add(MapData p_data)
		{
			if ((bool)listField && p_data != null)
			{
				ProgressionStateModel progression = base.app.model.storage.state.player.progression;
				string item_guid = p_data.guid;
				int progression2 = ((!string.IsNullOrEmpty(item_guid)) ? progression.GetTrackXP(item_guid) : 0);
				UICardButtonMapTrack uICardButtonMapTrack = listField.Push<UICardButtonMapTrack>();
				uICardButtonMapTrack.notification = "fly.map-track-card";
				DRLMapFavoriteData dRLMapFavoriteData = base.app.model.storage.state.player.favoriteMaps.Find((DRLMapFavoriteData map) => map.mapId == p_data.mapId && map.trackId == item_guid);
				bool num = dRLMapFavoriteData != null;
				DRLMap p_map = data;
				if (num)
				{
					p_map = base.app.model.storage.library.FindByGUID<DRLMap>(dRLMapFavoriteData.mapId);
				}
				uICardButtonMapTrack.Set(p_data, p_map);
				uICardButtonMapTrack.SetProgression(progression2);
				uICardButtonMapTrack.SetFavoriteActive(p_active: false);
			}
		}

		public void SetRatingOverall(float p_rating, float p_delay = 0f, float p_item_delay = 0.25f)
		{
			card.SetRating(p_rating, p_delay, p_item_delay);
		}

		public void SetRatingsAvailable(bool p_available)
		{
			if (card.stars != null)
			{
				card.stars.fade.alpha = (p_available ? 1f : 0f);
				card.stars.Clear();
				card.stars.SetProgress(0f);
			}
			for (int i = 0; i < listField.Count; i++)
			{
				UICardButtonMapTrack uICardButtonMapTrack = listField.Get<UICardButtonMapTrack>(i);
				if (uICardButtonMapTrack.stars != null)
				{
					uICardButtonMapTrack.stars.gameObject.SetActive(p_available);
					uICardButtonMapTrack.stars.fade.alpha = (p_available ? 1f : 0f);
					uICardButtonMapTrack.stars.Clear();
					uICardButtonMapTrack.stars.SetProgress(0f);
				}
			}
		}

		public void ShowLoadingUI()
		{
			m_statusView.gameObject.SetActive(value: true);
			base.app.view.audio.PlayUILoadingLoop();
			m_statusView.fade.FadeIn(0f);
			m_listFade.FadeOut(0f);
			m_statusView.SetLoading(0f);
		}

		public void HideLoadingUI(float p_duration = 0.4f, bool p_without_animating = false)
		{
			if (p_without_animating)
			{
				m_statusView.fade.alpha = 0f;
				m_listFade.alpha = 1f;
				m_statusView.gameObject.SetActive(value: false);
			}
			else
			{
				base.app.view.audio.StopUILoadingLoop();
				m_statusView.fade.FadeOut();
				m_listFade.FadeIn();
			}
		}
	}
}
