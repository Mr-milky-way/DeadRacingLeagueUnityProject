using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkEventPlayable : PlayableAsset, ITimelineClipAsset
{
	public AK.Wwise.Event akEvent = new AK.Wwise.Event();

	[SerializeField]
	private AkCurveInterpolation blendInCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

	[SerializeField]
	private AkCurveInterpolation blendOutCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

	[SerializeField]
	private ExposedReference<GameObject> emitterObjectRef;

	public float eventDurationMax = -1f;

	public float eventDurationMin = -1f;

	[NonSerialized]
	public TimelineClip owningClip;

	[SerializeField]
	private bool retriggerEvent;

	public bool UseWwiseEventDuration = true;

	[SerializeField]
	private bool StopEventAtClipEnd = true;

	ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.Looping | ClipCaps.Blending;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<AkEventPlayableBehavior> scriptPlayable = ScriptPlayable<AkEventPlayableBehavior>.Create(graph);
		GameObject gameObject = emitterObjectRef.Resolve(graph.GetResolver());
		if (gameObject == null)
		{
			gameObject = owner;
		}
		if (gameObject == null || akEvent == null)
		{
			return scriptPlayable;
		}
		AkEventPlayableBehavior behaviour = scriptPlayable.GetBehaviour();
		behaviour.akEvent = akEvent;
		behaviour.blendInCurve = blendInCurve;
		behaviour.blendOutCurve = blendOutCurve;
		if (owningClip != null)
		{
			behaviour.easeInDuration = (float)owningClip.easeInDuration;
			behaviour.easeOutDuration = (float)owningClip.easeOutDuration;
			behaviour.blendInDuration = (float)owningClip.blendInDuration;
			behaviour.blendOutDuration = (float)owningClip.blendOutDuration;
		}
		else
		{
			behaviour.easeInDuration = (behaviour.easeOutDuration = (behaviour.blendInDuration = (behaviour.blendOutDuration = 0f)));
		}
		behaviour.retriggerEvent = retriggerEvent;
		behaviour.StopEventAtClipEnd = StopEventAtClipEnd;
		behaviour.eventObject = gameObject;
		behaviour.overrideTrackEmitterObject = gameObject != null;
		behaviour.eventDurationMin = eventDurationMin;
		behaviour.eventDurationMax = eventDurationMax;
		return scriptPlayable;
	}
}
