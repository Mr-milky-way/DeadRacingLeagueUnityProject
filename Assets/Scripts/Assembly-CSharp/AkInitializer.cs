using UnityEngine;

[AddComponentMenu("Wwise/AkInitializer")]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class AkInitializer : MonoBehaviour
{
	private static AkInitializer ms_Instance;

	public AkWwiseInitializationSettings InitializationSettings;

	private void Awake()
	{
		if ((bool)ms_Instance)
		{
			Object.DestroyImmediate(this);
			return;
		}
		ms_Instance = this;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(this);
		}
	}

	private void OnEnable()
	{
		InitializationSettings = AkWwiseInitializationSettings.Instance;
		if (ms_Instance == this && Application.isPlaying)
		{
			AkSoundEngineController.Instance.Init(this);
		}
	}

	private void OnDisable()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.OnDisable();
		}
	}

	private void OnDestroy()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (ms_Instance == this && Application.isPlaying)
		{
			AkSoundEngineController.Instance.OnApplicationPause(pauseStatus);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (ms_Instance == this && Application.isPlaying)
		{
			AkSoundEngineController.Instance.OnApplicationFocus(focus);
		}
	}

	private void OnApplicationQuit()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.Terminate();
		}
	}

	private void LateUpdate()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.LateUpdate();
		}
	}
}
