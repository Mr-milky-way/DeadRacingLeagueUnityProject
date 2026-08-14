using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using drl.game;

namespace drl.backend
{
	public class PlatformService : MonoBehaviour
	{
		private bool m_ready;

		public bool active;

		public string id;

		public string playerName;

		public string session;

		public bool offline;

		public Texture2D playerThumbBig;

		public Texture2D playerThumbSmall;

		public string languageISO;

		public string countryISO;

		public string currencyISO;

		private static Dictionary<string, string> m_currency_symbols;

		public Texture2D defaultThumb;

		public List<PlatformServiceFlagType> flags;

		public List<string> blockedUserIds;

		public List<string> blockedUserIdsSession;

		[SerializeField]
		private List<GameFriendData> m_friends;

		[SerializeField]
		private List<PlatformGameInvite> m_invites;

		[SerializeField]
		private List<PlatformProduct> m_products;

		public bool ready
		{
			get
			{
				if (!m_ready)
				{
					return offline;
				}
				return true;
			}
			set
			{
				m_ready = value;
			}
		}

		public string currencySymbol
		{
			get
			{
				RefreshCurrencySymbolTable();
				string key = (string.IsNullOrEmpty(currencyISO) ? "us" : currencyISO.ToLower());
				if (!m_currency_symbols.ContainsKey(key))
				{
					return "$";
				}
				return m_currency_symbols[key];
			}
		}

		public List<GameFriendData> friends
		{
			get
			{
				if (m_friends != null)
				{
					return m_friends;
				}
				return m_friends = new List<GameFriendData>();
			}
		}

		public List<PlatformGameInvite> invites
		{
			get
			{
				if (m_invites != null)
				{
					return m_invites;
				}
				return m_invites = new List<PlatformGameInvite>();
			}
		}

		public List<PlatformProduct> products
		{
			get
			{
				if (m_products != null)
				{
					return m_products;
				}
				return m_products = new List<PlatformProduct>();
			}
		}

		public bool hasInvite
		{
			get
			{
				if (m_invites != null)
				{
					return m_invites.Count > 0;
				}
				return false;
			}
		}

		protected void RefreshCurrencySymbolTable()
		{
			if (m_currency_symbols != null)
			{
				return;
			}
			if (m_currency_symbols == null)
			{
				m_currency_symbols = new Dictionary<string, string>();
			}
			m_currency_symbols.Clear();
			CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			string message = "PlatformService> RefreshCurrencySymbolTable";
			foreach (CultureInfo cultureInfo in cultures)
			{
				if (!cultureInfo.IsNeutralCulture)
				{
					RegionInfo regionInfo = null;
					try
					{
						regionInfo = new RegionInfo(cultureInfo.Name);
					}
					catch
					{
					}
					if (regionInfo != null)
					{
						regionInfo.ThreeLetterISORegionName.ToLower();
						string key = regionInfo.ISOCurrencySymbol.ToLower();
						string text = regionInfo.CurrencySymbol;
						m_currency_symbols[key] = (string.IsNullOrEmpty(text) ? "$" : text);
					}
				}
			}
			Debug.Log(message);
		}

		public virtual void Awake()
		{
			SetFlag(PlatformServiceFlagType.XBoxUGCBlocked, p_value: false);
			SetFlag(PlatformServiceFlagType.XBoxMultiplayerAllowed, p_value: true);
			SetFlag(PlatformServiceFlagType.XBoxCrossPlayAllowed, p_value: true);
			SetFlag(PlatformServiceFlagType.XBoxCommunicationAllowed, p_value: true);
		}

		public virtual void Initialize()
		{
		}

		protected virtual void Refresh()
		{
		}

		public GameFriendData AddFriend(string p_platform_id)
		{
			GameFriendData gameFriendData = new GameFriendData();
			gameFriendData.platformId = p_platform_id;
			friends.Add(gameFriendData);
			return gameFriendData;
		}

		public virtual void RefreshFriends(Action p_oncomplete = null)
		{
			p_oncomplete?.Invoke();
		}

