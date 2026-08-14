using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(AkTimelineEventPlayable))]
[TrackBindingType(typeof(GameObject))]
public class AkTimelineEventTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<AkTimelineEventPlayableBehavior> scriptPlayable = ScriptPlayable<AkTimelineEventPlayableBehavior>.Create(graph);
		scriptPlayable.SetInputCount(inputCount);
		foreach (TimelineClip clip in GetClips())
		{
			(clip.asset as AkTimelineEventPlayable).owningClip = clip;
		}
		return scriptPlayable;
	}
}
