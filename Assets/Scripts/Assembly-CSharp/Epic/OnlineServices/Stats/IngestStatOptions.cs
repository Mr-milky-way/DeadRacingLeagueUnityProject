namespace Epic.OnlineServices.Stats
{
	public class IngestStatOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public IngestData[] Stats { get; set; }

		public ProductUserId TargetUserId { get; set; }
	}
}
