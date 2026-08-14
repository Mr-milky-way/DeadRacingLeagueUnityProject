using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkRoomPortalObstruction")]
[RequireComponent(typeof(AkRoomPortal))]
public class AkRoomPortalObstruction : AkObstructionOcclusion
{
	private AkRoomPortal m_portal;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_portal = GetComponent<AkRoomPortal>();
	}

	protected override void UpdateCurrentListenerList()
	{
		currentListenerList.Add(AkSpatialAudioListener.TheSpatialAudioListener);
	}

	protected override void SetObstructionOcclusion(KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		if (m_portal.IsValid)
		{
			AkSoundEngine.SetPortalObstructionAndOcclusion(m_portal.GetID(), ObsOccPair.Value.currentValue, 0f);
		}
	}
}
