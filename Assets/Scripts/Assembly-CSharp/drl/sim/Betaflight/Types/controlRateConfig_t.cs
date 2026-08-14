namespace drl.sim.Betaflight.Types
{
	public class controlRateConfig_t
	{
		public int rcRate8;

		public int rcYawRate8;

		public int rcExpo8;

		public int thrMid8;

		public int thrExpo8;

		public int[] rates = new int[3];

		public int dynThrPID;

		public int rcYawExpo8;

		public int tpa_breakpoint;
	}
}
