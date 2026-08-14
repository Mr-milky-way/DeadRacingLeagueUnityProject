namespace Epic.OnlineServices.Ecom
{
	public class CopyItemImageInfoByIndexOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string ItemId { get; set; }

		public uint ImageInfoIndex { get; set; }
	}
}
