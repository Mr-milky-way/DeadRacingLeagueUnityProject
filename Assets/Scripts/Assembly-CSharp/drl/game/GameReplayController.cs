using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameReplayController : Controller<DRLApp>
	{
		public ReplayRecorderController recorder => AssertFind<ReplayRecorderController>("recorder");

		public ReplayPlayerController player => AssertFind<ReplayPlayerController>("player");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
		}

		public void SetCameraMode(ViewerCameraModeType p_mode, Drone p_drone)
		{
			DroneCamera camera = base.app.model.game.camera;
			if (!camera)
			{
				return;
			}
			Debug.Log("GameReplayController> SetCameraMode - mode[" + p_mode.ToString() + "] drone[" + p_drone?.ToString() + "]");
			camera.wasd.joystickSensitivityMultiplier = 1f;
			switch (p_mode)
			{
			case ViewerCameraModeType.FPV:
				if ((bool)p_drone)
				{
					camera.SetFPV(p_drone);
					camera.wasd.useJoystick = false;
					camera.wasd.snapOnRelease = false;
					camera.wasd.scrollStep = 0.5f;
				}
				break;
			case ViewerCameraModeType.Orbit:
				if ((bool)p_drone)
				{
					camera.SetTPVFree(p_drone, 2f, 0.4f, 35f);
					camera.orbit.angle = new Vector2(0f, 0f);
					camera.orbit.speed.angle = 0.5f;
					camera.follow.offset = new Vector3(0f, 0.025f, 0f);
					camera.fov = 45f;
					camera.wasd.useJoystick = false;
					camera.wasd.snapOnRelease = true;
					camera.wasd.scrollStep = 4f;
					camera.wasd.orbitDragKey = KeyCode.Mouse0;
					camera.wasd.sensitivity = 0.35f;
					camera.wasd.joystickSensitivityMultiplier = 5f;
					Vector3 forward = p_drone.transform.forward;
					forward.y = 0f;
					forward.Normalize();
					camera.orbit.anchorRotation = Quaternion.LookRotation(forward, Vector3.up);
					camera.orbit.StopTransition(OrbitTransform.TransitionMask.AnchorRotationMask);
					camera.orbit.Snap(p_position: true, p_angle: false);
				}
				break;
			case ViewerCameraModeType.FreeCamera:
				camera.SetFreeCamera(p_reset_y: true);
				camera.wasd.useJoystick = false;
				camera.wasd.snapOnRelease = true;
				camera.wasd.orbitDragKey = KeyCode.Mouse1;
				break;
			}
		}
	}
}
