using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkAmbient")]
public class AkAmbient : AkEvent
{
	public static Dictionary<uint, AkMultiPosEvent> multiPosEventTree = new Dictionary<uint, AkMultiPosEvent>();

	public AkMultiPositionType MultiPositionType = AkMultiPositionType.MultiPositionType_MultiSources;

	public MultiPositionTypeLabel multiPositionTypeLabel;

	public AkAmbientLargeModePositioner[] LargeModePositions;

	[HideInInspector]
	[SerializeField]
	public List<Vector3> multiPositionArray;

	private void OnEnable()
	{
		if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode)
		{
			return;
		}
		AkGameObj[] components = base.gameObject.GetComponents<AkGameObj>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
		if (multiPosEventTree.TryGetValue(data.Id, out var value))
		{
			if (!value.list.Contains(this))
			{
				value.list.Add(this);
			}
		}
		else
		{
			value = new AkMultiPosEvent();
			value.list.Add(this);
			multiPosEventTree.Add(data.Id, value);
		}
		AkPositionArray akPositionArray = BuildMultiDirectionArray(value);
		AkSoundEngine.SetMultiplePositions(value.list[0].gameObject, akPositionArray, (ushort)akPositionArray.Count, MultiPositionType);
	}

	protected override void Start()
	{
		base.Start();
		if (multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
		{
			AkGameObj[] components = base.gameObject.GetComponents<AkGameObj>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = true;
			}
		}
		else if (multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			AkGameObj[] components2 = base.gameObject.GetComponents<AkGameObj>();
			for (int j = 0; j < components2.Length; j++)
			{
				components2[j].enabled = false;
			}
			AkPositionArray akPositionArray = BuildAkPositionArray();
			AkSoundEngine.SetMultiplePositions(base.gameObject, akPositionArray, (ushort)akPositionArray.Count, MultiPositionType);
		}
	}

	private void OnDisable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			AkMultiPosEvent akMultiPosEvent = multiPosEventTree[data.Id];
			if (akMultiPosEvent.list.Count == 1)
			{
				multiPosEventTree.Remove(data.Id);
				return;
			}
			akMultiPosEvent.list.Remove(this);
			AkPositionArray akPositionArray = BuildMultiDirectionArray(akMultiPosEvent);
			AkSoundEngine.SetMultiplePositions(akMultiPosEvent.list[0].gameObject, akPositionArray, (ushort)akPositionArray.Count, MultiPositionType);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode)
		{
			base.HandleEvent(in_gameObject);
			return;
		}
		AkMultiPosEvent akMultiPosEvent = multiPosEventTree[data.Id];
		if (!akMultiPosEvent.eventIsPlaying)
		{
			akMultiPosEvent.eventIsPlaying = true;
			soundEmitterObject = akMultiPosEvent.list[0].gameObject;
			if (enableActionOnEvent)
			{
				data.ExecuteAction(soundEmitterObject, actionOnEventType, (int)transitionDuration * 1000, curveInterpolation);
			}
			else
			{
				playingId = data.Post(soundEmitterObject, 1u, akMultiPosEvent.FinishedPlaying);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(base.transform.position, "WwiseAudioSpeaker.png", allowScaling: false);
	}

	public AkPositionArray BuildMultiDirectionArray(AkMultiPosEvent eventPosList)
	{
		AkPositionArray akPositionArray = new AkPositionArray((uint)eventPosList.list.Count);
		for (int i = 0; i < eventPosList.list.Count; i++)
		{
			akPositionArray.Add(eventPosList.list[i].transform.position, eventPosList.list[i].transform.forward, eventPosList.list[i].transform.up);
		}
		return akPositionArray;
	}

	private AkPositionArray BuildAkPositionArray()
	{
		List<AkAmbientLargeModePositioner> list = new List<AkAmbientLargeModePositioner>();
		for (int i = 0; i < LargeModePositions.Length; i++)
		{
			if (LargeModePositions[i] != null && !list.Contains(LargeModePositions[i]))
			{
				list.Add(LargeModePositions[i]);
			}
		}
		AkPositionArray akPositionArray = new AkPositionArray((uint)list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			akPositionArray.Add(list[j].Position, list[j].Forward, list[j].Up);
		}
		return akPositionArray;
	}
}
