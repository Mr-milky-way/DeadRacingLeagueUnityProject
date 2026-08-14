using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ProfileStateModel : Model<DRLApp>
	{
		public Texture photo;

		private List<string> m_inventory;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public string steamId
		{
			get
			{
				return data.Get<string>("steam-id");
			}
			set
			{
				data.Set("steam-id", value);
				Refresh();
			}
		}

		public string xbuid
		{
			get
			{
				return data.Get<string>("xbuid");
			}
			set
			{
				data.Set("xbuid", value);
				Refresh();
			}
		}

		public string playstationId
		{
			get
			{
				return data.Get<string>("playstation-id");
			}
			set
			{
				data.Set("playstation-id", value);
				Refresh();
			}
		}

		public string playerId
		{
			get
			{
				return data.Get<string>("player-id");
			}
			set
			{
				data.Set("player-id", value);
				Refresh();
			}
		}

		public string platformId
		{
			get
			{
				return steamId;
			}
			set
			{
				steamId = value;
			}
		}

		public string branchId
		{
			get
			{
				return data.Get("branch-id", "editor");
			}
			set
			{
				data.Set("branch-id", value);
				Refresh();
			}
		}

		public string steamInstallPath
		{
			get
			{
				return data.Get("steam-install-path", "");
			}
			set
			{
				data.Set("steam-install-path", value);
				Refresh();
			}
		}

		public uint steamUnixSecondsFromPurchase
		{
			get
			{
				return data.Get<uint>("steam-purchase-unix-seconds");
			}
			set
			{
				data.Set("steam-purchase-unix-seconds", value);
				Refresh();
			}
		}

		public string username
		{
			get
			{
				return data.Get("profile-name", "Player");
			}
			set
			{
				data.Set("profile-name", value);
				Refresh();
			}
		}

		public string blockList
		{
			get
			{
				return data.Get("profile-block-list", "Player");
			}
			set
			{
				data.Set("profile-block-list", value);
				Refresh();
			}
		}

		public bool isDeveloper => data.Get("profile-developer", d: false);

		public bool isCommentator
		{
			get
			{
				return data.Get("is-commentator", d: false);
			}
			set
			{
				data.Set("is-commentator", value);
				Refresh();
			}
		}

		public bool isObserver
		{
			get
			{
				return data.Get("is-observer", d: false);
			}
			set
			{
				data.Set("is-observer", value);
				Refresh();
			}
		}

		public string rewardParts => data.Get<string>("profile-reward-parts", null);

		public string fullName
		{
			get
			{
				return data.Get<string>("profile-full-name");
			}
			set
			{
				data.Set("profile-full-name", value);
				Refresh();
			}
		}

		public string email
		{
			get
			{
				return data.Get<string>("profile-email");
			}
			set
			{
				data.Set("profile-email", value);
				Refresh();
			}
		}

		public int age
		{
			get
			{
				return data.Get<int>("profile-age");
			}
			set
			{
				data.Set("profile-age", value);
				Refresh();
			}
		}

		public float score
		{
			get
			{
				return data.Get("profile-score", 0f);
			}
			set
			{
				data.Set("profile-score", value);
				Refresh();
			}
		}

		public bool hasReview
		{
			get
			{
				return data.Get("has-review", d: false);
			}
			set
			{
				data.Set("has-review", value);
				Refresh();
			}
		}

		public bool willPromptReview
		{
			get
			{
				return data.Get("prompt-review", d: false);
			}
			set
			{
				data.Set("prompt-review", value);
				Refresh();
			}
		}

		public string country
		{
			get
			{
				return data.Get<string>("profile-country");
			}
			set
			{
				data.Set("profile-country", value);
				Refresh();
			}
		}

		public string gender
		{
			get
			{
				return data.Get<string>("profile-gender");
			}
			set
			{
				data.Set("profile-gender", value);
				Refresh();
			}
		}

		public string watchDRL
		{
			get
			{
				return data.Get<string>("profile-watch-drl");
			}
			set
			{
				data.Set("profile-watch-drl", value);
				Refresh();
			}
		}

		public string americanCitizen
		{
			get
			{
				return data.Get("profile-american-citizen", "");
			}
			set
			{
				data.Set("profile-american-citizen", value);
			}
		}

		public string experienceNonFPV
		{
			get
			{
				return data.Get("profile-experience-non-fpv", "");
			}
			set
			{
				data.Set("profile-experience-non-fpv", value);
			}
		}

		public string experienceNonFPVYears
		{
			get
			{
				return data.Get("profile-experience-non-fpv-years", "");
			}
			set
			{
				data.Set("profile-experience-non-fpv-years", value);
			}
		}

		public string experienceFPV
		{
			get
			{
				return data.Get("profile-experience-fpv", "");
			}
			set
			{
				data.Set("profile-experience-fpv", value);
			}
		}

		public string experienceFPVYears
		{
			get
			{
				return data.Get("profile-experience-fpv-years", "");
			}
			set
			{
				data.Set("profile-experience-fpv-years", value);
			}
		}

		public string experiencePreferenceFPV
		{
			get
			{
				return data.Get("profile-experience-preference-fpv", "");
			}
			set
			{
				data.Set("profile-experience-preference-fpv", value);
			}
		}

		public string experienceRealLifeRacing
		{
			get
			{
				return data.Get("profile-experience-real-life-racing", "");
			}
			set
			{
				data.Set("profile-experience-real-life-racing", value);
			}
		}

		public string experienceBuiltOwnDrone
		{
			get
			{
				return data.Get("profile-experience-built-own-drone", "");
			}
			set
			{
				data.Set("profile-experience-built-own-drone", value);
			}
		}

		public string affiliationMultiGP
		{
			get
			{
				return data.Get("profile-affiliation-multigp", "");
			}
			set
			{
				data.Set("profile-affiliation-multigp", value);
			}
		}

		public string affiliationMilitary
		{
			get
			{
				return data.Get("profile-affiliation-military", "");
			}
			set
			{
				data.Set("profile-affiliation-military", value);
			}
		}

		public string affiliationAMA
		{
			get
			{
				return data.Get("profile-affiliation-ama", "");
			}
			set
			{
				data.Set("profile-affiliation-ama", value);
			}
		}

		public string photoURL
		{
			get
			{
				string text = data.Get("profile-custom-photo-url", "");
				string text2 = data.Get("profile-steam-photo-url", "");
				string result = data.Get<string>("profile-photo-url");
				if (!string.IsNullOrEmpty(text2))
				{
					result = text2;
				}
				if (!string.IsNullOrEmpty(text))
				{
					result = text;
				}
				return result;
			}
			set
			{
				data.Set("profile-photo-url", value);
				Refresh();
			}
		}

		public int photoSize
		{
			get
			{
				return data.Get("profile-photo-size", 0);
			}
			set
			{
				data.Set("profile-photo-size", value);
				Refresh();
			}
		}

		public string languageISO
		{
			get
			{
				return data.Get<string>("profile-language-iso");
			}
			set
			{
				data.Set("profile-language-iso", value);
				Refresh();
			}
		}

		public string countryISO
		{
			get
			{
				return data.Get<string>("profile-country-iso");
			}
			set
			{
				data.Set("profile-country-iso", value);
				Refresh();
			}
		}

		public int storageReplayFileCount
		{
			get
			{
				return data.Get<int>("storage-replay-file-count");
			}
			set
			{
				data.Set("storage-replay-file-count", value);
				Refresh();
			}
		}

		public string storageReplayMemoryUsage
		{
			get
			{
				return data.Get<string>("storage-replay-memory-usage");
			}
			set
			{
				data.Set("storage-replay-memory-usage", value);
				Refresh();
			}
		}

		public string colorHex
		{
			get
			{
				if (!data.Contains("profile-color"))
				{
					return "ff0000";
				}
				return data.Get<string>("profile-color");
			}
			set
			{
				data.Set("profile-color", value);
				Refresh();
			}
		}

		public Color color
		{
			get
			{
				string text = (data.Contains("profile-color") ? data.Get<string>("profile-color") : "");
				if (string.IsNullOrEmpty(text))
				{
					return Color.clear;
				}
				uint result = 0u;
				uint.TryParse(text, NumberStyles.HexNumber, null, out result);
				return Colorf.RGBToColor(result);
			}
			set
			{
				string v = Colorf.ColorToRGB(value).ToString("x6");
				data.Set("profile-color", v);
				Refresh();
				Notify("settings.profile-color.apply");
			}
		}

		public bool isDRLPilot => data.Get("is-drl-pilot", d: false);

		public bool limitFPS => data.Get("fps-limit", d: false);

		public float dataCompletion
		{
			get
			{
				return data.Get("profile-data-completion", 0f);
			}
			set
			{
				data.Set("profile-data-completion", value);
				Refresh();
			}
		}

		public float flightTime
		{
			get
			{
				return data.Get("flight-time", 0f);
			}
			set
			{
				data.Set("flight-time", value);
				Refresh();
			}
		}

		public NotificationState notificationStateMenu
		{
			get
			{
				return (NotificationState)data.Get("settings-notification-state-menu", 1);
			}
			set
			{
				data.Set("settings-notification-state-menu", (int)value);
				Refresh();
			}
		}

		public NotificationState notificationStateInGame
		{
			get
			{
				return (NotificationState)data.Get("settings-notification-state-ingame", 1);
			}
			set
			{
				data.Set("settings-notification-state-ingame", (int)value);
				Refresh();
			}
		}

		public bool usingTransmitterAdapter
		{
			get
			{
				return data.Get("settings-controller-using-adapter", d: false);
			}
			set
			{
				data.Set("settings-controller-using-adapter", value);
				Refresh();
			}
		}

		public bool invalidateCache
		{
			get
			{
				return data.Get("invalidate-settings-cache", d: false);
			}
			set
			{
				data.Set("invalidate-settings-cache", value);
				Refresh();
			}
		}

		public bool clearMapsCache
		{
			get
			{
				return data.Get("clear-maps-cache", d: true);
			}
			set
			{
				data.Set("clear-maps-cache", value);
				Refresh();
			}
		}

		public List<string> inventory
		{
			get
			{
				if (m_inventory == null)
				{
					m_inventory = new List<string>();
				}
				string[] array = Serialize.FromJson<string[]>(data.Get("profile-inventory", "[]"));
				if (array != null)
				{
					m_inventory.Clear();
					m_inventory = array.ToList();
				}
				return m_inventory;
			}
			set
			{
				m_inventory = value;
			}
		}

		public bool xboxPrivacyUGCBlocked
		{
			get
			{
				return data.Get("xbox-privacy-ugc-blocked", d: false);
			}
			set
			{
				data.Set("xbox-privacy-ugc-blocked", value);
				Refresh();
			}
		}

		public bool ps4PrivacyUGCBlocked
		{
			get
			{
				return data.Get("ps4-privacy-ugc-blocked", d: false);
			}
			set
			{
				data.Set("ps4-privacy-ugc-blocked", value);
				Refresh();
			}
		}

		public float droneResetDelay
		{
			get
			{
				return data.Get("reset-delay", 0f);
			}
			set
			{
				data.Set("reset-delay", value);
				Refresh();
			}
		}

		public void CalculateDataCompletion()
		{
			int num = 0;
			float num2 = 6f;
			if (!string.IsNullOrEmpty(fullName))
			{
				num++;
			}
			if (!string.IsNullOrEmpty(email))
			{
				num++;
			}
			if (!string.IsNullOrEmpty(country))
			{
				num++;
			}
			if (!string.IsNullOrEmpty(watchDRL))
			{
				num++;
			}
			if (!string.IsNullOrEmpty(experienceNonFPV))
			{
				num++;
			}
			if (!string.IsNullOrEmpty(experienceFPV))
			{
				num++;
			}
			Debug.Log("ProfileStateModel> Profile completion = " + (dataCompletion = (float)num / num2));
		}

		public bool ContainsInventory(IList<string> p_guids, bool p_all = true)
		{
			if (p_guids == null)
			{
				return false;
			}
			List<string> list = inventory;
			if (list == null)
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < p_guids.Count; i++)
			{
				string item = p_guids[i];
				if (list.Contains(item))
				{
					num++;
				}
			}
			if (num <= 0)
			{
				return false;
			}
			if (!p_all)
			{
				return num >= 1;
			}
			return num == p_guids.Count;
		}

		public void RegisterInventoryGUIDs(IList<string> p_list)
		{
			List<string> list = inventory;
			if (list == null || p_list == null)
			{
				return;
			}
			list.AddRange(p_list);
			list.Sort();
			if (list.Count >= 2)
			{
				for (int i = 0; i < list.Count; i++)
				{
					for (int j = i + 1; j < list.Count; j++)
					{
						if (list[i] == list[j])
						{
							list.RemoveAt(j--);
						}
					}
				}
			}
			Debug.Log("ProfileStateModel> RegisterInventoryGUIDs / Added GUIDs [" + string.Join(",", p_list) + "]\n" + string.Join("\n", list));
		}

		public void RemoveInventoryGUIDs(IList<string> p_list)
		{
			List<string> list = inventory;
			if (list == null || p_list == null)
			{
				return;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (p_list[i] == list[j])
					{
						Debug.Log("ProfileStateMode> Removing item from inventory: " + p_list[i]);
						list.RemoveAt(j--);
					}
				}
			}
			list.Sort();
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
