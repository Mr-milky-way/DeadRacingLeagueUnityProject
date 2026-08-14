using System;
using GameAnalyticsSDK;
using UnityEngine;

namespace drl.analytics
{
	public class GAService
	{
		private const string GAME_KEY = "8f00e2cfd2d7fb749d988a086dec6339";

		private const string SECRET_KEY = "d0f505e49638d0e4799c5088a331681b80c53ad4";

		public readonly GADesign Design = new GADesign();

		public void Initialize(string build)
		{
			int num = -1;
			switch (Application.platform)
			{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.PS4:
			case RuntimePlatform.XboxOne:
				num = 0;
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				num = 1;
				break;
			default:
				Debug.LogWarning($"DRLAnalyticsController > Platform {Application.platform} not supported for Analytics yet");
				break;
			}
			if (num < 0)
			{
				return;
			}
			if (!Application.isEditor)
			{
				GameAnalytics.SettingsGA.InfoLogBuild = false;
				GameAnalytics.SettingsGA.InfoLogEditor = false;
				GameAnalytics.SettingsGA.Build[num] = build;
				GameAnalytics.SettingsGA.UpdateGameKey(num, "8f00e2cfd2d7fb749d988a086dec6339");
				GameAnalytics.SettingsGA.UpdateSecretKey(num, "d0f505e49638d0e4799c5088a331681b80c53ad4");
				bool flag = true;
				try
				{
					GameAnalytics.Initialize();
				}
				catch (Exception ex)
				{
					Debug.Log("GAService> Initialize / Error\n" + ex.Message);
					flag = false;
				}
				if (flag)
				{
					Debug.Log("GAService> Initialize / GameAnalytics Initialized");
				}
			}
			else
			{
				GameAnalytics.SettingsGA.SubmitFpsAverage = false;
				GameAnalytics.SettingsGA.SubmitFpsAverage = false;
			}
		}
	}
}
