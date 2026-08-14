using UnityEngine;
using drl.sim;
using drl.sim.rci;

namespace drl.game
{
	public class UIReplayController : UIGameViewerController
	{
		private float m_last_elapsed;

		public ReplayPlayerModel player => base.app.model.game.replay.player;

		public new UIReplayView view => AssertLocal<UIReplayView>("view");

		public float raceTime
		{
			get
			{
				if (!view.focus)
				{
					return 0f;
				}
				return view.focus.raceTime;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (view.current && view.enabled)
			{
				switch (p_event)
				{
				case "ui.screen@open":
					base.game.FadeBlur(0f, 0f);
					base.app.view.ui.SetDark(p_flag: false);
					base.app.view.ui.footer.Hide(0f);
					break;
				case "viewer.controls.nav.settings@click":
					player.paused = true;
					view.controls.playback.SetPause(p_flag: false);
					break;
				}
				base.OnNotification(p_event, p_target, p_data);
			}
		}

		protected override void OnControlsFormEvent(string p_event, Object p_target, bool p_is_change)
		{
			base.OnControlsFormEvent(p_event, p_target, p_is_change);
			bool flag = p_is_change;
			string text = (p_target ? p_target.name : "");
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			UIGameViewerControlsPlaybackPanel playback = view.controls.playback;
			_ = view.info;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "playback-skip-start":
				player.Seek(0f);
				view.UpdateFocusedClip();
				break;
			case "playback-skip-end":
				player.Seek(player.duration);
				view.UpdateFocusedClip();
				break;
			case "playback-play":
				SwitchPlayback();
				if (!player.paused && playback.autoHideEnabled)
				{
					view.DisableControls();
				}
				break;
			case "playback-stop":
				player.paused = false;
				player.playing = false;
				playback.SetPause(p_flag: false);
				player.Seek(0f);
				view.UpdateFocusedClip();
				break;
			case "playback-speed":
				if (flag)
				{
					float speed = Mathf.Floor((p_target as DRLSliderView).slider.value * 100f) / 100f;
					SetSpeed(speed);
				}
				break;
			case "playback-time":
				if (flag)
				{
					float value = (p_target as DRLSliderView).slider.value;
					SetTime(value, raceTime);
				}
				break;
			case "playback-autohide":
				break;
			}
		}

		protected void SetSpeed(float p_speed)
		{
			player.speed = p_speed;
			for (int i = 0; i < player.clips.Count; i++)
			{
				ReplayClipPlayerModel clip = player.GetClip(i);
				if ((bool)clip.drone && clip.drone.ready)
				{
					clip.drone.renderer.trailScale = player.speed;
				}
			}
		}

		protected void SetDroneTrailDuration(float p_duration)
		{
			for (int i = 0; i < player.clips.Count; i++)
			{
				ReplayClipPlayerModel clip = player.GetClip(i);
				if ((bool)clip.drone && clip.drone.ready)
				{
					clip.drone.renderer.SetTrailsDuration(p_duration);
				}
			}
		}

		protected void SetTime(float p_time, float p_max_time = -1f)
		{
			player.Seek(p_time);
			if (p_max_time >= 0f)
			{
				p_time = Mathf.Min(p_time, p_max_time);
			}
			view.info.time = p_time;
			view.UpdateFocusedClip();
		}

		protected void SwitchPlayback()
		{
			UIGameViewerControlsPlaybackPanel playback = view.controls.playback;
			if ((!(player.speed > 0f) || !(player.elapsed >= player.duration)) && (!(player.speed < 0f) || !(player.elapsed <= 0f)))
			{
				player.paused = player.playing && !player.paused;
				playback.SetPause(!player.paused);
				player.playing = true;
			}
		}

		protected override void OnCameraModeChange(ViewerCameraModeType p_mode)
		{
			if ((bool)view.focus)
			{
				Drone drone = view.focus.drone;
				base.game.replay.SetCameraMode(p_mode, drone);
				switch (p_mode)
				{
				case ViewerCameraModeType.FPV:
					SetDroneTrailDuration(0.8f);
					drone.body.frame.camera.fov = drone.fc.profile.fov;
					break;
				case ViewerCameraModeType.FreeCamera:
					SetDroneTrailDuration(8f);
					break;
				case ViewerCameraModeType.Orbit:
					SetDroneTrailDuration(8f);
					break;
				}
			}
		}

		protected override void OnTargetChange(int p_index)
		{
			if (p_index < 0 || p_index >= player.clips.Count)
			{
				return;
			}
			if (view.controls.playback.targetIndex != p_index)
			{
				view.controls.playback.targetIndex = p_index;
			}
			ReplayClipPlayerModel clip = player.GetClip(p_index);
			if (!(clip == view.focus))
			{
				if ((bool)clip)
				{
					view.info.SetUser(clip.player);
					view.info.controller.SetController(clip.controller);
				}
				view.focus = clip;
				OnCameraModeChange(view.controls.playback.oldCameraMode);
				view.UpdateFocusedClip();
			}
		}

		protected void Update()
		{
			if (!base.enabled || !view.enabled)
			{
				return;
			}
			if (view.focus != null && view.focus.drone != null && view.focus.drone.hasBody && view.focus.drone.body.hasFrame && view.focus.drone.body.frame.hasCamera && view.focus.drone.body.frame.camera.fov < 1f)
			{
				view.focus.drone.body.frame.camera.fov = (view.focus.drone.hasFc ? view.focus.drone.fc.profile.fov : 90f);
			}
			if (DRLUINavigationSystem.IsTyping)
			{
				return;
			}
			bool flag = RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1);
			if (Input.GetKeyDown(KeyCode.Return))
			{
				flag = true;
			}
			if (flag)
			{
				view.PlaybackUnpause();
			}
			if (Input.GetKeyUp(KeyCode.T) && player.clips.Count > 0)
			{
				bool trailsEnabled = player.clips[0].drone.renderer.GetTrailsEnabled();
				for (int i = 0; i < player.clips.Count; i++)
				{
					player.clips[i].drone.renderer.SetTrailsEnabled(!trailsEnabled);
				}
			}
			if (player.playing)
			{
				_ = player.elapsed;
				view.UpdateFocusedClip();
				if (m_last_elapsed < player.duration && player.elapsed >= player.duration)
				{
					player.playing = false;
					player.paused = true;
					view.controls.playback.SetPause(p_flag: false);
				}
				m_last_elapsed = player.elapsed;
			}
			UpdateKeyboardTargetSelection();
			UpdateKeyboardCameraControls();
			UpdateKeyboardControls();
			if (Input.GetKeyUp(KeyCode.Space))
			{
				SwitchPlayback();
			}
		}
	}
}
