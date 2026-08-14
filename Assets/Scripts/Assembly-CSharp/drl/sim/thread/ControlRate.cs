namespace drl.sim.thread
{
	public struct ControlRate
	{
		public byte thrMid8;

		public byte thrExpo8;

		public byte rates_type;

		public byte[] rcRates;

		public byte[] rcExpo;

		public byte[] rates;

		public byte dynThrPID;

		public ushort tpa_breakpoint;

		public byte throttle_limit_type;

		public byte throttle_limit_percent;
	}
}
