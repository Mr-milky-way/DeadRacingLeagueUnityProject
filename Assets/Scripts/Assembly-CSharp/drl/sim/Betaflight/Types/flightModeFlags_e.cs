namespace drl.sim.Betaflight.Types
{
	public enum flightModeFlags_e
	{
		ANGLE_MODE = 1,
		HORIZON_MODE = 2,
		MAG_MODE = 4,
		BARO_MODE = 8,
		GPS_HOME_MODE = 0x10,
		GPS_HOLD_MODE = 0x20,
		HEADFREE_MODE = 0x40,
		UNUSED_MODE = 0x80,
		PASSTHRU_MODE = 0x100,
		SONAR_MODE = 0x200,
		FAILSAFE_MODE = 0x400
	}
}
