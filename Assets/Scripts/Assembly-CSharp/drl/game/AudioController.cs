using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class AudioController : Controller<DRLApp>
	{
		private float m_prev_volume;

		public AudioView audio => base.app.view.audio;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			string text = "";
			if (p_event.Contains("@no-sound") || (base.app != null && base.app.model.storage != null && (!base.validContext || !base.app.model.storage.state.player.settings.audio.audioUIEnabled)))
			{
				return;
			}
			if (p_event.Contains("@soon") && p_event.Contains("@click"))
			{
				base.app.view.audio.PlayUIGenericError();
				return;
			}
			if (p_event.IndexOf("@over") >= 0)
			{
				text = "over";
				if (!Cursor.visible)
				{
					text = "";
				}
			}
			if (p_event.IndexOf("@focus") >= 0)
			{
				text = "focus";
				if (Cursor.visible)
				{
					text = "";
				}
			}
			if (p_event.IndexOf("@change") >= 0)
			{
				if (p_target is StepperView)
				{
					text = "click";
				}
				if (p_target is InputFieldView)
				{
					text = "change";
				}
				if (p_target is SliderView)
				{
					text = "change";
				}
				if (p_target is UIScreenView)
				{
					text = "";
				}
			}
			if (p_event.IndexOf("@click") >= 0)
			{
				text = "click";
				if (p_event == "ui.screen.return@click")
				{
					text = "";
				}
			}
			bool flag = false;
			if (p_target is UICardButtonLarge)
			{
				flag = true;
			}
			if (p_target is UICardButtonQuest)
			{
				flag = true;
			}
			switch (text)
			{
			case "focus":
				if (flag)
				{
					base.app.view.audio.PlayUIOver();
				}
				else
				{
					base.app.view.audio.PlayUISmallFocus();
				}
				break;
			case "over":
				if (flag)
				{
					base.app.view.audio.PlayUIOver();
				}
				else
				{
					base.app.view.audio.PlayUISmallFocus();
				}
				break;
			case "click":
				if (flag)
				{
					base.app.view.audio.PlayUIClick();
				}
				else
				{
					base.app.view.audio.PlayUISmallClick();
				}
				break;
			case "text-over":
				base.app.view.audio.PlayUITextOver();
				break;
			case "main-click":
				base.app.view.audio.PlayUIClick();
				break;
			case "change":
				base.app.view.audio.PlayUIChange();
				break;
			}
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen.history.add":
				base.app.view.audio.PlayUIScreenForward();
				break;
			case "ui.screen.history.remove":
				base.app.view.audio.PlayUIScreenBackward();
				break;
			case "map-editor.entity.delete":
				base.app.view.audio.PlayUIMapEditorDelete();
				break;
			case "map-editor.entity.clone":
			case "map-editor.entity.create":
				base.app.view.audio.PlayUIMapEditorPlace();
				break;
			case "map-editor.selection.entities@add":
				base.app.view.audio.PlayUIMapEditorSelect();
				break;
			case "garage.edit.rig-trailcolor@click":
				base.app.view.audio.PlayUIGarageTrailChange();
				break;
			case "garage.edit.rig-texturecolor@click":
			case "garage.edit.rig-edgecolor@click":
			case "garage.edit.rig-propcolor@click":
				base.app.view.audio.PlayUIGarageSpray();
				break;
			case "garage.edit.prop-spin-impulse":
				if (p_data != null && p_data.Length != 0)
				{
					GameObject p_target4 = (GameObject)p_data[0];
					base.app.view.audio.PlayDroneMotorStart(p_target4);
				}
				break;
			case "garage.edit.prop-spin-start":
				if (p_data != null && p_data.Length != 0)
				{
					GameObject p_target3 = (GameObject)p_data[0];
					base.app.view.audio.PlayDroneMotorIdle(p_target3);
				}
				break;
			case "garage.edit.prop-spin-stop":
				if (p_data != null && p_data.Length != 0)
				{
					GameObject p_target2 = (GameObject)p_data[0];
					base.app.view.audio.StopDroneMotor(p_target2);
				}
				break;
			case "garage.edit.change-part":
				base.app.view.audio.PlayUIGaragPartChange();
				break;
			case "garage.edit.change-style":
				base.app.view.audio.PlayUIGarageSpray();
				break;
			case "garage.edit.change-frame":
				base.app.view.audio.PlayUIGaragPartChange();
				if (p_data != null && p_data.Length != 0)
				{
					GameObject p_target5 = (GameObject)p_data[0];
					base.app.view.audio.UpdateDroneMotorIdle(p_target5);
				}
				break;
			case "garage.isOpen":
				if (base.app.scene.manager.levelName == "main")
				{
					base.app.view.audio.StopMusicMain();
				}
				else
				{
					base.app.view.audio.PauseGameMusic();
				}
				base.app.view.audio.StopMusicGarage();
				base.app.view.audio.PlayMusicGarage();
				break;
			case "garage.isClosed":
			{
				string text2 = "";
				for (int i = 0; i < p_data.Length; i++)
				{
					text2 = p_data[i].ToString();
				}
				if (text2 == "pause")
				{
					base.app.view.audio.UpdateGameStatus("paused");
				}
				else
				{
					base.app.view.audio.UpdateGameStatus("playing");
				}
				base.app.view.audio.StopAllDroneSounds();
				base.app.view.audio.StopMusicGarage();
				if (base.app.scene.manager.levelName == "main")
				{
					base.app.view.audio.PlayMusicMain();
				}
				else
				{
					Activity.RunOnce(base.app.view.audio.ResumeGameMusic, 0.5f);
				}
				break;
			}
			case "ui.slider.handle@unfocus":
				base.app.view.audio.PlayUISmallClick();
				break;
			}
		}

		protected void OnApplicationFocus(bool hasFocus)
		{
			if (!Application.isEditor && base.validContext)
			{
				if (hasFocus)
				{
					base.app.view.audio.volume = base.app.model.storage.state.player.settings.audio.volumeMain;
				}
				else
				{
					base.app.view.audio.volume = 0f;
				}
			}
		}
	}
}
