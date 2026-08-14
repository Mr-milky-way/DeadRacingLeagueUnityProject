namespace drl.sim
{
	public class DroneAntennaTx : DronePart
	{
		public float output = 600f;

		public float range = 2000f;

		public override string GetPrefix()
		{
			return "TX";
		}
	}
}