		public bool ContainsFriend(string p_id)
		{
			for (int i = 0; i < friends.Count; i++)
			{
				if (friends[i].platformId == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public PlatformGameInvite GetInvite(string p_from)
		{
			return invites.Find((PlatformGameInvite it) => it.from == p_from);
		}

		public PlatformGameInvite GetInvite(int p_index)
		{
			if (p_index >= 0)
			{
				if (p_index < invites.Count)
				{
					return invites[p_index];
				}
				return null;
			}
			return null;
		}

		public PlatformGameInvite AddInvite(string p_from, string p_to, string p_region, string p_room, string p_args = "")
		{
			PlatformGameInvite platformGameInvite = GetInvite(p_from);
			if (platformGameInvite == null)
			{
				platformGameInvite = new PlatformGameInvite();
				invites.Add(platformGameInvite);
			}
			platformGameInvite.from = p_from;
			platformGameInvite.to = p_to;
			platformGameInvite.region = p_region;
			platformGameInvite.room = p_room;
			platformGameInvite.args = p_args;
			Debug.Log("PlatformService> AddInvite / from[" + p_from + "] to[" + p_to + "] region[" + p_region + "] room[" + p_room + "]");
			return platformGameInvite;
		}

		public void ClearInvites()
		{
			invites.Clear();
		}

		public void SetFlag(PlatformServiceFlagType p_flag, bool p_value)
		{
			if (p_flag != PlatformServiceFlagType.None)
			{
				if (p_value && !flags.Contains(p_flag))
				{
					flags.Add(p_flag);
				}
				if (!p_value && flags.Contains(p_flag))
				{
					flags.Remove(p_flag);
				}
			}
		}

		public bool ContainsFlag(PlatformServiceFlagType p_flag)
		{
			return flags.Contains(p_flag);
		}

		public virtual void CheckPlatformMultiplayerPrivilege(Action p_on_result)
		{
			p_on_result?.Invoke();
		}

		public virtual void CheckPlatformUGCPrivilege(Action p_on_result)
		{
			p_on_result?.Invoke();
		}

		public virtual void CheckPlatformCommunicationPrivilege(Action p_on_result)
		{
			p_on_result?.Invoke();
		}

		public virtual void TextValidate(string p_input, Action<bool, string> p_on_result, bool p_chatMessage = false)
		{
			p_on_result?.Invoke(arg1: true, p_input);
		}

		public virtual void RefreshFlags(Action p_oncomplete = null)
		{
			p_oncomplete?.Invoke();
		}

		public virtual void IsUserCommunicationBlocked(string p_id, Action<bool> p_on_result)
		{
			if (p_on_result != null)
			{
				if (blockedUserIds.Contains(p_id))
				{
					p_on_result(obj: true);
				}
				else if (blockedUserIdsSession.Contains(p_id))
				{
					p_on_result(obj: true);
				}
				else
				{
					p_on_result(obj: false);
				}
			}
		}

		public void SetUserSessionBlocked(string p_id, bool p_flag)
		{
			List<string> list = blockedUserIdsSession;
			if (p_flag && !list.Contains(p_id))
			{
				list.Add(p_id);
			}
			if (!p_flag && list.Contains(p_id))
			{
				list.Remove(p_id);
			}
		}

		public bool GetUserSessionBlocked(string p_id)
		{
			if (blockedUserIdsSession == null || blockedUserIdsSession.Count == 0)
			{
				return false;
			}
			return blockedUserIdsSession.Contains(p_id);
		}

		public void ClearSessionBlockedUsers()
		{
			blockedUserIdsSession.Clear();
		}

		public virtual void UpdateAchievement(string p_id, float p_progress, Action p_oncomplete)
		{
			p_oncomplete?.Invoke();
		}

		public virtual void RefreshProducts(Action p_on_complete = null)
		{
			products.Clear();
			p_on_complete?.Invoke();
		}

		public virtual void PurchaseProduct(string p_id, Action<bool, string> p_on_result)
		{
			p_on_result?.Invoke(arg1: true, "");
		}

		public virtual PlatformProduct GetProductById(string p_id)
		{
			return products.Find((PlatformProduct it) => it.id == p_id);
		}

		public virtual string GetProductPriceString(string p_id)
		{
			PlatformProduct productById = GetProductById(p_id);
			if (productById != null)
			{
				return currencySymbol + " " + productById.price.ToString("0.00");
			}
			return "";
		}
	}
}
