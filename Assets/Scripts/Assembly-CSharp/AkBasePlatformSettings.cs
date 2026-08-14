using UnityEngine;

public class AkBasePlatformSettings : ScriptableObject
{
	public virtual AkInitializationSettings AkInitializationSettings => new AkInitializationSettings();

	public virtual AkSpatialAudioInitSettings AkSpatialAudioInitSettings => new AkSpatialAudioInitSettings();

	public virtual AkCallbackManager.InitializationSettings CallbackManagerInitializationSettings => new AkCallbackManager.InitializationSettings();

	public virtual string SoundBankPersistentDataPath => null;

	public virtual string InitialLanguage => "English(US)";

	public virtual bool RenderDuringFocusLoss => false;

	public virtual string SoundbankPath => AkBasePathGetter.DefaultBasePath;

	public virtual AkCommunicationSettings AkCommunicationSettings => new AkCommunicationSettings();

	public virtual bool UseAsyncOpen => false;
}
