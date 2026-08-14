namespace Epic.OnlineServices.Reports
{
	public class SendPlayerBehaviorReportOptions
	{
		public ProductUserId ReporterUserId { get; set; }

		public ProductUserId ReportedUserId { get; set; }

		public PlayerReportsCategory Category { get; set; }

		public string Message { get; set; }

		public string Context { get; set; }
	}
}
