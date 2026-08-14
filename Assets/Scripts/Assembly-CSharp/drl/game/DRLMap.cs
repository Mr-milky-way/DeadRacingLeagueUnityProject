using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using drl.sim;

namespace drl.game
{
	public class DRLMap : DRLGameAsset
	{
		public Texture background;

		public Texture blur;

		public VideoClip video;

		[TextArea(1, 4)]
		public string caption;

		[TextArea(1, 4)]
		public string title;

		public string scene;

		public int lightingPreset = -1;

		public MapData data;

		[TextArea(1, 4)]
		public string description;

		public int[] droneSizes;

		public DroneRigData[] promoDrones;

		public bool promoDronesOnly = true;

		public string groups;

		public List<string> allowedAssetGroups;

		public string label => title.ToUpper().Replace("\n", " ");

		public bool custom => data != null;

		public override string GetPrefix()
		{
			return "MP";
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
