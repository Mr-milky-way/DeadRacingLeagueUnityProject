using GameAnalyticsSDK.Wrapper;

namespace GameAnalyticsSDK.Events
{
	public static class GA_Ads
	{
		public static void NewEvent(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement, long duration)
		{
			GA_Wrapper.AddAdEventWithDuration(adAction, adType, adSdkName, adPlacement, duration);
		}

		public static void NewEvent(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement, GAAdError noAdReason)
		{
			GA_Wrapper.AddAdEventWithReason(adAction, adType, adSdkName, adPlacement, noAdReason);
		}

		public static void NewEvent(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement)
		{
			GA_Wrapper.AddAdEvent(adAction, adType, adSdkName, adPlacement);
		}
	}
}
