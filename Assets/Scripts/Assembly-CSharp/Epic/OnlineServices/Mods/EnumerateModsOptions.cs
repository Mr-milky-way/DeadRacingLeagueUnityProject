namespace Epic.OnlineServices.Mods
{
	public class EnumerateModsOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public ModEnumerationType Type { get; set; }
	}
}
