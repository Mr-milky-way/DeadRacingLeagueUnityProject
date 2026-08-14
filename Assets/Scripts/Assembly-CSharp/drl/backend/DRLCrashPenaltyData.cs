using thelab.core;

namespace drl.backend
{
	public class DRLCrashPenaltyData : SerializedData
	{
		public float damageTier1 => Get("damage-tier-1", 0.1f);

		public float damageTier2 => Get("damage-tier-2", 0.25f);

		public float damageTier3 => Get("damage-tier-3", 1f);

		public float speedReduction1 => Get("speed-reduction-1", 0.15f);

		public float speedReduction2 => Get("speed-reduction-2", 0.3f);

		public float speedReduction3 => Get("speed-reduction-3", 0.5f);

		public float lineDeviation1 => Get("line-deviation-1", 0.1f);

		public float lineDeviation2 => Get("line-deviation-2", 0.2f);

		public float lineDeviation3 => Get("line-deviation-3", 0.3f);

		public float crashEnergy => Get("crash-energy", 200f);

		public float damageEnergy => Get("damage-energy", 50f);

		public float spinoutAmount => Get("spinout-amount", 0.25f);

		public float energyTransferRate => Get("energy-transfer", 0.55f);
	}
}
