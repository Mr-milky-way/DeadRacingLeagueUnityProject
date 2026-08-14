using System;

namespace drl.sim
{
	[Serializable]
	public class DroneBatteryPowerData
	{
		public float totalCapacity;

		public float remainingCharge;

		public float voltageMin;

		public float voltageMax;

		public float voltage;

		public float voltageAvailable;

		public float currentDraw;

		public float currentMax;
	}
}
