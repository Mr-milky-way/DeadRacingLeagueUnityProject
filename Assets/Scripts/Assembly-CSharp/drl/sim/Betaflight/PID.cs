namespace drl.sim.Betaflight
{
	public class PID
	{
		private static float itermAccelerator = 1f;

		public static int targetPidLooptime;

		public static void pidSetItermAccelerator(float newItermAccelerator)
		{
			itermAccelerator = newItermAccelerator;
		}
	}
}
