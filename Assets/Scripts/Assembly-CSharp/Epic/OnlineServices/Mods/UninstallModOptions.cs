namespace Epic.OnlineServices.Mods
{
	public class UninstallModOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public ModIdentifier Mod { get; set; }
	}
}
