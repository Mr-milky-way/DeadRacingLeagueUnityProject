namespace drl.sim
{
	public class DroneReceiver : DronePart
	{
		public int channel;

		public SignalVector signal;

		public override string GetPrefix()
		{
			return "RC";
		}

		public void ClearSignal()
		{
			signal.throttle = 0f;
			signal.yaw = 0f;
			signal.pitch = 0f;
			signal.roll = 0f;
			signal.altitude = -1f;
		}
	}
}
