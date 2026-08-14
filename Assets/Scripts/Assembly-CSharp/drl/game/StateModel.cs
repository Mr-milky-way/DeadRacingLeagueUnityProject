using thelab.mvc;

namespace drl.game
{
	public class StateModel : Model<DRLApp>
	{
		public bool ready;

		public ServerStateModel server => AssertFind<ServerStateModel>("server");

		public PlayerStateModel player => AssertFind<PlayerStateModel>("player");

		public LicenseStateModel license => AssertFind<LicenseStateModel>("license");
	}
}
