using UnityEngine;
using drl.sim;
using thelab.mvc;

namespace drl.game
{
	public class ReplayModel : Model<DRLApp>
	{
		public TextAsset defaultRig;

		public DroneRigData rig
		{
			get
			{
				if (!defaultRig)
				{
					return null;
				}
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(defaultRig.bytes);
				return droneRigData;
			}
		}
	}
}
