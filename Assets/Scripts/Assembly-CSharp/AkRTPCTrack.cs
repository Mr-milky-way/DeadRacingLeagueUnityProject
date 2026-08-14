using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.32f, 0.13f, 0.13f)]
[TrackClipType(typeof(AkRTPCPlayable))]
[TrackBindingType(typeof(GameObject))]
[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkRTPCTrack : TrackAsset
{
	public RTPC Parameter;

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<AkRTPCPlayableBehaviour> scriptPlayable = ScriptPlayable<AkRTPCPlayableBehaviour>.Create(graph, inputCount);
		setPlayableProperties();
		return scriptPlayable;
	}

	public void setPlayableProperties()
	{
		foreach (TimelineClip clip in GetClips())
		{
			AkRTPCPlayable obj = (AkRTPCPlayable)clip.asset;
			obj.Parameter = Parameter;
			obj.OwningClip = clip;
		}
	}

	public void OnValidate()
	{
		foreach (TimelineClip clip in GetClips())
		{
			((AkRTPCPlayable)clip.asset).Parameter = Parameter;
		}
	}
}
