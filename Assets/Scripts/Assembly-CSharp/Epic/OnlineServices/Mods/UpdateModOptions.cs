namespace Epic.OnlineServices.Mods
{
	public class UpdateModOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public ModIdentifier Mod { get; set; }
	}
}
