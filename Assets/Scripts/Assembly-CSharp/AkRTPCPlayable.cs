using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkRTPCPlayable : PlayableAsset, ITimelineClipAsset
{
	public bool overrideTrackObject;

	public ExposedReference<GameObject> RTPCObject;

	public bool setRTPCGlobally;

	public AkRTPCPlayableBehaviour template = new AkRTPCPlayableBehaviour();

	public RTPC Parameter { get; set; }

	public TimelineClip OwningClip { get; set; }

	ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
	{
		ScriptPlayable<AkRTPCPlayableBehaviour> scriptPlayable = ScriptPlayable<AkRTPCPlayableBehaviour>.Create(graph, template);
		AkRTPCPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.overrideTrackObject = overrideTrackObject;
		behaviour.setRTPCGlobally = setRTPCGlobally;
		behaviour.rtpcObject = (overrideTrackObject ? RTPCObject.Resolve(graph.GetResolver()) : go);
		behaviour.parameter = Parameter;
		return scriptPlayable;
	}
}
