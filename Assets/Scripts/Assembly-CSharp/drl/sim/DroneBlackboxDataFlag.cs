namespace drl.sim
{
	public enum DroneBlackboxDataFlag : byte
	{
		None = 0,
		Transform = 1,
		Velocity = 2,
		RPM = 4,
		Input = 8,
		PIDControl = 16,
		Event = 32,
		Physics = 64,
		TransformPart = 128,
		All = byte.MaxValue,
		Basic = 175
	}
}
