using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UIMapOverviewView : UIScreenView
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

		public void Set(DRLMap p_data, List<object> p_content)
		{
			Clear();
			bool flag = category == GameFlag.MapDRLSimCup || category == GameFlag.MapVirtualSeason;
			data = p_data;
			if ((bool)card)
			{
				card.Set(data, !flag);
			}
			if (category != GameFlag.MapMultiGP)
			{
				p_content.Sort(delegate(object item1, object item2)
				{
					int num3 = ((item1 is MapData) ? ((MapData)item1).mapDifficulty : ((DRLMapTrack)item1).difficulty);
					int value = ((item2 is MapData) ? ((MapData)item2).mapDifficulty : ((DRLMapTrack)item2).difficulty);
					return num3.CompareTo(value);
				});
				p_content.Sort(delegate(object item1, object item2)
				{
					string text3 = ((item1 is MapData) ? ((MapData)item1).mapTitle : ((DRLMapTrack)item1).title);
					string strB = ((item2 is MapData) ? ((MapData)item2).mapTitle : ((DRLMapTrack)item2).title);
					int num3 = ((item1 is MapData) ? ((MapData)item1).mapDifficulty : ((DRLMapTrack)item1).difficulty);
					int num4 = ((item2 is MapData) ? ((MapData)item2).mapDifficulty : ((DRLMapTrack)item2).difficulty);
					return (num3 != num4) ? num3.CompareTo(num4) : text3.CompareTo(strB);
				});
				if (category == GameFlag.MapVirtualSeason)
				{
					p_content.Sort(delegate(object item1, object item2)
					{
						string obj3 = ((item1 is MapData) ? ((MapData)item1).mapTitle : ((DRLMapTrack)item1).title);
						string strB = ((item2 is MapData) ? ((MapData)item2).mapTitle : ((DRLMapTrack)item2).title);
						return obj3.CompareTo(strB);
					});
				}
				for (int num = 0; num < p_content.Count; num++)
				{
					object obj = p_content[num];
					if (obj is DRLMapTrack)
					{
						Add((DRLMapTrack)obj);
					}
					if (obj is MapData)
					{
						Add((MapData)obj);
					}
				}
				return;
			}
			ProgressionStateModel progression = base.app.model.storage.state.player.progression;
			multiGPTracks.SetActive(value: true);
			tracks.SetActive(value: false);
			for (int num2 = p_content.Count - 1; num2 >= 0; num2--)
			{
				DRLMapTrack dRLMapTrack = null;
				MapData mapData = null;
				if (p_content[num2] is DRLMapTrack)
				{
					dRLMapTrack = p_content[num2] as DRLMapTrack;
				}
				if (p_content[num2] is MapData)
				{
					mapData = p_content[num2] as MapData;
				}
				if (!(dRLMapTrack == null) || mapData != null)
				{
					string text = ((mapData != null) ? mapData.guid : (dRLMapTrack ? dRLMapTrack.guid : ""));
					int p_xp_total = ((!string.IsNullOrEmpty(text)) ? progression.GetTrackXP(text) : 0);
					if (mapData != null)
					{
						if (mapData.mapTitle.Contains("UTT"))
						{
							mapData.mapGroups = "UTT";
						}
						if (mapData.mapTitle.Contains("2015"))
						{
							mapData.mapGroups = "2015";
						}
						if (mapData.mapTitle.Contains("2016"))
						{
							mapData.mapGroups = "2016";
						}
						if (mapData.mapTitle.Contains("2017"))
						{
							mapData.mapGroups = "2017";
						}
						if (mapData.mapTitle.Contains("2018"))
						{
							mapData.mapGroups = "2018";
						}
						if (mapData.mapTitle.Contains("2019"))
						{
							mapData.mapGroups = "2019";
						}
					}
					if (!string.IsNullOrEmpty((dRLMapTrack == null) ? mapData.mapGroups : dRLMapTrack.groups))
					{
						string text2 = ((dRLMapTrack == null) ? mapData.GetGroups()[0] : dRLMapTrack.GetGroups()[0]);
						if (text2.StartsWith("@"))
						{
							text2 = text2.Remove(0, 1);
							text2 = base.app.model.storage.locale.Get(text2, "");
						}
						if (!m_trackCategories.ContainsKey(text2))
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(separator, multiGPTracks.transform);
							GameObject obj2 = UnityEngine.Object.Instantiate(multiGPCategories, multiGPTracks.transform);
							gameObject.name = "separator";
							obj2.name = text2;
							UIMapCategory component = obj2.GetComponent<UIMapCategory>();
							if (component != null)
							{
								m_trackCategories.Add(text2, new Tuple<UIMapCategory, GameObject>(component, gameObject));
								gameObject.SetActive(value: true);
								component.headerLabel.text = text2;
								component.Add(p_content[num2], "fly.map-card", (mapData == null) ? null : data, p_xp_total);
							}
						}
						else
						{
							m_trackCategories[text2].Item1.Add(p_content[num2], "fly.map-card", (mapData == null) ? null : data, p_xp_total);
						}
					}
				}
			}
			List<GridLayoutGroup> list = new List<GridLayoutGroup>();
			foreach (KeyValuePair<string, Tuple<UIMapCategory, GameObject>> trackCategory in m_trackCategories)
			{
				GridLayoutGroup component2 = trackCategory.Value.Item1.list.GetComponent<GridLayoutGroup>();
				UINavigation.Link(component2);
				list.Add(component2);
			}
			UINavigation.LinkGrids(list, base.leftNavigation);
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
				uICardButtonMapTrack.SetFavoriteActive(p_active: true);
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
				bool flag = dRLMapFavoriteData != null;
				DRLMap p_map = data;
				if (flag)
				{
					p_map = base.app.model.storage.library.FindByGUID<DRLMap>(dRLMapFavoriteData.mapId);
				}
				uICardButtonMapTrack.Set(p_data, p_map);
				uICardButtonMapTrack.SetProgression(progression2);
				uICardButtonMapTrack.SetFavoriteToggleOn(flag);
				uICardButtonMapTrack.SetFavoriteActive(p_active: true);
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
