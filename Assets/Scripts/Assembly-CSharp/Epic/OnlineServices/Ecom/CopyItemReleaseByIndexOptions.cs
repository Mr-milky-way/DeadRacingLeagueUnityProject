namespace Epic.OnlineServices.Ecom
{
	public class CopyItemReleaseByIndexOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string ItemId { get; set; }

		public uint ReleaseIndex { get; set; }
	}
}
