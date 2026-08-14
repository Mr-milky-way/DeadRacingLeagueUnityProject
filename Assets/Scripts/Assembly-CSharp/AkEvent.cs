using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Wwise/AkEvent")]
[RequireComponent(typeof(AkGameObj))]
public class AkEvent : AkDragDropTriggerHandler
{
	[Serializable]
	public class CallbackData
	{
		public CallbackFlags Flags;

		public string FunctionName;

		public GameObject GameObject;

		public void CallFunction(AkEventCallbackMsg eventCallbackMsg)
		{
			if (((uint)eventCallbackMsg.type & Flags.value) != 0 && (bool)GameObject)
			{
				GameObject.SendMessage(FunctionName, eventCallbackMsg);
			}
		}
	}

	public AkActionOnEventType actionOnEventType;

	public AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public bool enableActionOnEvent;

	public AK.Wwise.Event data = new AK.Wwise.Event();

	public bool useCallbacks;

	public List<CallbackData> Callbacks = new List<CallbackData>();

	public uint playingId;

	public GameObject soundEmitterObject;

	public float transitionDuration;

	private AkEventCallbackMsg EventCallbackMsg;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("eventID")]
	private int eventIdInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("valueGuid")]
	private byte[] valueGuidInternal;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("m_callbackData")]
	private AkEventCallbackData m_callbackDataInternal;

	protected override BaseType WwiseType => data;

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.2 and will be removed in a future release.")]
	public int eventID
	{
		get
		{
			if (data != null)
			{
				return (int)data.Id;
			}
			return 0;
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public byte[] valueGuid
	{
		get
		{
			if (data == null)
			{
				return null;
			}
			WwiseObjectReference objectReference = data.ObjectReference;
			if ((bool)objectReference)
			{
				return objectReference.Guid.ToByteArray();
			}
			return null;
		}
	}

	[Obsolete("This functionality is deprecated as of Wwise v2018.1.6 and will be removed in a future release.")]
	public AkEventCallbackData m_callbackData => m_callbackDataInternal;

	protected override void Start()
	{
		if (useCallbacks)
		{
			EventCallbackMsg = new AkEventCallbackMsg
			{
				sender = base.gameObject
			};
		}
		base.Start();
	}

	private void Callback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		EventCallbackMsg.type = in_type;
		EventCallbackMsg.info = in_info;
		for (int i = 0; i < Callbacks.Count; i++)
		{
			Callbacks[i].CallFunction(EventCallbackMsg);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		GameObject gameObject = (soundEmitterObject = ((useOtherObject && in_gameObject != null) ? in_gameObject : base.gameObject));
		if (enableActionOnEvent)
		{
			data.ExecuteAction(gameObject, actionOnEventType, (int)transitionDuration * 1000, curveInterpolation);
			return;
		}
		if (useCallbacks)
		{
			uint num = 0u;
			for (int i = 0; i < Callbacks.Count; i++)
			{
				if ((bool)Callbacks[i].GameObject && !string.IsNullOrEmpty(Callbacks[i].FunctionName))
				{
					num |= Callbacks[i].Flags.value;
				}
			}
			if (num != 0)
			{
				playingId = data.Post(gameObject, num, Callback);
				return;
			}
		}
		playingId = data.Post(gameObject);
	}

	public void Stop(int _transitionDuration)
	{
		Stop(_transitionDuration, AkCurveInterpolation.AkCurveInterpolation_Linear);
	}

	public void Stop(int _transitionDuration, AkCurveInterpolation _curveInterpolation)
	{
		data.Stop(soundEmitterObject, _transitionDuration, _curveInterpolation);
	}
}
