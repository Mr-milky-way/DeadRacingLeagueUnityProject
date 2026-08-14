namespace drl.sim.thread
{
	public struct Rates
	{
		public byte Roll;

		public byte Pitch;

		public byte Yaw;

		public byte Throttle;

		public Rates(byte _roll, byte _pitch, byte _yaw, byte _throttle)
		{
			Roll = _roll;
			Pitch = _pitch;
			Yaw = _yaw;
			Throttle = _throttle;
		}
	}
}
