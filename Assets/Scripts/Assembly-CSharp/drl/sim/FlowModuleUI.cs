using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	public class FlowModuleUI : MonoBehaviour
	{
		public enum ElementType
		{
			DialogImage = 0,
			DialogText = 1,
			Dialog = 2,
			HeaderTimer = 3,
			HeaderStep = 4,
			Header = 5,
			FooterController = 6,
			FooterNPC = 7,
			Footer = 8
		}

		public void SetTimer(float p_time)
		{
			SetTimer(0, "TIME", p_time);
		}

		public virtual void SetTimer(int p_position, string p_label, float p_time)
		{
		}

		public virtual float GetTimerValue(int p_position)
		{
			return 0f;
		}

		public virtual void ClearTimer(int p_position)
		{
		}

		public virtual void HilightTimer(int p_position)
		{
		}

		public virtual void SetStep(int p_step, int p_total)
		{
		}

		public virtual int GetSteps()
		{
			return 0;
		}

		public virtual void SetProgressBar(int p_step, int p_total)
		{
		}

		public virtual void SetCount(int p_step)
		{
		}

		public virtual void HilightStepProgress()
		{
		}

		public virtual void PlayMissionCompleteAudio()
		{
		}

		public virtual void PlayButtonNextAudio()
		{
		}

		public virtual void PlayFastForwardCamera()
		{
		}

		public virtual void PlayBalloonRadarAudio(GameObject p_target)
		{
		}

		public virtual void StopBalloonRadarAudio(GameObject p_target)
		{
		}

		public virtual void SoftResetTimers()
		{
		}

		public virtual void SaveStepTimes()
		{
		}

		public virtual void SetStepTimes(Vector2 p_times)
		{
		}

		public virtual float GetStepTimes(int p_position)
		{
			return 0f;
		}

		public virtual void ShowObjectives()
		{
		}

		public virtual void SetObjectives(string[] p_labels)
		{
		}

		public virtual void NextObjective()
		{
		}

		public virtual void ClearObjectives()
		{
		}

		public virtual void FadeInCounter(int p_c)
		{
		}

		public virtual void FadeIn(float p_delay, float duration = 2f)
		{
		}

		public virtual void FadeOut(float p_delay, float duration = 2f)
		{
		}

		public virtual void Show(ElementType p_type, float p_delay = 0f)
		{
		}

		public virtual void Hide(ElementType p_type, float p_delay = 0f)
		{
		}

		public virtual void ShowSkip(bool dmv = false)
		{
		}

		public virtual void UpdateSkip(float val)
		{
		}

		public virtual void HideSkip(bool dmv = false)
		{
		}

		public virtual void SetFooterLeftText(string p_text)
		{
		}

		public virtual void SetFooterRightText(string p_text)
		{
		}

		public virtual void SetFooterNPCState(NPCStateType p_state)
		{
		}

		public virtual void ShowFooter()
		{
		}

		public virtual void HideFooter()
		{
		}

		public virtual void ShowHeader(bool p_show, float p_duration = 0.7f)
		{
		}

		public virtual void SetControllerAnimation(UIControllerAnimationType p_type)
		{
		}

		public virtual void ClearControllerAnimation()
		{
			SetControllerAnimation(UIControllerAnimationType.StopAll);
		}

		public virtual void SetControllerOverlay(ControllerStateType p_type)
		{
		}

		public virtual void ShowController(float p_delay)
		{
		}

		public virtual void HideController(float p_delay)
		{
		}

		public virtual void ShowNPCOverlay(NPCStateType p_state, string p_text)
		{
		}

		public virtual void HideNPCOverlay()
		{
		}

		public virtual void SetGauge(int p_position, bool p_flag)
		{
		}

		public virtual void SetGauge(int p_position, string p_up, string p_middle, string p_down)
		{
		}

		public virtual void SetGauge(int p_position, float p_ratio)
		{
		}

		public virtual void SetGauge(bool p_locked, int p_position, float p_duration)
		{
		}

		public virtual void SetPrecisionGauge(int p_position, string p_up, string p_middle, string p_down, float p_precision)
		{
		}

		public virtual void SetPrecisionGauge(int p_position, bool p_flag)
		{
		}

		public virtual void SetPrecisionGauge(int p_position, float p_ratio)
		{
		}

		public virtual void SetPrecisionGauge(bool p_locked, int p_position, float p_duration)
		{
		}

		public virtual void HighlightGauge(int p_position)
		{
		}

		public virtual void HighlightPrecisionGauge(int p_position)
		{
		}

		public virtual void SetDialog(Texture p_image, string p_text)
		{
		}

		public virtual void SetDialog(string p_text)
		{
		}

		public virtual void SetDialogNPC(string p_text)
		{
		}

		public virtual void SetDialog(Texture p_image)
		{
		}

		public virtual void ClearDialog(bool p_image, bool p_text)
		{
		}

		public virtual void UpdateMarker(FNCollider.Trigger p_trigger, int p_template_id = 0)
		{
		}

		public virtual void ClearMarkers()
		{
		}

		public virtual void ShowIndicator(Indicator p_indicator)
		{
		}

		public virtual void HideIndicator(Indicator p_indicator)
		{
		}

		public virtual void ClearIndicators()
		{
		}

		public virtual void ShowButtonNext()
		{
		}

		public virtual void UpdateButtonNext(float progress)
		{
		}

		public virtual void HideButtonNext()
		{
		}

		public virtual void ClearMissionUI()
		{
		}

		public virtual void ShowContent()
		{
		}

		public virtual void Notify(string p_notification, float p_delay, params object[] p_args)
		{
		}
	}
}
