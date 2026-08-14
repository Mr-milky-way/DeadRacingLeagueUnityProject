namespace drl.sim
{
	public enum FlightControllerMode
	{
		Bypass = 0,
		Acro = 1,
		Beginner = 2,
		Intermediate = 3,
		Pro = 4,
		Lock = 7,
		Baro = 16,
		DJI = 32,
		Target = 64,
		Training = 256,
		Speed = 512,
		Level = 1000,
		Horizon = 1001,
		Air = 1002,
		AcroClassic = 1024,
		Debug = 4096,
		Free = 8192,
		Stabilized = 8193,
		Arcade = 8194
	}
}
