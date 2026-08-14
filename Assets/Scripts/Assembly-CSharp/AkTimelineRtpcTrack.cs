using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.32f, 0.13f, 0.13f)]
[TrackClipType(typeof(AkTimelineRtpcPlayable))]
[TrackBindingType(typeof(GameObject))]
public class AkTimelineRtpcTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
	{
		ScriptPlayable<AkTimelineRtpcPlayableBehaviour> scriptPlayable = ScriptPlayable<AkTimelineRtpcPlayableBehaviour>.Create(graph, inputCount);
		foreach (TimelineClip clip in GetClips())
		{
			AkTimelineRtpcPlayable obj = clip.asset as AkTimelineRtpcPlayable;
			obj.owningClip = clip;
			obj.SetupClipDisplay();
		}
		return scriptPlayable;
	}

	public void OnValidate()
	{
		foreach (TimelineClip clip in GetClips())
		{
			(clip.asset as AkTimelineRtpcPlayable).SetupClipDisplay();
		}
	}
}
