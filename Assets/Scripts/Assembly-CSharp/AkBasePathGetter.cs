using System.IO;
using UnityEngine;

public class AkBasePathGetter
{
	public static readonly string DefaultBasePath = Path.Combine("Audio", "GeneratedSoundBanks");

	private static bool LogWarnings_Internal = true;

	private const string DecodedBankFolder = "DecodedBanks";

	public static bool LogWarnings
	{
		get
		{
			return LogWarnings_Internal;
		}
		set
		{
			LogWarnings_Internal = value;
		}
	}

	public static string SoundBankBasePath { get; private set; }

	public static string PersistentDataPath { get; private set; }

	public static string DecodedBankFullPath { get; private set; }

	public static string GetPlatformName()
	{
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(empty))
		{
			return empty;
		}
		return "Windows";
	}

	public static string GetPlatformBasePath()
	{
		string platformName = GetPlatformName();
		string text = string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			text = AkWwiseInitializationSettings.ActivePlatformSettings.SoundbankPath;
		}
		text = Path.Combine(Application.streamingAssetsPath, text);
		string path = Path.Combine(text, platformName);
		AkUtilities.FixSlashes(ref path);
		return path;
	}

	public static void EvaluateGamePaths()
	{
		string text = (PersistentDataPath = Application.persistentDataPath);
		string soundBankPersistentDataPath = AkWwiseInitializationSettings.ActivePlatformSettings.SoundBankPersistentDataPath;
		string text2 = null;
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(soundBankPersistentDataPath))
		{
			text2 = Path.GetFullPath(Path.Combine(text, soundBankPersistentDataPath));
			if (LogWarnings)
			{
				Debug.LogFormat("WwiseUnity: Using persistentDataPath. SoundBanks base path set to <{0}>.", text2);
			}
		}
		else
		{
			text2 = GetPlatformBasePath();
			bool flag = File.Exists(Path.Combine(text2, "Init.bnk"));
			if ((string.IsNullOrEmpty(text2) || !flag) && LogWarnings)
			{
				Debug.LogErrorFormat("WwiseUnity: Could not locate the SoundBanks in {0}. Did you make sure to copy them to the StreamingAssets folder?", text2);
			}
		}
		SoundBankBasePath = text2;
		DecodedBankFullPath = Path.Combine(text2, "DecodedBanks");
	}
}
