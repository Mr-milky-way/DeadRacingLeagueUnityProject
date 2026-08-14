namespace Epic.OnlineServices.Mods
{
	public class InstallModOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public ModIdentifier Mod { get; set; }

		public bool RemoveAfterExit { get; set; }
	}
}
