namespace Epic.OnlineServices.Sanctions
{
	public class CopyPlayerSanctionByIndexOptions
	{
		public ProductUserId TargetUserId { get; set; }

		public uint SanctionIndex { get; set; }
	}
}
