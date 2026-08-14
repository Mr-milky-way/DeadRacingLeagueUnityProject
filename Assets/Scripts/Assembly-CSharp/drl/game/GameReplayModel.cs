using System.Collections.Generic;
using drl.sim;
using thelab.mvc;

namespace drl.game
{
	public class GameReplayModel : Model<DRLApp>
	{
		public BlackboxRecord gameReplay;

		public ReplayRecord gameReplayV2;

		public bool hasGameReplay
		{
			get
			{
				if (!ReplayFile.EnableVersion2)
				{
					return gameReplay != null;
				}
				return gameReplayV2 != null;
			}
		}

		public List<BlackboxData> gameReplayClips
		{
			get
			{
				if (!hasGameReplay)
				{
					return new List<BlackboxData>();
				}
				return gameReplay.clips;
			}
		}

		public List<ReplayFile> gameReplayClipsV2
		{
			get
			{
				if (!hasGameReplay)
				{
					return new List<ReplayFile>();
				}
				return gameReplayV2.replays;
			}
		}

		public ReplayRecorderModel recorder => AssertFind<ReplayRecorderModel>("recorder");

		public ReplayPlayerModel player => AssertFind<ReplayPlayerModel>("player");
	}
}
