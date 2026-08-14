using Steamworks;
using UnityEngine;
using drl.backend;

namespace drl
{
	public static class WebBrowser
	{
		public static void OpenURL(string p_url, PlatformService platformService)
		{
			if (SteamUtils.IsOverlayEnabled())
			{
				SteamFriends.ActivateGameOverlayToWebPage(p_url);
			}
			else
			{
				Application.OpenURL(p_url);
			}
		}
	}
}
