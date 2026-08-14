using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class NetworkRaceModule : NetworkGameModule
	{
		public new NetworkRaceController controller => (NetworkRaceController)base.controller;

		public override DroneRigData defaultRig => controller.model.rig;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if ((bool)controller && controller.isActiveAndEnabled)
			{
				base.OnNotification(p_event, p_target, p_data);
			}
		}
	}
}
