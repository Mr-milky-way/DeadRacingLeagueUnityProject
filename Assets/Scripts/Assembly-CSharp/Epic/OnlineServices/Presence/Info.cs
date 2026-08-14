namespace Epic.OnlineServices.Presence
{
	public class Info : ISettable
	{
		public Status Status { get; set; }

		public EpicAccountId UserId { get; set; }

		public string ProductId { get; set; }

		public string ProductVersion { get; set; }

		public string Platform { get; set; }

		public string RichText { get; set; }

		public DataRecord[] Records { get; set; }

		public string ProductName { get; set; }

		internal void Set(InfoInternal? other)
		{
			if (other.HasValue)
			{
				Status = other.Value.Status;
				UserId = other.Value.UserId;
				ProductId = other.Value.ProductId;
				ProductVersion = other.Value.ProductVersion;
				Platform = other.Value.Platform;
				RichText = other.Value.RichText;
				Records = other.Value.Records;
				ProductName = other.Value.ProductName;
			}
		}

		public void Set(object other)
		{
			Set(other as InfoInternal?);
		}
	}
}
