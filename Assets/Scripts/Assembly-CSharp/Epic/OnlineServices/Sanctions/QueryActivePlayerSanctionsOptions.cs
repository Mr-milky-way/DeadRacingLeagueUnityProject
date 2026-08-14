namespace Epic.OnlineServices.Sanctions
{
	public class QueryActivePlayerSanctionsOptions
	{
		public ProductUserId TargetUserId { get; set; }

		public ProductUserId LocalUserId { get; set; }
	}
}
