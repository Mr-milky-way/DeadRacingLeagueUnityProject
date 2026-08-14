using System;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	public static class PhotonUtils
	{
		public static Region BestRegion
		{
			get
			{
				Region result = null;
				int num = int.MaxValue;
				if (PhotonNetwork.networkingPeer == null || PhotonNetwork.networkingPeer.AvailableRegions == null)
				{
					return result;
				}
				foreach (Region availableRegion in PhotonNetwork.networkingPeer.AvailableRegions)
				{
					if (availableRegion.Ping != 0 && availableRegion.Ping < num)
					{
						num = availableRegion.Ping;
						result = availableRegion;
					}
				}
				return result;
			}
		}

		public static string GetRegionName(CloudRegionCode regionCode)
		{
			return regionCode switch
			{
				CloudRegionCode.asia => "multiplayer.regions.asia", 
				CloudRegionCode.au => "multiplayer.regions.australia", 
				CloudRegionCode.cae => "multiplayer.regions.east-canada", 
				CloudRegionCode.eu => "multiplayer.regions.europe", 
				CloudRegionCode.@in => "multiplayer.regions.india", 
				CloudRegionCode.jp => "multiplayer.regions.japan", 
				CloudRegionCode.kr => "multiplayer.regions.south-korea", 
				CloudRegionCode.none => "multiplayer.regions.auto", 
				CloudRegionCode.sa => "multiplayer.regions.south-america", 
				CloudRegionCode.us => "multiplayer.regions.north-america", 
				CloudRegionCode.usw => "multiplayer.regions.us-west", 
				_ => "multiplayer.regions.uknown", 
			};
		}

		public static float GetPingQualityLevel(int ping)
		{
			if ((float)ping < 120f)
			{
				return Mathf.Lerp(0.8f, 1f, Mathf.InverseLerp(120f, 20f, ping));
			}
			if ((float)ping < 250f)
			{
				return Mathf.Lerp(0.5f, 0.8f, Mathf.InverseLerp(250f, 120f, ping));
			}
			if ((float)ping < 400f)
			{
				return Mathf.Lerp(0.1f, 0.5f, Mathf.InverseLerp(400f, 250f, ping));
			}
			return 0f;
		}

		public static string TimeAgo(DateTime utcDate, Localization loc = null)
		{
			string result = (loc ? loc.Get("social.chat.message.time-ago.justnow", "just now") : "just now");
			string text = (loc ? loc.Get("social.chat.message.time-ago.months-ago", "months ago") : "months ago");
			string text2 = (loc ? loc.Get("social.chat.message.time-ago.days-ago", "days ago") : "days ago");
			string text3 = (loc ? loc.Get("social.chat.message.time-ago.hours-ago", "hrs ago") : "hrs ago");
			string text4 = (loc ? loc.Get("social.chat.message.time-ago.mins-ago", "mins ago") : "mins ago");
			string result2 = (loc ? loc.Get("social.chat.message.time-ago.one-min-ago", "1 min ago") : "1 min ago");
			string result3 = (loc ? loc.Get("social.chat.message.time-ago.one-min-ago", "a moment ago") : "a moment ago");
			int num = (int)(DateTime.UtcNow - utcDate).TotalSeconds;
			if (num < 0)
			{
				return result;
			}
			int num2 = num / 2592000;
			int num3 = num % 2592000 / 86400;
			int num4 = num % 86400 / 3600;
			int num5 = num % 3600 / 60;
			if (num2 > 0)
			{
				return num2 + " " + text;
			}
			if (num3 > 0)
			{
				return num3 + " " + text2;
			}
			if (num4 > 0)
			{
				return num4 + " " + text3;
			}
			if (num5 > 1)
			{
				return num5 + " " + text4;
			}
			if (num > 60)
			{
				return result2;
			}
			if (num > 20)
			{
				return result3;
			}
			return result;
		}
	}
}
