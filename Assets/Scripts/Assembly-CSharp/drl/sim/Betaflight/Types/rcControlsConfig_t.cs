namespace drl.sim.Betaflight.Types
{
	public class rcControlsConfig_t
	{
		public int deadband;

		public int yaw_deadband;

		public int alt_hold_deadband = 40;

		public int alt_hold_fast_change = 1;

		public bool yaw_control_reversed;
	}
}
