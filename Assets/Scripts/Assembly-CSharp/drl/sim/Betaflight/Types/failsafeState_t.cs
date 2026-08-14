namespace drl.sim.Betaflight.Types
{
	public struct failsafeState_t
	{
		public int events;

		public bool monitoring;

		public bool active;

		public int rxDataFailurePeriod;

		public int validRxDataReceivedAt;

		public int validRxDataFailedAt;

		public int throttleLowPeriod;

		public int landingShouldBeFinishedAt;

		public int receivingRxDataPeriod;

		public int receivingRxDataPeriodPreset;
	}
}
