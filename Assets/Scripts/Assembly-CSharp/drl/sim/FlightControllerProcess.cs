namespace drl.sim
{
	public enum FlightControllerProcess
	{
		None = 0,
		Level = 1,
		Altitude = 2,
		Limiter = 4,
		Training = 8,
		Lock = 0x10,
		Debug = 0x2000
	}
}
