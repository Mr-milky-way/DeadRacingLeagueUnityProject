namespace Epic.OnlineServices.Ecom
{
	public class CopyTransactionByIndexOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public uint TransactionIndex { get; set; }
	}
}
