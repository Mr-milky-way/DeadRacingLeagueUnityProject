using drl.game;

namespace drl.sim
{
	public class DroneAssetTag : DRLAssetTag<DroneAssetTagType>
	{
		public DroneAssetTagType brand => Find(DroneAssetTagType.__Brand__, DroneAssetTagType.__Model__);

		public DroneAssetTagType model => Find(DroneAssetTagType.__Model__, DroneAssetTagType.__TagEnd__);

		public DroneAssetTagType category => Find(DroneAssetTagType.__Category__, DroneAssetTagType.__Metric__);
	}
}
