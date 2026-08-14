using System;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	[Serializable]
	public class NetworkGhost : INetworkPlayer
	{
		public string ProfileName;

		public string ProfilePhoto;

		public string DronePhoto;

		public string PlayerBackendId;

		public string ReplayURL;

		public string ProfileColorHex;

		public string DroneRig;

		public int ID => 13;

		public string PlayerId => PlayerBackendId;

		public bool IsMaster => false;

		public bool IsSpectator => false;

		public Color GetProfileColor()
		{
			return Colorf.ParseRGB(ProfileColorHex, Color.yellow);
		}
	}
}
