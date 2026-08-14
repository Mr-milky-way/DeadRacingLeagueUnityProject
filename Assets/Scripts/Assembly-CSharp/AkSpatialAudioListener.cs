using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkSpatialAudioListener")]
[RequireComponent(typeof(AkAudioListener))]
[RequireComponent(typeof(AkRoomAwareObject))]
[DisallowMultipleComponent]
public class AkSpatialAudioListener : MonoBehaviour
{
	public class SpatialAudioListenerList
	{
		private readonly List<AkSpatialAudioListener> listenerList = new List<AkSpatialAudioListener>();

		public List<AkSpatialAudioListener> ListenerList => listenerList;

		public bool Add(AkSpatialAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			if (listenerList.Contains(listener))
			{
				return false;
			}
			listenerList.Add(listener);
			Refresh();
			return true;
		}

		public bool Remove(AkSpatialAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			if (!listenerList.Remove(listener))
			{
				return false;
			}
			Refresh();
			return true;
		}

		private void Refresh()
		{
			if (ListenerList.Count == 1)
			{
				if (s_SpatialAudioListener != null)
				{
					AkSoundEngine.UnregisterSpatialAudioListener(s_SpatialAudioListener.gameObject);
				}
				s_SpatialAudioListener = ListenerList[0];
				AkSoundEngine.RegisterSpatialAudioListener(s_SpatialAudioListener.gameObject);
			}
			else if (ListenerList.Count == 0 && s_SpatialAudioListener != null)
			{
				AkSoundEngine.UnregisterSpatialAudioListener(s_SpatialAudioListener.gameObject);
				s_SpatialAudioListener = null;
			}
		}
	}

	private static AkSpatialAudioListener s_SpatialAudioListener;

	private static readonly SpatialAudioListenerList spatialAudioListeners = new SpatialAudioListenerList();

	private AkAudioListener AkAudioListener;

	public static AkAudioListener TheSpatialAudioListener
	{
		get
		{
			if (!(s_SpatialAudioListener != null))
			{
				return null;
			}
			return s_SpatialAudioListener.AkAudioListener;
		}
	}

	public static SpatialAudioListenerList SpatialAudioListeners => spatialAudioListeners;

	private void Awake()
	{
		AkAudioListener = GetComponent<AkAudioListener>();
	}

	private void OnEnable()
	{
		spatialAudioListeners.Add(this);
	}

	private void OnDisable()
	{
		spatialAudioListeners.Remove(this);
	}
}
