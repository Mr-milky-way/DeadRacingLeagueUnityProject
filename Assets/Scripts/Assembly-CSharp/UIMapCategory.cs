using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.game;
using thelab.core;

public class UIMapCategory : MonoBehaviour
{
	public Text headerLabel;

	public ListComponent list;

	public RectTransform rect;

	public void Add(DRLMap p_map, string p_notification)
	{
		if (!p_map)
		{
			Debug.LogWarning("UIMapCategory> Add - Invalid Map");
		}
		else if (!(list == null))
		{
			UICardButtonMap uICardButtonMap = list.Push<UICardButtonMap>();
			uICardButtonMap.notification = p_notification;
			uICardButtonMap.Set(p_map);
			Refresh();
		}
	}

	public void Add(object p_map, string p_notification, DRLMap p_data = null, int p_xp_total = 0)
	{
		if (p_map == null)
		{
			Debug.LogWarning("UIMapCategory> Add - Invalid Map");
			return;
		}
		UICardButtonMapTrack uICardButtonMapTrack = list.Push<UICardButtonMapTrack>();
		uICardButtonMapTrack.notification = "fly.map-track-card";
		string map_id;
		string track_id;
		if (p_data != null)
		{
			if (!(p_map is MapData mapData))
			{
				return;
			}
			map_id = mapData.mapId;
			track_id = mapData.guid;
			uICardButtonMapTrack.Set(mapData, p_data);
		}
		else
		{
			DRLMapTrack dRLMapTrack = p_map as DRLMapTrack;
			if (dRLMapTrack == null)
			{
				return;
			}
			map_id = dRLMapTrack.map.data.mapId;
			track_id = dRLMapTrack.map.data.guid;
			uICardButtonMapTrack.Set(dRLMapTrack);
		}
		uICardButtonMapTrack.SetProgression(p_xp_total);
		list.Sort(DifficultySort);
		Refresh();
		bool favoriteToggleOn = uICardButtonMapTrack.app.model.storage.state.player.favoriteMaps.Any((DRLMapFavoriteData map) => map.mapId == map_id && map.trackId == track_id);
		uICardButtonMapTrack.SetFavoriteToggleOn(favoriteToggleOn);
		uICardButtonMapTrack.SetFavoriteActive(p_active: true);
	}

	private void Refresh()
	{
		if (list.Count != 0)
		{
			int num = (int)Mathf.Ceil((float)list.Count / 2f);
			rect.sizeDelta = new Vector2(320f * (float)num + (float)(10 * (num - 1)), rect.sizeDelta.y);
		}
	}

	private int DifficultySort(Component x, Component y)
	{
		UICardButtonMapTrack component = x.GetComponent<UICardButtonMapTrack>();
		UICardButtonMapTrack component2 = y.GetComponent<UICardButtonMapTrack>();
		int difficulty = component.difficulty;
		int difficulty2 = component2.difficulty;
		return difficulty.CompareTo(difficulty2);
	}
}
