using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AkTimelineRtpcPlayable : PlayableAsset, ITimelineClipAsset
{
	public RTPC RTPC = new RTPC();

	public bool setGlobally;

	public AkTimelineRtpcPlayableBehaviour template = new AkTimelineRtpcPlayableBehaviour();

	public TimelineClip owningClip { get; set; }

	ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.None;

	public void SetupClipDisplay()
	{
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject)
	{
		ScriptPlayable<AkTimelineRtpcPlayableBehaviour> scriptPlayable = ScriptPlayable<AkTimelineRtpcPlayableBehaviour>.Create(graph, template);
		AkTimelineRtpcPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.RTPC = RTPC;
		behaviour.setGlobally = setGlobally;
		behaviour.gameObject = gameObject;
		return scriptPlayable;
	}
}
