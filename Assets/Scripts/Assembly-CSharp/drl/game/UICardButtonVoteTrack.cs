using UnityEngine;
using UnityEngine.UI;
using drl.backend;

namespace drl.game
{
	public class UICardButtonVoteTrack : UICardButtonLarge
	{
		public Text voteField;

		public Text trackNameField;

		public Image outlineField;

		public DRLMap map;

		public DRLMapTrack track;

		public MapData mapData;

		public override UICardType type => UICardType.ButtonVoteTrack;

		public int vote
		{
			set
			{
				voteField.text = Mathf.Max(0, value).ToString("0") + " VOTES";
			}
		}

		public string guid
		{
			get
			{
				if (track != null)
				{
					return track.guid;
				}
				if (mapData != null)
				{
					return mapData.guid;
				}
				return null;
			}
		}

		public override void Build()
		{
			base.Build();
		}

		public void Set(DRLMapTrack p_track)
		{
			if ((bool)p_track)
			{
				track = p_track;
				mapData = null;
				map = p_track.map;
				if ((bool)map)
				{
					base.label = map.title.ToUpper();
					base.preview = map.preview;
					trackNameField.text = track.title.ToUpper();
					vote = 0;
				}
			}
		}

		public void Set(MapData p_mapData)
		{
			if (p_mapData != null)
			{
				mapData = p_mapData;
				track = null;
				map = base.app.model.storage.library.FindByGUID<DRLMap>(mapData.mapId);
				if ((bool)map)
				{
					base.label = map.title.ToUpper();
					base.preview = map.preview;
					trackNameField.text = mapData.mapTitle;
					vote = 0;
				}
			}
		}

		public void Set(DRLCommunityMapData p_mapData)
		{
			if (p_mapData != null)
			{
				map = base.app.model.storage.library.FindByGUID<DRLMap>(p_mapData.mapId);
				if ((bool)map)
				{
					base.label = map.title;
					base.preview = map.preview;
					trackNameField.text = mapData.mapTitle;
					vote = 0;
				}
			}
		}

		public void Hilight(bool p_flag)
		{
			outlineField.enabled = p_flag;
		}
	}
}
