namespace drl.sim
{
	public class DroneAttachment : DronePart
	{
		public DroneAttachmentType type;

		public override string GetPrefix()
		{
			return "AT";
		}
	}
}
