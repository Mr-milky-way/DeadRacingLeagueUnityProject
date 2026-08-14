using System;
using drl.sim;

namespace drl.game
{
	[Serializable]
	public class MapLoadData
	{
		public DRLMap baseMap;

		public DRLMapTrack baseTrack;

		public MapData customMap;

		public OpponentModeType opponentMode;

		public BlackboxRecord opponentRecord;

		public ReplayRecord opponentRecordV2;

		public bool isCustom => customMap != null;

		public MapLoadData(DRLMap p_map, DRLMapTrack p_track, MapData p_customMap, OpponentModeType p_opponentMode, BlackboxRecord p_opponentRecord = null, ReplayRecord p_opponentRecordV2 = null)
		{
			baseMap = p_map;
			baseTrack = p_track;
			customMap = p_customMap;
			opponentMode = p_opponentMode;
			opponentRecord = p_opponentRecord;
			opponentRecordV2 = p_opponentRecordV2;
		}
	}
}
