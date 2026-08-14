using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkEmitterObstructionOcclusion")]
[RequireComponent(typeof(AkGameObj))]
public class AkEmitterObstructionOcclusion : AkObstructionOcclusion
{
	private AkGameObj m_gameObj;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_gameObj = GetComponent<AkGameObj>();
	}

	protected override void UpdateCurrentListenerList()
	{
		if (AkSpatialAudioListener.TheSpatialAudioListener != null && AkRoom.RoomCount > 0)
		{
			currentListenerList.Add(AkSpatialAudioListener.TheSpatialAudioListener);
			return;
		}
		if (m_gameObj.IsUsingDefaultListeners)
		{
			currentListenerList.AddRange(AkAudioListener.DefaultListeners.ListenerList);
		}
		currentListenerList.AddRange(m_gameObj.ListenerList);
	}

	protected override void SetObstructionOcclusion(KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		if (AkSpatialAudioListener.TheSpatialAudioListener != null && AkRoom.RoomCount > 0)
		{
			AkSoundEngine.SetObjectObstructionAndOcclusion(base.gameObject, ObsOccPair.Key.gameObject, ObsOccPair.Value.currentValue, 0f);
		}
		else
		{
			AkSoundEngine.SetObjectObstructionAndOcclusion(base.gameObject, ObsOccPair.Key.gameObject, 0f, ObsOccPair.Value.currentValue);
		}
	}
}
