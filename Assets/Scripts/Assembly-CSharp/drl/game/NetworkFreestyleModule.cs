using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class NetworkFreestyleModule : NetworkGameModule
	{
		public new NetworkFreestyleController controller => (NetworkFreestyleController)base.controller;

		public override DroneRigData defaultRig => controller.model.rig;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if ((bool)controller && controller.isActiveAndEnabled)
			{
				base.OnNotification(p_event, p_target, p_data);
				if (p_event != null)
				{
					_ = p_event == "viewer.controls.nav.exit@click";
				}
			}
		}
	}
}
