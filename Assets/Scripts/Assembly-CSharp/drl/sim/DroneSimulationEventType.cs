namespace drl.sim
{
	public enum DroneSimulationEventType
	{
		None = 0,
		Initialize = 1,
		Run = 2,
		Start = 3,
		Stop = 4,
		PauseChange = 5,
		ChangeSpeed = 6,
		DroneAdd = 7,
		DroneReady = 8,
		CameraAdd = 9,
		DroneRemove = 10,
		AllDronesReady = 11,
		DroneNanRecover = 12
	}
}
