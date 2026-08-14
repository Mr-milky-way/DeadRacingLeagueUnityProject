using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;

public class AkTimelineEventPlayableBehavior : PlayableBehaviour
{
	[Flags]
	private enum Actions
	{
		None = 0,
		Playback = 1,
		Retrigger = 2,
		DelayedStop = 4,
		Seek = 8,
		FadeIn = 0x10,
		FadeOut = 0x20
	}

	private float currentDuration = -1f;

	private float currentDurationProportion = 1f;

	private bool eventIsPlaying;

	private bool fadeinTriggered;

	private bool fadeoutTriggered;

	private float previousEventStartTime;

	private const uint CallbackFlags = 9u;

	private Actions requiredActions;

	private const int scrubPlaybackLengthMs = 100;

	public AK.Wwise.Event akEvent;

	public float eventDurationMax;

	public float eventDurationMin;

	public float blendInDuration;

	public float blendOutDuration;

	public float easeInDuration;

	public float easeOutDuration;

	public AkCurveInterpolation blendInCurve;

	public AkCurveInterpolation blendOutCurve;

	public GameObject eventObject;

	public bool retriggerEvent;

	private bool wasScrubbingAndRequiresRetrigger;

	public bool StopEventAtClipEnd;

	private const float alph = 0.05f;

	private void CallbackHandler(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		switch (in_type)
		{
		case AkCallbackType.AK_EndOfEvent:
			eventIsPlaying = (fadeinTriggered = (fadeoutTriggered = false));
			break;
		case AkCallbackType.AK_Duration:
		{
			float fEstimatedDuration = (in_info as AkDurationCallbackInfo).fEstimatedDuration;
			currentDuration = fEstimatedDuration * currentDurationProportion / 1000f;
			break;
		}
		}
	}

