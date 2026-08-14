using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkRTPCPlayableBehaviour : PlayableBehaviour
{
	[SerializeField]
	private float RTPCValue;

	public bool setRTPCGlobally { private get; set; }

	public bool overrideTrackObject { private get; set; }

	public GameObject rtpcObject { private get; set; }

	public RTPC parameter { private get; set; }

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (parameter != null)
		{
			if (!overrideTrackObject)
			{
				GameObject gameObject = playerData as GameObject;
				if (gameObject != null)
				{
					rtpcObject = gameObject;
				}
			}
			if (setRTPCGlobally || rtpcObject == null)
			{
				parameter.SetGlobalValue(RTPCValue);
			}
			else
			{
				parameter.SetValue(rtpcObject, RTPCValue);
			}
		}
		base.ProcessFrame(playable, info, playerData);
	}
}
