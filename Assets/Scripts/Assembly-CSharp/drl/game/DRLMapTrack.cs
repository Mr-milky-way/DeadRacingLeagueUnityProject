using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class DRLMapTrack : DRLGameAsset
	{
		public DRLMap map;

		public string id;

		[TextArea(1, 4)]
		public string title;

		[TextArea(1, 4)]
		public string description;

		public float length;

		public int difficulty;

		public bool disableDroneOnFinish;

		public string podium = "PD-a6d";

		public int[] droneSizes;

		public DroneRigData[] promoDrones;

		public bool promoDronesOnly = true;

		public string groups;

		public string scene
		{
			get
			{
				if (!map)
				{
					return "";
				}
				return map.scene + "-tracks";
			}
		}

		public string label => title.ToUpper().Replace("\n", " ");

		public bool freestyleOnly
		{
			get
			{
				GameFlagTag component = GetComponent<GameFlagTag>();
				if ((bool)component)
				{
					return !component.Match(GameFlag.Race);
				}
				return false;
			}
		}

		public override string GetPrefix()
		{
			return "MT";
		}

		public List<string> GetGroups()
		{
			List<string> list = new List<string>(groups.Split(','));
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].Trim();
				if (string.IsNullOrEmpty(list[i]))
				{
					list.RemoveAt(i--);
				}
			}
			return list;
		}
	}
}
