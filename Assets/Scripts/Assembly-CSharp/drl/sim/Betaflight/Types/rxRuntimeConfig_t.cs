namespace drl.sim.Betaflight.Types
{
	public struct rxRuntimeConfig_t
	{
		public int channelCount;

		public int rxRefreshRate;

		private int rcReadRawFn;

		private int rcFrameStatusFn;
	}
}
