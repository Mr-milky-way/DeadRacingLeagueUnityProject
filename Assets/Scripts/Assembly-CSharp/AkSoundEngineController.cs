using UnityEngine;

public class AkSoundEngineController
{
	private static AkSoundEngineController ms_Instance;

	public static AkSoundEngineController Instance
	{
		get
		{
			if (ms_Instance == null)
			{
				ms_Instance = new AkSoundEngineController();
			}
			return ms_Instance;
		}
	}

	private AkSoundEngineController()
	{
	}

	~AkSoundEngineController()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
		}
	}

	public void LateUpdate()
	{
		AkRoomManager.Update();
		AkRoomAwareManager.UpdateRoomAwareObjects();
		AkCallbackManager.PostCallbacks();
		AkBankManager.DoUnloadBanks();
		AkSoundEngine.RenderAudio();
	}

	public void Init(AkInitializer akInitializer)
	{
		if (true)
		{
			AkRoomManager.Init();
		}
		if (akInitializer == null)
		{
			Debug.LogError("WwiseUnity: AkInitializer must not be null. Sound engine will not be initialized.");
			return;
		}
		bool num = AkSoundEngine.IsInitialized();
		AkLogger.Instance.Init();
		if (num)
		{
			Debug.LogError("WwiseUnity: Sound engine is already initialized.");
		}
		else
		{
			AkWwiseInitializationSettings.InitializeSoundEngine();
		}
	}

	public void OnDisable()
	{
	}

	public void Terminate()
	{
		AkWwiseInitializationSettings.TerminateSoundEngine();
		AkRoomManager.Terminate();
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		ActivateAudio(!pauseStatus);
	}

	public void OnApplicationFocus(bool focus)
	{
		ActivateAudio(focus, AkWwiseInitializationSettings.ActivePlatformSettings.RenderDuringFocusLoss);
	}

	private void ActivateAudio(bool activate, bool renderAnyway = false)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (activate)
			{
				AkSoundEngine.WakeupFromSuspend();
			}
			else
			{
				AkSoundEngine.Suspend(renderAnyway);
			}
			AkSoundEngine.RenderAudio();
		}
	}
}
