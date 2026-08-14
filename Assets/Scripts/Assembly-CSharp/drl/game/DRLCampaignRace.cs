using System;

namespace drl.game
{
	[Serializable]
	public class DRLCampaignRace
	{
		public int heats = 2;

		public string[] phaseNames = new string[2] { "SemiFinal", "Final" };

		public DRLMapTrack track;

		public MapData customMap;

		public string customMapId;

		public int phases
		{
			get
			{
				if (phaseNames != null)
				{
					return phaseNames.Length;
				}
				return 0;
			}
		}

		public int total => heats * phases;

		public string mapId
		{
			get
			{
				if (customMap != null)
				{
					return customMap.mapId;
				}
				if ((bool)track)
				{
					return track.map.guid;
				}
				return "";
			}
		}

		public string trackId
		{
			get
			{
				if (customMap != null)
				{
					return customMap.trackId;
				}
				if ((bool)track)
				{
					return track.guid;
				}
				return "";
			}
		}

		public bool hasTrack => track != null;

		public bool isCustomMap => customMap != null;

		public DRLCampaignRace()
		{
			heats = 2;
			phaseNames = new string[2] { "SemiFinal", "Final" };
		}
	}
}
