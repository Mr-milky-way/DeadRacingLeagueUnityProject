using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICircuitMapItemView : UICardView
	{
		[Header("UI References")]
		public Image lockIcon;

		public Image lockOverlay;

		public RawImage mapPreview;

		public GameObject lockedContainer;

		public GameObject timeFooter;

		public Text mapTimeText;

		public Text mapTitle;

		public Text mapSubTitle;

		[HideInInspector]
		public DRLMap map;

		[HideInInspector]
		public DRLMapTrack track;

		[HideInInspector]
		public MapData customMapData;

		[HideInInspector]
		public DRLCircuitMapData mapData;

		private float mapTime;

		public float MapTime
		{
			get
			{
				return mapTime;
			}
			set
			{
				mapTime = value;
				mapTimeText.text = Format.SecondsToMMSSFFF(mapTime);
			}
		}

		public bool isComplete { get; set; }

		public bool isLocked { get; set; }

		public void Set(DRLCircuitMapData p_data)
		{
			mapPreview.enabled = true;
			StorageModel storage = base.app.model.storage;
			map = storage.library.FindByGUID<DRLMap>(p_data.mapId);
			if (!p_data.isCustom)
			{
				track = storage.GetMapTrack(p_data.mapId, p_data.trackId, p_freestyle: false);
			}
			else
			{
				track = storage.GetMapTracks(map, GameFlag.Freestyle)[0];
				customMapData = storage.maps.FindByGUID(p_data.trackId);
			}
			mapPreview.texture = map.preview;
			mapTitle.text = map.label.ToUpper();
			mapSubTitle.text = ((!p_data.isCustom) ? track.title.ToUpper() : ((customMapData == null) ? "" : customMapData.mapTitle.ToUpper()));
		}

		public void LockMap()
		{
			lockIcon.enabled = true;
			lockOverlay.enabled = true;
			isLocked = true;
			HideFooter();
		}

		public void UnlockMap()
		{
			lockIcon.enabled = false;
			lockOverlay.enabled = false;
			isLocked = false;
		}

		public void SetTrackComplete(float p_time)
		{
			SetMapTime(p_flag: true, p_time);
			isComplete = true;
		}

		public void ResetTrackComplete()
		{
			SetMapTime(p_flag: false);
			isComplete = false;
		}

		private void SetMapTime(bool p_flag, float p_time = 0f)
		{
			timeFooter.SetActive(p_flag);
			if (!p_flag)
			{
				p_time = 0f;
			}
			MapTime = p_time;
		}

		public void HideFooter()
		{
			timeFooter.SetActive(value: false);
		}
	}
}
