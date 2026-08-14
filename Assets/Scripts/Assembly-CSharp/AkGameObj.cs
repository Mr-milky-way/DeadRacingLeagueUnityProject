using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkGameObj")]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[DefaultExecutionOrder(-25)]
public class AkGameObj : MonoBehaviour
{
	[SerializeField]
	private AkGameObjListenerList m_listeners = new AkGameObjListenerList();

	public bool isEnvironmentAware = true;

	[SerializeField]
	private bool isStaticObject;

	private Collider m_Collider;

	private AkGameObjEnvironmentData m_envData;

	private AkGameObjPositionData m_posData;

	public AkGameObjPositionOffsetData m_positionOffsetData;

	private bool isRegistered;

	[HideInInspector]
	[SerializeField]
	private AkGameObjPosOffsetData m_posOffsetData;

	private const int AK_NUM_LISTENERS = 8;

	[HideInInspector]
	[SerializeField]
	private int listenerMask = 1;

	public bool IsUsingDefaultListeners => m_listeners.useDefaultListeners;

	public List<AkAudioListener> ListenerList => m_listeners.ListenerList;

	internal void AddListener(AkAudioListener listener)
	{
		m_listeners.Add(listener);
	}

	internal void RemoveListener(AkAudioListener listener)
	{
		m_listeners.Remove(listener);
	}

	public AKRESULT Register()
	{
		if (isRegistered)
		{
			return AKRESULT.AK_Success;
		}
		isRegistered = true;
		return AkSoundEngine.RegisterGameObj(base.gameObject, base.gameObject.name);
	}

	private void SetPosition()
	{
		Vector3 position = GetPosition();
		Vector3 forward = GetForward();
		Vector3 upward = GetUpward();
		if (m_posData != null)
		{
			if (m_posData.position == position && m_posData.forward == forward && m_posData.up == upward)
			{
				return;
			}
			m_posData.position = position;
			m_posData.forward = forward;
			m_posData.up = upward;
		}
		AkSoundEngine.SetObjectPosition(base.gameObject, position, forward, upward);
	}

	private void Awake()
	{
		if (!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}
		m_Collider = GetComponent<Collider>();
		if (Register() != AKRESULT.AK_Success)
		{
			return;
		}
		SetPosition();
		if (isEnvironmentAware)
		{
			m_envData = new AkGameObjEnvironmentData();
			if ((bool)m_Collider)
			{
				m_envData.AddAkEnvironment(m_Collider, m_Collider);
			}
			m_envData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		m_listeners.Init(this);
	}

	private void CheckStaticStatus()
	{
	}

	private void OnEnable()
	{
		base.enabled = !isStaticObject;
	}

	private void OnDestroy()
	{
		AkTriggerHandler[] components = base.gameObject.GetComponents<AkTriggerHandler>();
		foreach (AkTriggerHandler akTriggerHandler in components)
		{
			if (akTriggerHandler.triggerList.Contains(-358577003))
			{
				akTriggerHandler.DoDestroy();
			}
		}
		if (AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.UnregisterGameObj(base.gameObject);
		}
	}

	private void Update()
	{
		if (m_envData != null)
		{
			m_envData.UpdateAuxSend(base.gameObject, base.transform.position);
		}
		if (!isStaticObject)
		{
			SetPosition();
		}
	}

	public virtual Vector3 GetPosition()
	{
		if (m_positionOffsetData == null)
		{
			return base.transform.position;
		}
		Vector3 vector = base.transform.rotation * m_positionOffsetData.positionOffset;
		return base.transform.position + vector;
	}

	public virtual Vector3 GetForward()
	{
		return base.transform.forward;
	}

	public virtual Vector3 GetUpward()
	{
		return base.transform.up;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.AddAkEnvironment(other, m_Collider);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.RemoveAkEnvironment(other, m_Collider);
		}
	}
}
