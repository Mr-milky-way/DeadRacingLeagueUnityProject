namespace drl.sim
{
	public enum DroneEventType
	{
		None = -1,
		Ready = 0,
		Armed = 1,
		Disarmed = 2,
		Collision = 3,
		Scrape = 4,
		PropScrape = 5,
		Crash = 6,
		TurtleOn = 7,
		TurtleOff = 8,
		Recover = 9,
		NanRecover = 10,
		WaterImpact = 11,
		ScrapeAudio = 12
	}
}