	private bool IsScrubbing(FrameData info)
	{
		return info.evaluationType == FrameData.EvaluationType.Evaluate;
	}

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		base.PrepareFrame(playable, info);
		if (akEvent == null)
		{
			return;
		}
		bool flag = ShouldPlay(playable);
		if (IsScrubbing(info) && flag)
		{
			requiredActions |= Actions.Seek;
			if (!eventIsPlaying)
			{
				requiredActions |= Actions.Playback | Actions.DelayedStop;
				CheckForFadeInFadeOut(playable);
			}
		}
		else if (!eventIsPlaying && (requiredActions & Actions.Playback) == 0)
		{
			requiredActions |= Actions.Retrigger;
			CheckForFadeInFadeOut(playable);
		}
		else
		{
			CheckForFadeOut(playable, playable.GetTime());
		}
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		base.OnBehaviourPlay(playable, info);
		if (akEvent != null && ShouldPlay(playable))
		{
			requiredActions |= Actions.Playback;
			if (IsScrubbing(info))
			{
				wasScrubbingAndRequiresRetrigger = true;
				requiredActions |= Actions.DelayedStop;
			}
			else if (GetProportionalTime(playable) > 0.05f)
			{
				requiredActions |= Actions.Seek;
			}
			CheckForFadeInFadeOut(playable);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		wasScrubbingAndRequiresRetrigger = false;
		base.OnBehaviourPause(playable, info);
		if (eventObject != null && akEvent != null && StopEventAtClipEnd)
		{
			StopEvent();
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		base.ProcessFrame(playable, info, playerData);
		if (akEvent == null)
		{
			return;
		}
		GameObject gameObject = playerData as GameObject;
		if (gameObject != null)
		{
			eventObject = gameObject;
		}
		if (!(eventObject == null))
		{
			if ((requiredActions & Actions.Playback) != Actions.None)
			{
				PlayEvent();
			}
			if ((requiredActions & Actions.Seek) != Actions.None)
			{
				SeekToTime(playable);
			}
			if ((retriggerEvent || wasScrubbingAndRequiresRetrigger) && (requiredActions & Actions.Retrigger) != Actions.None)
			{
				RetriggerEvent(playable);
			}
			if ((requiredActions & Actions.DelayedStop) != Actions.None)
			{
				StopEvent(100);
			}
			if (!fadeinTriggered && (requiredActions & Actions.FadeIn) != Actions.None)
			{
				TriggerFadeIn(playable);
			}
			if (!fadeoutTriggered && (requiredActions & Actions.FadeOut) != Actions.None)
			{
				TriggerFadeOut(playable);
			}
			requiredActions = Actions.None;
		}
	}

	private bool ShouldPlay(Playable playable)
	{
		double previousTime = playable.GetPreviousTime();
		double time = playable.GetTime();
		if (previousTime == 0.0 && Math.Abs(time - previousTime) > 1.0)
		{
			return false;
		}
		if (retriggerEvent)
		{
			return true;
		}
		if (eventDurationMax == eventDurationMin && eventDurationMin != -1f)
		{
			return time < (double)eventDurationMax;
		}
		time -= (double)previousEventStartTime;
		float num = ((currentDuration == -1f) ? ((float)playable.GetDuration()) : currentDuration);
		return time < (double)num;
	}

	private void CheckForFadeInFadeOut(Playable playable)
	{
		double time = playable.GetTime();
		if ((double)blendInDuration > time || (double)easeInDuration > time)
		{
			requiredActions |= Actions.FadeIn;
		}
		CheckForFadeOut(playable, time);
	}

	private void CheckForFadeOut(Playable playable, double currentClipTime)
	{
		double num = playable.GetDuration() - currentClipTime;
		if ((double)blendOutDuration >= num || (double)easeOutDuration >= num)
		{
			requiredActions |= Actions.FadeOut;
		}
	}

	private void TriggerFadeIn(Playable playable)
	{
		double time = playable.GetTime();
		double num = (double)Mathf.Max(easeInDuration, blendInDuration) - time;
		if (num > 0.0)
		{
			fadeinTriggered = true;
			akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Pause, 0, blendOutCurve);
			akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Resume, (int)(num * 1000.0), blendInCurve);
		}
	}

	private void TriggerFadeOut(Playable playable)
	{
		fadeoutTriggered = true;
		double num = playable.GetDuration() - playable.GetTime();
		akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Stop, (int)(num * 1000.0), blendOutCurve);
	}

	private void StopEvent(int transition = 0)
	{
		if (eventIsPlaying)
		{
			akEvent.Stop(eventObject, transition);
		}
	}

	private bool PostEvent()
	{
		fadeinTriggered = (fadeoutTriggered = false);
		uint num = akEvent.Post(eventObject, 9u, CallbackHandler);
		eventIsPlaying = num != 0;
		return eventIsPlaying;
	}

	private void PlayEvent()
	{
		if (PostEvent())
		{
			currentDurationProportion = 1f;
			previousEventStartTime = 0f;
		}
	}

	private void RetriggerEvent(Playable playable)
	{
		wasScrubbingAndRequiresRetrigger = false;
		if (PostEvent())
		{
			currentDurationProportion = 1f - SeekToTime(playable);
			previousEventStartTime = (float)playable.GetTime();
		}
	}

	private float GetProportionalTime(Playable playable)
	{
		if (eventDurationMax == eventDurationMin && eventDurationMin != -1f)
		{
			return (float)playable.GetTime() % eventDurationMax / eventDurationMax;
		}
		float num = (float)playable.GetTime() - previousEventStartTime;
		float num2 = ((currentDuration == -1f) ? ((float)playable.GetDuration()) : currentDuration);
		return num % num2 / num2;
	}

	private float SeekToTime(Playable playable)
	{
		float proportionalTime = GetProportionalTime(playable);
		if (proportionalTime >= 1f)
		{
			return 1f;
		}
		if (eventIsPlaying)
		{
			AkSoundEngine.SeekOnEvent(akEvent.Id, eventObject, proportionalTime);
		}
		return proportionalTime;
	}
}
