using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class DRLGameFlowUI : FlowModuleUI
	{
		[SerializeField]
		private UIView m_ui;

		[SerializeField]
		private UIHUD m_hud;

		public UIView ui => Reflection<object>.Assert(ref m_ui, base.gameObject, p_add: false);

		public UIHUD hud
		{
			get
			{
				if ((bool)m_hud)
				{
					return m_hud;
				}
				return m_hud = (ui.game ? ui.game.hud : null);
			}
		}

		public override void SetTimer(int p_position, string p_label, float p_time)
		{
			switch (p_position)
			{
			case 0:
				hud.training.SetTimerVisible(p_position, p_time >= 0f);
				hud.training.time0 = ((p_time >= 0f) ? p_time : 0f);
				break;
			case 1:
				hud.training.SetTimerVisible(p_position, p_time >= 0f);
				hud.training.time1 = ((p_time >= 0f) ? p_time : 0f);
				break;
			}
		}

		public override float GetTimerValue(int p_position)
		{
			return hud.training.GetTimerValue(p_position);
		}

		public override void HilightTimer(int p_position)
		{
			if (p_position == 0)
			{
				hud.training.clockParticle0.Play(withChildren: true);
			}
		}

		public override void SoftResetTimers()
		{
			hud.training.ResetStepTimes();
		}

		public override void SaveStepTimes()
		{
			hud.training.SaveStepTimes();
		}

		public override void SetStepTimes(Vector2 p_times)
		{
			hud.training.SetStepTimes(p_times);
		}

		public override float GetStepTimes(int p_position)
		{
			return hud.training.GetStepTimes(p_position);
		}

		public override void ClearTimer(int p_position)
		{
			SetTimer(p_position, "", 0f);
			SetTimer(p_position, "", -1f);
		}

		public override void SetStep(int p_step, int p_total)
		{
			p_step = Mathf.Clamp(p_step, 0, p_total);
			hud.training.SetStep(p_step, p_total);
		}

		public override int GetSteps()
		{
			return hud.training.GetSteps();
		}

		public override void SetProgressBar(int p_step, int p_total)
		{
			p_step = Mathf.Clamp(p_step, 0, p_total);
			hud.training.SetProgressBar(p_step, p_total);
			if (p_total > 0 && p_step > 0)
			{
				hud.training.HilightStepProgress();
			}
		}

		public override void SetCount(int p_step)
		{
			p_step = Mathf.Max(p_step, 0);
			hud.training.SetStepsVisible(p_flag: true);
			hud.training.SetCount(p_step);
		}

		public override void PlayMissionCompleteAudio()
		{
			ui.app.view.audio.PlayBigStepComplete();
		}

		public override void PlayButtonNextAudio()
		{
			ui.app.view.audio.PlayUIClick();
		}

		public override void PlayFastForwardCamera()
		{
			ui.app.view.audio.PlayUICameraFForward();
		}

		public override void PlayBalloonRadarAudio(GameObject p_balloon)
		{
			ui.app.view.audio.PlayGameBalloonRadar(p_balloon);
		}

		public override void StopBalloonRadarAudio(GameObject p_balloon)
		{
			ui.app.view.audio.StopGameBalloonRadar(p_balloon);
		}

		public override void HilightStepProgress()
		{
			hud.training.HilightStepProgress();
		}

		public override void ShowNPCOverlay(NPCStateType p_state, string p_text)
		{
			hud.training.ShowNPCOverlay(p_state, p_text);
		}

		public override void HideNPCOverlay()
		{
			hud.training.ShowContent();
		}

		public override void ShowContent()
		{
			hud.Fade(1f);
		}

		public override void SetFooterLeftText(string p_text)
		{
			hud.training.SetFooterLeftText(p_text);
		}

		public override void ShowFooter()
		{
			hud.training.ShowFooter(p_show: true);
		}

		public override void HideFooter()
		{
			hud.training.ShowFooter(p_show: false);
		}

		public override void SetFooterRightText(string p_text)
		{
			hud.training.footerRightField.text = p_text;
		}

		public override void SetFooterNPCState(NPCStateType p_state)
		{
			hud.training.SetFooterNPCState(p_state);
		}

		public override void ShowHeader(bool p_show, float p_duration)
		{
			hud.training.ShowHeader(p_show, p_duration);
		}

		public override void FadeIn(float p_delay, float duration = 2f)
		{
			ui.fade.FadeIn(duration, p_delay);
		}

		public override void FadeOut(float p_delay, float duration = 2f)
		{
			ui.fade.FadeOut(duration, p_delay);
		}

		public override void ShowSkip(bool dmv = false)
		{
			if (dmv)
			{
				hud.training.ShowDMVIntroSkip();
			}
			else
			{
				hud.training.ShowSkipWarning();
			}
		}

		public override void UpdateSkip(float val)
		{
			hud.training.UpdateSkipValue(val);
		}

		public override void HideSkip(bool dmv = false)
		{
			if (dmv)
			{
				hud.training.HideDMVIntroSkip();
			}
			else
			{
				hud.training.HideSkipWarning();
			}
		}

		public override void SetControllerAnimation(UIControllerAnimationType p_type)
		{
			hud.controller.SetAnimation(p_type);
		}

		public override void SetGauge(int p_position, bool p_flag)
		{
			switch (p_position)
			{
			case 0:
				hud.training.SetLeftGauges(p_precision: false, p_flag);
				break;
			case 1:
				hud.training.SetRightGauges(p_precision: false, p_flag);
				break;
			}
		}

		public override void SetGauge(int p_position, float p_ratio)
		{
			switch (p_position)
			{
			case 0:
				hud.training.leftGauge.barRatio = p_ratio;
				break;
			case 1:
				hud.training.rightGauge.barRatio = p_ratio;
				break;
			}
		}

		public override void SetGauge(int p_position, string p_up, string p_middle, string p_down)
		{
			if (p_position >= 0)
			{
				UIHUDGauge uIHUDGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDGauge = hud.training.leftGauge;
					break;
				case 1:
					uIHUDGauge = hud.training.rightGauge;
					break;
				}
				if ((bool)uIHUDGauge)
				{
					uIHUDGauge.labelTop = p_up;
					uIHUDGauge.labelMiddle = p_middle;
					uIHUDGauge.labelBottom = p_down;
				}
			}
		}

		public override void SetGauge(bool p_locked, int p_position, float p_duration)
		{
			if (p_position >= 0)
			{
				UIHUDGauge uIHUDGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDGauge = hud.training.leftGauge;
					break;
				case 1:
					uIHUDGauge = hud.training.rightGauge;
					break;
				}
				if ((bool)uIHUDGauge)
				{
					uIHUDGauge.SetLock(p_locked, p_duration);
				}
			}
		}

		public override void SetPrecisionGauge(int p_position, bool p_flag)
		{
			switch (p_position)
			{
			case 0:
				hud.training.SetLeftGauges(p_flag, p_gauge: false);
				break;
			case 1:
				hud.training.SetRightGauges(p_flag, p_gauge: false);
				break;
			}
		}

		public override void SetPrecisionGauge(int p_position, string p_up, string p_middle, string p_down, float p_precision)
		{
			if (p_position >= 0)
			{
				UIHUDPrecisionGauge uIHUDPrecisionGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDPrecisionGauge = hud.training.leftPrecisionGauge;
					break;
				case 1:
					uIHUDPrecisionGauge = hud.training.rightPrecisionGauge;
					break;
				}
				if ((bool)uIHUDPrecisionGauge)
				{
					uIHUDPrecisionGauge.labelTop = p_up;
					uIHUDPrecisionGauge.labelMiddle = p_middle;
					uIHUDPrecisionGauge.labelBottom = p_down;
					uIHUDPrecisionGauge.barMiddleRatio = p_precision;
				}
			}
		}

		public override void SetPrecisionGauge(int p_position, float p_ratio)
		{
			switch (p_position)
			{
			case 0:
				hud.training.leftPrecisionGauge.barDragRatio = p_ratio;
				break;
			case 1:
				hud.training.rightPrecisionGauge.barDragRatio = p_ratio;
				break;
			}
		}

		public override void SetPrecisionGauge(bool p_locked, int p_position, float p_duration)
		{
			if (p_position >= 0)
			{
				UIHUDPrecisionGauge uIHUDPrecisionGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDPrecisionGauge = hud.training.leftPrecisionGauge;
					break;
				case 1:
					uIHUDPrecisionGauge = hud.training.rightPrecisionGauge;
					break;
				}
				if ((bool)uIHUDPrecisionGauge)
				{
					uIHUDPrecisionGauge.SetLock(p_locked, p_duration);
				}
			}
		}

		public override void HighlightGauge(int p_position)
		{
			if (p_position >= 0)
			{
				UIHUDGauge uIHUDGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDGauge = hud.training.leftGauge;
					break;
				case 1:
					uIHUDGauge = hud.training.rightGauge;
					break;
				}
				if ((bool)uIHUDGauge)
				{
					uIHUDGauge.Hilight();
				}
			}
		}

		public override void HighlightPrecisionGauge(int p_position)
		{
			if (p_position >= 0)
			{
				UIHUDPrecisionGauge uIHUDPrecisionGauge = null;
				switch (p_position)
				{
				case 0:
					uIHUDPrecisionGauge = hud.training.leftPrecisionGauge;
					break;
				case 1:
					uIHUDPrecisionGauge = hud.training.rightPrecisionGauge;
					break;
				}
				if ((bool)uIHUDPrecisionGauge)
				{
					uIHUDPrecisionGauge.Hilight();
				}
			}
		}

		public override void UpdateMarker(FNCollider.Trigger p_trigger, int p_template_id = 0)
		{
			if (p_template_id == -1)
			{
				return;
			}
			UIHUDMarkerLayer marker = ui.game.hud.marker;
			ColliderEventComponent target = p_trigger.target;
			if (p_trigger.completed)
			{
				if (marker.Contains(target))
				{
					marker.Remove(target);
				}
			}
			else if (!marker.Contains(target))
			{
				marker.Add(target, p_template_id);
			}
		}

		public override void ClearMarkers()
		{
			ui.game.hud.marker.Clear();
		}

		public override void ShowButtonNext()
		{
			hud.training.ShowButtonNext();
		}

		public override void UpdateButtonNext(float progress)
		{
			hud.training.UpdateButtonNext(progress);
		}

		public override void HideButtonNext()
		{
			hud.training.HideButtonNext();
		}

		public override void ShowIndicator(Indicator p_indicator)
		{
			hud.training.ShowIndicator(p_indicator);
		}

		public override void HideIndicator(Indicator p_indicator)
		{
			hud.training.HideIndicator(p_indicator);
		}

		public override void ClearIndicators()
		{
			hud.training.ClearIndicators();
		}

		public override void Notify(string p_notification, float p_delay, params object[] p_args)
		{
			ui.app.Notify(p_delay, p_notification, this, p_args);
		}

		public override void ClearMissionUI()
		{
			ClearIndicators();
			ClearMarkers();
			ClearControllerAnimation();
			SetControllerAnimation(UIControllerAnimationType.UserInput);
			SetPrecisionGauge(0, p_flag: false);
			SetPrecisionGauge(1, p_flag: false);
			SetGauge(0, p_flag: false);
			SetGauge(1, p_flag: false);
		}

		public override void Show(ElementType p_type, float p_delay = 0f)
		{
			if (p_type == ElementType.FooterController)
			{
				hud.training.ShowController();
			}
		}

		public override void Hide(ElementType p_type, float p_delay = 0f)
		{
			switch (p_type)
			{
			case ElementType.HeaderStep:
				hud.training.SetStepsVisible(p_flag: false);
				break;
			case ElementType.FooterController:
				hud.training.HideController();
				break;
			case ElementType.FooterNPC:
				hud.training.SetFooterNPCState(NPCStateType.Hide);
				break;
			case ElementType.Footer:
				hud.training.ShowFooter(p_show: false);
				break;
			case ElementType.Header:
				break;
			}
		}

		public override void ShowObjectives()
		{
			hud.training.ShowObjectives();
		}

		public override void SetObjectives(string[] p_labels)
		{
			hud.training.SetObjectives(p_labels);
		}

		public override void NextObjective()
		{
			hud.training.NextObjective();
		}

		public override void ClearObjectives()
		{
			hud.training.ClearObjectives();
		}

		public override void FadeInCounter(int p_c)
		{
			hud.training.FadeInCounterLamp(p_c);
		}
	}
}
