using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkAudioListener")]
[RequireComponent(typeof(AkGameObj))]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-50)]
public class AkAudioListener : MonoBehaviour
{
	public class BaseListenerList
	{
		private readonly List<ulong> listenerIdList = new List<ulong>();

		private readonly List<AkAudioListener> listenerList = new List<AkAudioListener>();

		public List<AkAudioListener> ListenerList => listenerList;

		public virtual bool Add(AkAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			ulong akGameObjectID = listener.GetAkGameObjectID();
			if (listenerIdList.Contains(akGameObjectID))
			{
				return false;
			}
			listenerIdList.Add(akGameObjectID);
			listenerList.Add(listener);
			return true;
		}

		public virtual bool Remove(AkAudioListener listener)
		{
			if (listener == null)
			{
				return false;
			}
			ulong akGameObjectID = listener.GetAkGameObjectID();
			if (!listenerIdList.Remove(akGameObjectID))
			{
				return false;
			}
			listenerList.Remove(listener);
			return true;
		}

		public ulong[] GetListenerIds()
		{
			return listenerIdList.ToArray();
		}
	}

	public class DefaultListenerList : BaseListenerList
	{
		public override bool Add(AkAudioListener listener)
		{
			bool num = base.Add(listener);
			if (num && AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.AddDefaultListener(listener.gameObject);
			}
			return num;
		}

		public override bool Remove(AkAudioListener listener)
		{
			bool num = base.Remove(listener);
			if (num && AkSoundEngine.IsInitialized())
			{
				AkSoundEngine.RemoveDefaultListener(listener.gameObject);
			}
			return num;
		}
	}

	private static readonly DefaultListenerList defaultListeners = new DefaultListenerList();

	private ulong akGameObjectID = ulong.MaxValue;

	private List<AkGameObj> EmittersToStartListeningTo = new List<AkGameObj>();

	private List<AkGameObj> EmittersToStopListeningTo = new List<AkGameObj>();

	public bool isDefaultListener = true;

	[SerializeField]
	public int listenerId;

	public static DefaultListenerList DefaultListeners => defaultListeners;

	public void StartListeningToEmitter(AkGameObj emitter)
	{
		EmittersToStartListeningTo.Add(emitter);
		EmittersToStopListeningTo.Remove(emitter);
	}

	public void StopListeningToEmitter(AkGameObj emitter)
	{
		EmittersToStartListeningTo.Remove(emitter);
		EmittersToStopListeningTo.Add(emitter);
	}

	public void SetIsDefaultListener(bool isDefault)
	{
		if (isDefaultListener != isDefault)
		{
			isDefaultListener = isDefault;
			if (isDefault)
			{
				DefaultListeners.Add(this);
			}
			else
			{
				DefaultListeners.Remove(this);
			}
		}
	}

	private void Awake()
	{
		AkGameObj component = GetComponent<AkGameObj>();
		if ((bool)component)
		{
			component.Register();
		}
		akGameObjectID = AkSoundEngine.GetAkGameObjectID(base.gameObject);
	}

	private void OnEnable()
	{
		if (isDefaultListener)
		{
			DefaultListeners.Add(this);
		}
	}

	private void OnDisable()
	{
		if (isDefaultListener)
		{
			DefaultListeners.Remove(this);
		}
	}

	private void Update()
	{
		for (int i = 0; i < EmittersToStartListeningTo.Count; i++)
		{
			EmittersToStartListeningTo[i].AddListener(this);
		}
		EmittersToStartListeningTo.Clear();
		for (int j = 0; j < EmittersToStopListeningTo.Count; j++)
		{
			EmittersToStopListeningTo[j].RemoveListener(this);
		}
		EmittersToStopListeningTo.Clear();
	}

	public ulong GetAkGameObjectID()
	{
		return akGameObjectID;
	}

	public void Migrate14()
	{
		bool flag = listenerId == 0;
		Debug.Log("WwiseUnity: AkAudioListener.Migrate14 for " + base.gameObject.name);
		isDefaultListener = flag;
	}
}
