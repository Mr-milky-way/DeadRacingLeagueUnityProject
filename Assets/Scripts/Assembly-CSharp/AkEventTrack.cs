using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(AkEventPlayable))]
[TrackBindingType(typeof(GameObject))]
[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkEventTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<AkEventPlayableBehavior> scriptPlayable = ScriptPlayable<AkEventPlayableBehavior>.Create(graph);
		scriptPlayable.SetInputCount(inputCount);
		foreach (TimelineClip clip in GetClips())
		{
			(clip.asset as AkEventPlayable).owningClip = clip;
		}
		return scriptPlayable;
	}
}
