using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIVoteTrackView : View<DRLApp>
	{
		public RectTransform containerRT;

		public RectTransform cardsRT;

		public FadeComponent backgroundFrontFade;

		public ListComponent cardList;

		public FadeComponent titleFade;

		public Text titleCaptionField;

		public float y
		{
			get
			{
				return containerRT.anchoredPosition.y;
			}
			set
			{
				Vector2 anchoredPosition = containerRT.anchoredPosition;
				anchoredPosition.y = value;
				containerRT.anchoredPosition = anchoredPosition;
			}
		}

		public float cardsY
		{
			get
			{
				return cardsRT.anchoredPosition.y;
			}
			set
			{
				Vector2 anchoredPosition = cardsRT.anchoredPosition;
				anchoredPosition.y = value;
				cardsRT.anchoredPosition = anchoredPosition;
			}
		}

		public bool HasCards => cardList.Count > 0;

		public string caption
		{
			set
			{
				if ((bool)titleCaptionField)
				{
					titleCaptionField.text = value;
				}
			}
		}

		public void Show(float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "y");
			if (p_duration <= 0f)
			{
				y = 210f;
			}
			else
			{
				Tween.Add(this, "y", 210f, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Hide(float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "y");
			if (p_duration <= 0f)
			{
				y = -200f;
			}
			else
			{
				Tween.Add(this, "y", -200f, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Minimize(float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "cardsY");
			if (p_duration <= 0f)
			{
				cardsY = 0f;
				titleFade.alpha = 1f;
				backgroundFrontFade.alpha = 1f;
			}
			else
			{
				titleFade.FadeIn(0.2f, 0.1f + p_delay);
				backgroundFrontFade.FadeIn(0.2f, p_delay);
				Tween.Add(this, "cardsY", 0f, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Maximize(float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "cardsY");
			if (p_duration <= 0f)
			{
				cardsY = 260f;
				titleFade.alpha = -0.1f;
				backgroundFrontFade.alpha = -0.1f;
			}
			else
			{
				titleFade.FadeOut(0.2f, p_delay);
				backgroundFrontFade.FadeOut(0.2f, 0.1f + p_delay);
				Tween.Add(this, "cardsY", 260f, p_duration, p_delay + 0.1f, Cubic.Out);
			}
		}

		public void Clear()
		{
			cardList.Clear();
		}

		public void Add(DRLMapTrack p_track)
		{
			cardList.Push<UICardButtonVoteTrack>().Set(p_track);
		}

		public void Add(MapData p_track)
		{
			cardList.Push<UICardButtonVoteTrack>().Set(p_track);
		}

		public void Add(DRLCommunityMapData p_track)
		{
			cardList.Push<UICardButtonVoteTrack>().Set(p_track);
		}

		public UICardButtonVoteTrack Get(int p_id)
		{
			return cardList.Get<UICardButtonVoteTrack>(p_id);
		}

		public UICardButtonVoteTrack GetByGUID(string p_guid)
		{
			for (int i = 0; i < cardList.Count; i++)
			{
				UICardButtonVoteTrack uICardButtonVoteTrack = Get(i);
				if ((bool)uICardButtonVoteTrack && uICardButtonVoteTrack.guid == p_guid)
				{
					return uICardButtonVoteTrack;
				}
			}
			return null;
		}

		public void HilightByGUID(string p_guid)
		{
			for (int i = 0; i < cardList.Count; i++)
			{
				UICardButtonVoteTrack uICardButtonVoteTrack = Get(i);
				if ((bool)uICardButtonVoteTrack)
				{
					bool p_flag = true;
					if (string.IsNullOrEmpty(uICardButtonVoteTrack.guid))
					{
						p_flag = false;
					}
					if (uICardButtonVoteTrack.guid != p_guid)
					{
						p_flag = false;
					}
					uICardButtonVoteTrack.Hilight(p_flag);
				}
			}
		}

		public void Intialize()
		{
			if (cardList.Count <= 0 && base.app.model.network.room != null)
			{
				List<string> p_tracks = new List<string>();
				Initialize(p_tracks);
			}
		}

		public void Initialize(List<string> p_tracks)
		{
			cardList.Clear();
			for (int i = 0; i < p_tracks.Count; i++)
			{
				DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(p_tracks[i]);
				if ((bool)dRLMapTrack)
				{
					Add(dRLMapTrack);
					continue;
				}
				MapData mapData = base.app.model.storage.maps.FindByGUID(p_tracks[i]);
				if (mapData != null)
				{
					Add(mapData);
					continue;
				}
				if (base.app.arguments.game.map.custom)
				{
					MapData data = base.app.arguments.game.map.data;
					if (data != null)
					{
						Add(data);
						continue;
					}
				}
				Debug.LogError("UIVoteTrackView>Initialize - Could not find Map with guid" + p_tracks[i]);
			}
			cardsY = 0f;
		}

		public void Refresh(Dictionary<string, int> p_table)
		{
			for (int i = 0; i < cardList.Count; i++)
			{
				UICardButtonVoteTrack uICardButtonVoteTrack = Get(i);
				if ((bool)uICardButtonVoteTrack)
				{
					uICardButtonVoteTrack.vote = 0;
				}
			}
			foreach (KeyValuePair<string, int> item in p_table)
			{
				string key = item.Key;
				int value = item.Value;
				UICardButtonVoteTrack byGUID = GetByGUID(key);
				if ((bool)byGUID)
				{
					byGUID.vote = value;
				}
			}
		}
	}
}
