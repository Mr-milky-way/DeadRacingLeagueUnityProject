using System.Text.RegularExpressions;
using GameAnalyticsSDK;
using UnityEngine;

namespace drl.analytics
{
	public class GADesign
	{
		public readonly GAUserInterface UI = new GAUserInterface();

		public readonly GAGameplay Gameplay = new GAGameplay();

		public readonly GAControllers Controllers = new GAControllers();

		public readonly GATryouts Tryouts = new GATryouts();

		public static void DesignEvent(string eventId)
		{
			if (!string.IsNullOrEmpty(eventId))
			{
				eventId = Regex.Replace(eventId, "[^\\w:]", "");
				Debug.Log("GAService > DesignEvent id[" + eventId + "]");
				if (!Application.isEditor)
				{
					GameAnalytics.NewDesignEvent(eventId);
				}
			}
		}
	}
}
