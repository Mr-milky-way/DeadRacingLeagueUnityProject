using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class FreeCameraController : GameTypeController
	{
		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			switch (p_event)
			{
			case "game.simulation.load@complete":
			{
				DroneSimulation simulation = base.game.model.simulation;
				if (!simulation)
				{
					Debug.LogWarning("FreeCameraController> Failed to locate a simulation!");
					base.app.scene.LoadMain();
					break;
				}
				DroneCamera droneCamera = simulation.cameras.Get(0);
				droneCamera.SetFreeCamera();
				SetFreeCameraStart(droneCamera);
				base.game.input.SetController(this);
				base.game.input.listening = true;
				break;
			}
			case "game.pause":
				base.game.model.camera.wasd.enabled = false;
				break;
			case "game.unpause":
				base.game.model.camera.wasd.enabled = true;
				break;
			case "game.boot":
				break;
			}
		}

		public void SetFreeCameraStart(DroneCamera p_camera)
		{
			if ((bool)p_camera)
			{
				Debug.Log("FreeCameraController> SetCamera - camera[" + p_camera?.ToString() + "]");
				base.app.model.game.level.track.SetStartsFrontTransform(p_camera.transform, new Vector3(0f, 2f, 2f));
				p_camera.orbit.Snap();
			}
		}
	}
}
