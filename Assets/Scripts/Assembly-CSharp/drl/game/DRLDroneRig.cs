using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class DRLDroneRig : DRLGameAsset
	{
		public TextAsset rigFile;

		[TextArea(2, 2)]
		public string title;

		[TextArea(2, 6)]
		public string description;

		public bool allowLeaderboard;

		public DroneRigData rig
		{
			get
			{
				if (!rigFile)
				{
					return null;
				}
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(rigFile.bytes);
				return droneRigData;
			}
		}

		public string label => title.Replace("\n", " ");

		public override string GetPrefix()
		{
			return "DR";
		}
	}
}
