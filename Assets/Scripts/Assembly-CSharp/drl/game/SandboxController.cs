using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class SandboxController : FreestyleController
	{
		private bool garageOpen;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			switch (p_event)
			{
			case "game.simulation.drone.all@ready":
			{
				base.app.view.ui.game.hud.dashboard.isSandbox = true;
				base.app.view.ui.game.hud.dashboard.Init();
				base.app.view.ui.game.hud.dashboard.Hide();
				base.app.view.ui.game.hud.physics.view.ShowFooter(p_show: true);
				FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
				if (active != null && base.game.model.playerDrone != null)
				{
					base.game.model.playerDrone.fc.profile.SetData(active);
				}
				break;
			}
			case "game.ui.dashboard@hide":
				base.app.view.ui.game.hud.physics.view.ShowFooter(p_show: true);
				break;
			case "garage.open":
			{
				if (base.app.model.game.paused || base.app.view.ui.game.hud.dashboard.isShowing || garageOpen || !base.validContext)
				{
					break;
				}
				StorageModel storage = base.app.model.storage;
				GameModel gm = (base.app.controller.game ? base.app.controller.game.model : null);
				Drone d = (gm ? gm.playerDrone : null);
				if (!storage || !gm || !d || d.rig == null || d.rig.isLocked)
				{
					break;
				}
				Activity.RunOnce(delegate
				{
					base.app.model.game.playerDrone.fc.enabled = false;
				}, 0.8f);
				base.app.view.audio.PauseAllGameAudio();
				base.enabled = false;
				garageOpen = true;
				d.fc.armed = false;
				storage.PreloadDroneBundleData(null, null, p_ingame: true, delegate
				{
					base.enabled = true;
					UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
					if ((bool)gm)
					{
						uIGarageRigEditView.data = d.rig;
						uIGarageRigEditView.externalDrone = d;
					}
				});
				break;
			}
			case "garage.edit.done":
			case "garage.edit.fly.ready":
				garageOpen = false;
				base.app.model.game.playerDrone.fc.enabled = true;
				base.app.view.audio.ResumeAllGameAudio();
				break;
			}
		}
	}
}
