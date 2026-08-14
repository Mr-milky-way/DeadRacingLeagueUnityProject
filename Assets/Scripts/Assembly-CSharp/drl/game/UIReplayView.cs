using UnityEngine;

namespace drl.game
{
	public class UIReplayView : UIGameViewerView
	{
		public ReplayClipPlayerModel focus;

		public ReplayPlayerModel player => base.app.model.game.replay.player;

		public override void SetMode(ViewerModeType p_mode)
		{
			base.enabled = p_mode == mode;
			UISpectatorView component = GetComponent<UISpectatorView>();
			if ((bool)component)
			{
				component.enabled = !base.enabled;
			}
			if (p_mode == ViewerModeType.Replay)
			{
				SetDirectorModeAllowed(p_flag: false);
				controls.playback.SetReplayMode();
			}
		}

		public void SetTimeSlider(float p_time)
		{
			UIGameViewerControlsPlaybackPanel playback = controls.playback;
			notificationLock = true;
			playback.time = p_time;
			notificationLock = false;
		}

		public void UpdateFocusedClip()
		{
			_ = controls.playback;
			SetTimeSlider(player.elapsed);
			ReplayClipPlayerModel replayClipPlayerModel = focus;
			if ((bool)replayClipPlayerModel)
			{
				info.time = Mathf.Min(player.elapsed, replayClipPlayerModel.raceTime);
				info.controller.leftStick = replayClipPlayerModel.leftInput;
				info.controller.rightStick = replayClipPlayerModel.rightInput;
				info.rpm = replayClipPlayerModel.rpm;
			}
		}

		public void PlaybackPause()
		{
			if (!(controls.fade.alpha < 1f))
			{
				base.app.view.ui.navigation.enabled = false;
				DisableControls();
			}
		}

		public void PlaybackUnpause()
		{
			if (!(controls.fade.alpha >= 1f))
			{
				base.app.view.ui.navigation.enabled = true;
				EnableControls(p_focus: false);
			}
		}

		public void StopPlayback()
		{
			player.paused = false;
			player.playing = false;
			controls.playback.SetPause(p_flag: false);
			player.Seek(0f);
			UpdateFocusedClip();
		}
	}
}
