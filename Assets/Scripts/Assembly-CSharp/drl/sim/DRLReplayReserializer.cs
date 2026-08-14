using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DRLReplayReserializer : MonoBehaviour
	{
		[Serializable]
		public class PlayerHeaderData
		{
			public int clipIdx = -1;

			public string platformId;

			public string playerId;

			public string profileName;

			public string profileColorHex;

			public Color profileColor;
		}

		public TextAsset replayFile;

		[Header("Replay player data:")]
		public List<PlayerHeaderData> playerHeaders;

		public BlackboxRecord record { get; set; }
	}
}
