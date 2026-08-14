using System;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	[Serializable]
	public class GameFriendData
	{
		public string name;

		public string platformId;

		public Color color = Colorf.transparent;

		public Texture2D photo;

		public string profileThumbURL;

		public string flagURL;

		public GameFriendStatusType status;

		public bool ingame;

		public bool hasGame;

		public int profileRank;

		public bool online => status != GameFriendStatusType.Offline;
	}
}
