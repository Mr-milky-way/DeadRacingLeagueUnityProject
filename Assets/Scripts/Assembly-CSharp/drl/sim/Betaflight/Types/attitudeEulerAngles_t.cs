namespace drl.sim.Betaflight.Types
{
	public class attitudeEulerAngles_t
	{
		public struct Values
		{
			public int roll;

			public int pitch;

			public int yaw;
		}

		public int[] raw = new int[3];

		public Values values;
	}
}
