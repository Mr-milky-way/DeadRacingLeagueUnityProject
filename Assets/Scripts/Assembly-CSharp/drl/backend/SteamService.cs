using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.backend
{
	public class SteamService : PlatformService
	{
		[HideInInspector]
		public CGameID steamAppId;

		[HideInInspector]
		public CSteamID steamId;

		public ulong appId;

		public byte[] sessionRaw;

		public uint unixSecondsSincePurchase;

		public string datetimeSincePurchase;

		public int appBuildId;

		public string appDirectoryPath;

		public bool httpDebug;

		public Texture2D defaultAvatar;

		[SerializeField]
		private Texture2D m_avatarFull;

		[SerializeField]
		private Texture2D m_avatarThumb;

		[SerializeField]
		private List<SteamFriend> m_steam_friends;

		[SerializeField]
		private List<SteamItem> m_items;

		[SerializeField]
		private List<SteamItem> m_inventory;

		[SerializeField]
		private string m_branch_name;

		protected bool m_friends_refresh_lock;

		protected float m_friends_refresh_timeout;

		public SteamEvent OnEvent;

		public Callback<GetAuthSessionTicketResponse_t> steamAuthCallback;

		protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

		private Dictionary<string, Texture2D> m_avatar_cache;

		private Callback<MicroTxnAuthorizationResponse_t> m_transaction_watch_poll;

		private Callback<SteamInventoryResultReady_t> m_si_result_ready_cb;

		private CallResult<SteamInventoryRequestPricesResult_t> m_si_request_prices_cb;

		public bool hasSteamOverlay => SteamUtils.IsOverlayEnabled();

		public Texture2D avatarFull
		{
			get
			{
				if ((bool)m_avatarFull)
				{
					return m_avatarFull;
				}
				m_avatarFull = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
				m_avatarFull.SetPixel(1, 1, Color.black);
				m_avatarFull.name = "steam-avatar-full";
				return m_avatarFull;
			}
			set
			{
				if (!m_avatarFull)
				{
					return;
				}
				try
				{
					if ((bool)value)
					{
						m_avatarFull.Resize(value.width, value.height);
						m_avatarFull.LoadRawTextureData(value.GetRawTextureData());
					}
					else if ((bool)defaultAvatar)
					{
						m_avatarFull.Resize(defaultAvatar.width, defaultAvatar.height);
						m_avatarFull.LoadRawTextureData(defaultAvatar.GetRawTextureData());
					}
					else
					{
						m_avatarFull.Resize(1, 1);
						m_avatarFull.SetPixel(0, 0, Color.black);
					}
					m_avatarFull.Apply();
				}
				catch (Exception)
				{
					Debug.LogWarning("SteamService> Failed to set avatarFull");
				}
			}
		}

		public Texture2D avatarThumb
		{
			get
			{
				if ((bool)m_avatarThumb)
				{
					return m_avatarThumb;
				}
				m_avatarThumb = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
				m_avatarThumb.SetPixel(1, 1, Color.black);
				m_avatarThumb.name = "steam-avatar-thumb";
				return m_avatarThumb;
			}
			set
			{
				if (!m_avatarThumb)
				{
					return;
				}
				try
				{
					if ((bool)value)
					{
						m_avatarThumb.Resize(value.width, value.height);
						m_avatarThumb.LoadRawTextureData(value.GetRawTextureData());
					}
					else if ((bool)defaultAvatar)
					{
						m_avatarThumb.Resize(defaultAvatar.width, defaultAvatar.height);
						m_avatarThumb.LoadRawTextureData(defaultAvatar.GetRawTextureData());
					}
					else
					{
						m_avatarThumb.Resize(1, 1);
						m_avatarThumb.SetPixel(0, 0, Color.black);
					}
					m_avatarThumb.Apply();
				}
				catch (Exception)
				{
					Debug.LogWarning("SteamService> Failed to set avatarThumb");
				}
			}
		}

		public List<SteamFriend> steamFriends
		{
			get
			{
				if (m_steam_friends != null)
				{
					return m_steam_friends;
				}
				return m_steam_friends = new List<SteamFriend>();
			}
		}

		public List<SteamItem> items
		{
			get
			{
				if (m_items != null)
				{
					return m_items;
				}
				return m_items = new List<SteamItem>();
			}
			set
			{
				m_items = value;
			}
		}

		public List<SteamItem> inventory
		{
			get
			{
				if (m_inventory != null)
				{
					return m_inventory;
				}
				return m_inventory = new List<SteamItem>();
			}
			set
			{
				m_inventory = value;
			}
		}

		public string branchName
		{
			get
			{
				m_branch_name = "";
				if (SteamApps.GetCurrentBetaName(out m_branch_name, 128))
				{
					return m_branch_name = m_branch_name.Trim().ToLower();
				}
				return m_branch_name;
			}
		}

		public bool isSteamOffline => !SteamUser.BLoggedOn();

		public void SetAvatarFull(Texture2D p_texture)
		{
			m_avatarFull = p_texture;
		}

		public void SetAvatarThumb(Texture2D p_texture)
		{
			m_avatarThumb = p_texture;
		}

		protected new void Awake()
		{
		}

		public override void Initialize()
		{
			Debug.Log("SteamService> Initialize");
			if (DRLApp.forceOffline)
			{
				base.ready = true;
				return;
			}
			m_avatar_cache = new Dictionary<string, Texture2D>();
			_ = avatarFull;
			_ = avatarThumb;
			avatarFull = null;
			avatarThumb = null;
			StartCoroutine(HandleSteamInit());
		}

		private IEnumerator HandleSteamInit()
		{
			Debug.Log("SteamService> HandleSteamInit / Waiting for SteamManager to initialize");
			while (!SteamManager.Initialized)
			{
				yield return null;
			}
			Debug.Log("SteamService> HandleSteamInit / Steam Initialized.");
			steamAppId = new CGameID(SteamUtils.GetAppID());
			steamId = SteamUser.GetSteamID();
			id = steamId.m_SteamID.ToString();
			appId = steamAppId.m_GameID;
			languageISO = SteamApps.GetCurrentGameLanguage();
			countryISO = SteamUtils.GetIPCountry();
			unixSecondsSincePurchase = SteamApps.GetEarliestPurchaseUnixTime(SteamUtils.GetAppID());
			datetimeSincePurchase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixSecondsSincePurchase).ToLongDateString();
			appBuildId = SteamApps.GetAppBuildId();
			appDirectoryPath = "";
			SteamApps.GetAppInstallDir(SteamUtils.GetAppID(), out appDirectoryPath, 256u);
			playerName = SteamFriends.GetPersonaName();
			id = steamId.m_SteamID.ToString();
			Texture2D texture2D = GetAvatarFull(steamId);
			if ((bool)texture2D)
			{
				avatarFull = texture2D;
				playerThumbBig = avatarFull;
				UnityEngine.Object.Destroy(texture2D);
			}
			texture2D = GetAvatarThumb(steamId);
			if ((bool)texture2D)
			{
				avatarThumb = texture2D;
				playerThumbSmall = avatarThumb;
				UnityEngine.Object.Destroy(texture2D);
			}
			InitializeFriends();
			RefreshProducts();
			try
			{
				SteamController.Init();
			}
			catch (Exception ex)
			{
				Debug.LogError("SteamService> Failed to Init Steam Controller\n" + ex.Message);
			}
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
			Application.wantsToQuit += OnApplicationQuitCallback;
			steamAuthCallback = Callback<GetAuthSessionTicketResponse_t>.Create(delegate(GetAuthSessionTicketResponse_t p_result)
			{
				Debug.Log("SteamService> HandleSteamInit / OnAuthSessionTicketComplete");
				EResult eResult = p_result.m_eResult;
				if (sessionRaw == null)
				{
					sessionRaw = new byte[0];
				}
				Debug.Log($"SteamService> HandleSteamInit / res[{eResult}]");
				if (eResult == EResult.k_EResultOK)
				{
					Dispatch(SteamEventType.LoginSuccess);
					InitializeFriends();
					base.ready = true;
					LoadAppItemList(new List<int>(new int[2] { 10, 11 }));
				}
				else
				{
					Debug.LogWarning($"SteamService> HandleSteamInit / Auth Session NotOk - res[{eResult}]");
					session = "";
					Dispatch(SteamEventType.LoginFail);
				}
			});
			byte[] array = new byte[1024];
			uint pcbTicket = 0u;
			SteamUser.GetAuthSessionTicket(array, array.Length, out pcbTicket);
			Array.Resize(ref array, (int)pcbTicket);
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = new List<string>();
			for (int num = 0; num < array.Length; num++)
			{
				list.Add(array[num].ToString());
				stringBuilder.AppendFormat("{0:x2}", array[num]);
			}
			session = stringBuilder.ToString();
			sessionRaw = array;
			_ = session.Length / 3;
			if (isSteamOffline)
			{
				base.ready = true;
				DRLApp.offline = true;
				DRLApp.forceOffline = true;
			}
		}

		public void InitializeFriends()
		{
			m_steam_friends = GetFriendList();
			m_friends_refresh_timeout = 5f;
		}

		public List<SteamFriend> GetFriendList()
		{
			List<SteamFriend> list = new List<SteamFriend>();
			EFriendFlags iFriendFlags = EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagClanMember | EFriendFlags.k_EFriendFlagOnGameServer | EFriendFlags.k_EFriendFlagChatMember;
			int num = (base.ready ? SteamFriends.GetFriendCount(iFriendFlags) : 0);
			for (int i = 0; i < num; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, iFriendFlags);
				if (friendByIndex.IsValid())
				{
					SteamFriend steamFriend = new SteamFriend();
					steamFriend.steamId = friendByIndex;
					steamFriend.avatar = GetAvatarFull(friendByIndex, p_fallback: true);
					steamFriend.appId = appId;
					steamFriend.Refresh();
					list.Add(steamFriend);
				}
			}
			return list;
		}

		public SteamFriend FindFriend(ulong p_id, List<SteamFriend> p_list)
		{
			List<SteamFriend> list = ((p_list == null) ? steamFriends : p_list);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].id == p_id)
				{
					return list[i];
				}
			}
			return null;
		}

		public SteamFriend FindFriend(ulong p_id)
		{
			return FindFriend(p_id, steamFriends);
		}

		public void RefreshFriends(bool p_force = false)
		{
			if (p_force)
			{
				m_friends_refresh_timeout = 0f;
			}
			if (m_friends_refresh_timeout > 0f)
			{
				m_friends_refresh_timeout -= Time.unscaledDeltaTime;
				return;
			}
			m_friends_refresh_timeout = 5f;
			List<SteamFriend> friendList = GetFriendList();
			bool flag = false;
			if (friendList.Count != steamFriends.Count)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < friendList.Count; i++)
				{
					SteamFriend steamFriend = FindFriend(friendList[i].id);
					if (steamFriend != null && steamFriend.HasChange(friendList[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (p_force || flag)
			{
				m_steam_friends.Clear();
				m_steam_friends = friendList;
				Dispatch(SteamEventType.FriendsRefresh);
			}
		}

		public void AddFriend(ulong p_id)
		{
			CSteamID steamID = new CSteamID(p_id);
			SteamFriends.ActivateGameOverlayToUser("friendadd", steamID);
		}

		public void RemoveFriend(ulong p_id)
		{
			CSteamID steamID = new CSteamID(p_id);
			SteamFriends.ActivateGameOverlayToUser("friendremove", steamID);
		}

		public void AcceptFriend(ulong p_id)
		{
			CSteamID steamID = new CSteamID(p_id);
			SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", steamID);
		}

		public void IgnoreFriend(ulong p_id)
		{
			CSteamID steamID = new CSteamID(p_id);
			SteamFriends.ActivateGameOverlayToUser("friendrequestignore", steamID);
		}

		public bool IsFriend(ulong p_id)
		{
			for (int i = 0; i < steamFriends.Count; i++)
			{
				if (steamFriends[i].id == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsFriend(string p_id)
		{
			for (int i = 0; i < steamFriends.Count; i++)
			{
				if (steamFriends[i].id.ToString() == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public void InvitePlayer(ulong p_id, string p_args, Action<string> p_callback = null, float p_timeout = 30f)
		{
			if (!IsFriend(p_id))
			{
				Debug.LogWarning("SteamService> Tried to Invite a non-friend [" + p_id + "]");
				if (p_callback != null)
				{
					p_callback("");
				}
				return;
			}
			SteamFriends.InviteUserToGame(new CSteamID(p_id), p_args);
			Callback<GameRichPresenceJoinRequested_t> cb = null;
			Activity timeout_timer = Activity.RunOnce(delegate
			{
				if (cb != null)
				{
					cb.Dispose();
					cb = null;
					if (p_callback != null)
					{
						p_callback("");
					}
				}
			}, p_timeout);
			cb = Callback<GameRichPresenceJoinRequested_t>.Create(delegate(GameRichPresenceJoinRequested_t p_result)
			{
				if (p_result.m_steamIDFriend.m_SteamID == p_id)
				{
					timeout_timer.Stop();
					if (p_callback != null)
					{
						p_callback(p_result.m_rgchConnect);
					}
					if (cb != null)
					{
						cb.Dispose();
						cb = null;
					}
				}
			});
		}

		public void OpenFriendProfile(ulong p_id)
		{
			CSteamID steamID = new CSteamID(p_id);
			SteamFriends.ActivateGameOverlayToUser("steamid", steamID);
		}

		public void OpenFriends()
		{
			SteamFriends.ActivateGameOverlay("Friends");
		}

		private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
		{
			RefreshFriends(p_force: true);
		}

		public override void TextValidate(string p_input, Action<bool, string> p_on_result, bool p_chatMessage = false)
		{
			if (string.IsNullOrEmpty(p_input))
			{
				p_on_result?.Invoke(arg1: false, p_input);
				return;
			}
			string p_message = p_input;
			if (p_chatMessage)
			{
				string[] array = p_input.Split(new char[1] { '@' }, 2);
				if (array.Length <= 1)
				{
					p_on_result?.Invoke(arg1: false, p_input);
					return;
				}
				p_message = array[1];
			}
			DRLService.ValidateMessage(p_message, delegate(string s)
			{
				p_on_result?.Invoke(arg1: true, s);
			});
		}

		public void ConsumeItem(ulong p_id, uint p_quantity)
		{
			SteamInventory.ConsumeItem(out var _, new SteamItemInstanceID_t
			{
				m_SteamItemInstanceID = p_id
			}, p_quantity);
		}

		public void ConsumeItem(string p_id, uint p_quantity)
		{
			ulong result = 0uL;
			if (ulong.TryParse(p_id, out result))
			{
				ConsumeItem(result, p_quantity);
			}
		}

		public void ClearLicenseItems()
		{
			Debug.Log("SteamService> Clear Licenses");
			LoadUserItemList(delegate(SteamItem[] p_list)
			{
				foreach (SteamItem steamItem in p_list)
				{
					if (!(steamItem.itemdefid != "10") || !(steamItem.itemdefid != "11"))
					{
						Debug.Log("SteamService> Consuming License - id[" + steamItem.itemid + "] defid[" + steamItem.itemdefid + "]");
						ConsumeItem(steamItem.itemid, 1u);
					}
				}
			});
		}

		public void LoadUserItemList(Action<SteamItem[]> p_callback = null)
		{
			SteamInventory.GetAllItems(out var inventory_result);
			Callback<SteamInventoryResultReady_t>.Create(delegate(SteamInventoryResultReady_t p_result)
			{
				if (p_result.m_handle.m_SteamInventoryResult == inventory_result.m_SteamInventoryResult)
				{
					string text = "";
					SteamItem[] array = new SteamItem[0];
					Debug.Log("SteamService> LoadUserItemList - result[" + p_result.m_result.ToString() + "]");
					if (p_result.m_result == EResult.k_EResultOK)
					{
						uint punOutItemsArraySize = 0u;
						SteamInventory.GetResultItems(inventory_result, null, ref punOutItemsArraySize);
						array = new SteamItem[punOutItemsArraySize];
						text = text + "SteamService> LoadUserItemList - count[" + punOutItemsArraySize + "]\n";
						SteamItemDetails_t[] array2 = new SteamItemDetails_t[punOutItemsArraySize];
						SteamInventory.GetResultItems(inventory_result, array2, ref punOutItemsArraySize);
						for (int i = 0; i < array2.Length; i++)
						{
							SteamItemDetails_t steamItemDetails_t = array2[i];
							SteamItemDef_t iDefinition = steamItemDetails_t.m_iDefinition;
							SteamItem steamItem = GetSteamItem(iDefinition);
							if (steamItem != null)
							{
								steamItem.itemid = steamItemDetails_t.m_itemId.m_SteamItemInstanceID.ToString();
								steamItem.quantity = steamItemDetails_t.m_unQuantity;
								steamItem.flags = steamItemDetails_t.m_unFlags;
								text = text + "\n[" + i + "] name[" + steamItem.name + "] defid[" + steamItem.itemdefid + "] id[" + steamItem.itemid + "] qty[" + steamItem.quantity + "]";
							}
							else
							{
								text = text + "\n[" + i + "] invalid";
							}
							array[i] = steamItem;
						}
						Debug.Log(text);
						inventory = new List<SteamItem>(array);
					}
					if (p_callback != null)
					{
						p_callback(array);
					}
				}
			});
		}

		public void LoadAppItemList(List<int> p_item_ids, Action<SteamItem[]> p_callback = null)
		{
			Callback<SteamInventoryResultReady_t> cb = null;
			cb = Callback<SteamInventoryResultReady_t>.Create(delegate(SteamInventoryResultReady_t p_result)
			{
				Debug.Log("SteamService> LoadItemList - result[" + p_result.m_result.ToString() + "]");
				if (p_result.m_result == EResult.k_EResultOK)
				{
					List<int> list = p_item_ids;
					uint punItemDefIDsArraySize = 0u;
					SteamInventory.GetItemDefinitionIDs(null, ref punItemDefIDsArraySize);
					SteamItem[] array = new SteamItem[punItemDefIDsArraySize];
					int num = Mathf.Min(list.Count, (int)punItemDefIDsArraySize);
					string text = "SteamService> LoadItemList - ids[" + list.Count + "] count[" + punItemDefIDsArraySize + "]\n";
					for (int i = 0; i < num; i++)
					{
						int p_id = list[i];
						SteamItem steamItem = (array[i] = GetSteamItem(p_id));
						text = text + "\nid[" + ((steamItem == null) ? "" : steamItem.itemdefid) + "] keys[" + ((steamItem == null) ? "" : steamItem.properties) + "]";
					}
					items = new List<SteamItem>(array);
					if (p_callback != null)
					{
						p_callback(array);
					}
				}
				cb.Dispose();
			});
			SteamInventory.LoadItemDefinitions();
		}

		protected SteamItem GetSteamItem(SteamItemDef_t p_item)
		{
			SteamItem steamItem = new SteamItem();
			uint punValueBufferSizeOut = 2048u;
			string pchValueBuffer = null;
			SteamInventory.GetItemDefinitionProperty(p_item, null, out pchValueBuffer, ref punValueBufferSizeOut);
			if (punValueBufferSizeOut == 0)
			{
				return null;
			}
			steamItem.properties = pchValueBuffer;
			string[] array = pchValueBuffer.Split(',');
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string pchValueBuffer2 = "";
					uint punValueBufferSizeOut2 = 2048u;
					SteamInventory.GetItemDefinitionProperty(p_item, text, out pchValueBuffer2, ref punValueBufferSizeOut2);
					int result = 0;
					int.TryParse(pchValueBuffer2, out result);
					bool result2 = false;
					if (!bool.TryParse(pchValueBuffer2, out result2))
					{
						result2 = pchValueBuffer2 == "1";
					}
					switch (text)
					{
					case "name":
						steamItem.name = pchValueBuffer2;
						break;
					case "appid":
						steamItem.appid = pchValueBuffer2;
						break;
					case "Timestamp":
						steamItem.Timestamp = pchValueBuffer2;
						break;
					case "modified":
						steamItem.modified = pchValueBuffer2;
						break;
					case "itemdefid":
						steamItem.itemdefid = pchValueBuffer2;
						break;
					case "itemid":
						steamItem.itemid = pchValueBuffer2;
						break;
					case "guid":
						steamItem.guid = pchValueBuffer2;
						break;
					case "type":
						steamItem.type = pchValueBuffer2;
						break;
					case "display_type":
						steamItem.display_type = pchValueBuffer2;
						break;
					case "bundle":
						steamItem.bundle = pchValueBuffer2;
						break;
					case "name_color":
						steamItem.name_color = pchValueBuffer2;
						break;
					case "background_color":
						steamItem.background_color = pchValueBuffer2;
						break;
					case "item_slot":
						steamItem.item_slot = pchValueBuffer2;
						break;
					case "item_quality":
						steamItem.item_quality = pchValueBuffer2;
						break;
					case "icon_url":
						steamItem.icon_url = pchValueBuffer2;
						break;
					case "icon_url_large":
						steamItem.icon_url_large = pchValueBuffer2;
						break;
					case "quantity":
						steamItem.quantity = result;
						break;
					case "description":
						steamItem.description = pchValueBuffer2;
						break;
					case "hash":
						steamItem.hash = pchValueBuffer2;
						break;
					case "tradable":
						steamItem.tradable = result2;
						break;
					case "marketable":
						steamItem.marketable = result2;
						break;
					case "commodity":
						steamItem.commodity = result2;
						break;
					case "store_hidden":
						steamItem.store_hidden = result2;
						break;
					case "price":
						steamItem.price = pchValueBuffer2;
						break;
					case "price_category":
						steamItem.price_category = pchValueBuffer2;
						break;
					case "drop_interval":
						steamItem.drop_interval = result;
						break;
					case "drop_max_per_window":
						steamItem.drop_max_per_window = result;
						break;
					case "workshopid":
						steamItem.workshopid = pchValueBuffer2;
						break;
					}
				}
			}
			string text2 = ((!string.IsNullOrEmpty(steamItem.price)) ? steamItem.price : (string.IsNullOrEmpty(steamItem.price_category) ? "" : steamItem.price_category));
			text2 = text2.ToLower();
			steamItem.priceVLV = -1f;
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = (text2 + ";").Replace(";;", ";");
				int num = text2.IndexOf("vlv");
				if (num >= 0)
				{
					int num2 = text2.IndexOf(";", num);
					string s = text2.Substring(num, num2 - num).Replace("vlv", "").Trim();
					int result3 = 0;
					if (int.TryParse(s, out result3))
					{
						result3 = ((result3 % 10 == 0) ? (result3 - 1) : result3);
					}
					steamItem.priceVLV = (float)result3 / 100f;
				}
			}
			return steamItem;
		}

		protected SteamItem GetSteamItem(int p_id)
		{
			return GetSteamItem(new SteamItemDef_t
			{
				m_SteamItemDef = p_id
			});
		}

		protected void OnAuthSessionTicketComplete(GetAuthSessionTicketResponse_t p_result)
		{
			Debug.Log("SteamService> OnAuthSessionTicketComplete called");
			EResult eResult = p_result.m_eResult;
			if (sessionRaw == null)
			{
				sessionRaw = new byte[0];
			}
			Debug.Log($"SteamService> OnAuthSessionTicketComplete / id[{p_result.m_hAuthTicket.m_HAuthTicket}] session-len[{sessionRaw.Length}] res[{eResult}]");
			if (eResult == EResult.k_EResultOK)
			{
				Dispatch(SteamEventType.LoginSuccess);
				LoadAppItemList(new List<int>(new int[2] { 10, 11 }));
			}
			else
			{
				Debug.LogWarning("SteamService> OnAuthSessionTicketComplete - " + eResult);
				session = "";
				Dispatch(SteamEventType.LoginFail);
			}
		}

		public Texture2D GetSmallAvatar(CSteamID p_user)
		{
			return GetAvatar(p_user, 0);
		}

		public Texture2D GetMediumAvatar(CSteamID p_user)
		{
			return GetAvatar(p_user, 1);
		}

		public Texture2D GetLargeAvatar(CSteamID p_user)
		{
			return GetAvatar(p_user, 2);
		}

		public Texture2D GetAvatarFull(CSteamID p_user, bool p_fallback = false)
		{
			Texture2D texture2D = null;
			string key = p_user.m_SteamID.ToString();
			if (m_avatar_cache.ContainsKey(key))
			{
				return m_avatar_cache[key];
			}
			texture2D = GetLargeAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			texture2D = GetMediumAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			texture2D = GetSmallAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			if (p_fallback && (bool)defaultAvatar)
			{
				texture2D = new Texture2D(defaultAvatar.width, defaultAvatar.height, TextureFormat.RGBA32, mipChain: false);
				texture2D.SetPixels(defaultAvatar.GetPixels());
				texture2D.Apply();
				m_avatar_cache[key] = texture2D;
			}
			return texture2D;
		}

		public Texture2D GetAvatarThumb(CSteamID p_user, bool p_fallback = false)
		{
			Texture2D texture2D = null;
			texture2D = GetSmallAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			texture2D = GetMediumAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			texture2D = GetLargeAvatar(p_user);
			if ((bool)texture2D)
			{
				return texture2D;
			}
			if (p_fallback && (bool)defaultAvatar)
			{
				texture2D = new Texture2D(defaultAvatar.width, defaultAvatar.height, TextureFormat.RGBA32, mipChain: false);
				texture2D.SetPixels(defaultAvatar.GetPixels());
				texture2D.Apply();
			}
			return texture2D;
		}

		public Texture2D GetDefaultAvatar()
		{
			Texture2D texture2D = null;
			if ((bool)defaultAvatar)
			{
				texture2D = new Texture2D(defaultAvatar.width, defaultAvatar.height, TextureFormat.RGBA32, mipChain: false);
				texture2D.SetPixels(defaultAvatar.GetPixels());
			}
			if (!texture2D)
			{
				texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
				texture2D.SetPixel(0, 0, Color.black);
			}
			return texture2D;
		}

		protected Texture2D GetAvatar(CSteamID p_user, int p_size)
		{
			int iImage = 0;
			switch (p_size)
			{
			case 0:
				iImage = SteamFriends.GetSmallFriendAvatar(p_user);
				break;
			case 1:
				iImage = SteamFriends.GetMediumFriendAvatar(p_user);
				break;
			case 2:
				iImage = SteamFriends.GetLargeFriendAvatar(p_user);
				break;
			}
			string key = p_user.m_SteamID + "-" + iImage;
			if (m_avatar_cache.ContainsKey(key))
			{
				return m_avatar_cache[key];
			}
			uint pnWidth = 0u;
			uint pnHeight = 0u;
			Texture2D texture2D = null;
			byte[] array = null;
			if (SteamUtils.GetImageSize(iImage, out pnWidth, out pnHeight))
			{
				array = new byte[pnWidth * pnHeight * 4];
				if (!SteamUtils.GetImageRGBA(iImage, array, array.Length))
				{
					array = null;
				}
				if (array != null)
				{
					for (uint num = 0u; num < pnHeight / 2; num++)
					{
						for (uint num2 = 0u; num2 < pnWidth; num2++)
						{
							uint num3 = (num * pnWidth + num2) * 4;
							uint num4 = ((pnHeight - 1 - num) * pnWidth + num2) * 4;
							for (uint num5 = 0u; num5 < 4; num5++)
							{
								byte b = array[num3 + num5];
								byte b2 = array[num4 + num5];
								array[num3 + num5] = b2;
								array[num4 + num5] = b;
							}
						}
					}
					texture2D = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, mipChain: false);
					texture2D.LoadRawTextureData(array);
					texture2D.Apply();
					m_avatar_cache[key] = texture2D;
				}
			}
			return texture2D;
		}

		public bool HttpRequest<T>(string p_method, string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, string> p_headers, Dictionary<string, object> p_data)
		{
			if (string.IsNullOrEmpty(p_url))
			{
				return false;
			}
			HTTPRequestHandle rhdl = default(HTTPRequestHandle);
			Dictionary<string, object> dictionary = ((p_data == null) ? new Dictionary<string, object>() : p_data);
			Dictionary<string, string> dictionary2 = ((p_headers == null) ? new Dictionary<string, string>() : p_headers);
			EHTTPMethod eHTTPRequestMethod = EHTTPMethod.k_EHTTPMethodGET;
			bool flag = false;
			ulong request_ctx = (ulong)UnityEngine.Random.Range(0, int.MaxValue);
			ulong request_id = 0uL;
			switch (p_method.ToUpper())
			{
			case "GET":
				eHTTPRequestMethod = EHTTPMethod.k_EHTTPMethodGET;
				break;
			case "POST":
				eHTTPRequestMethod = EHTTPMethod.k_EHTTPMethodPOST;
				break;
			case "PUT":
				eHTTPRequestMethod = EHTTPMethod.k_EHTTPMethodPUT;
				break;
			}
			if (!dictionary2.ContainsKey("Content-Type"))
			{
				dictionary2["Content-Type"] = "application/x-www-form-urlencoded";
			}
			string text = "SteamService> " + p_method.ToUpper() + " - url[" + p_url + "] context[" + request_ctx + "]\n";
			rhdl = SteamHTTP.CreateHTTPRequest(eHTTPRequestMethod, p_url);
			SteamHTTP.SetHTTPRequestContextValue(rhdl, request_ctx);
			text += "=== HEADERS ===\n";
			foreach (KeyValuePair<string, string> item in dictionary2)
			{
				string key = item.Key;
				string value = item.Value;
				value = ((value == null) ? "" : value.ToString());
				bool flag2 = SteamHTTP.SetHTTPRequestHeaderValue(rhdl, key, value);
				text = text + key + ": " + value + " [" + flag2 + "]\n";
			}
			text += "\n";
			text += "=== DATA ===\n";
			foreach (KeyValuePair<string, object> item2 in dictionary)
			{
				string key2 = item2.Key;
				object value2 = item2.Value;
				string text2 = ((value2 == null) ? "" : value2.ToString());
				bool flag3 = SteamHTTP.SetHTTPRequestGetOrPostParameter(rhdl, key2, text2);
				text = text + key2 + ": " + text2 + " [" + flag3 + "]\n";
			}
			if (p_callback != null)
			{
				p_callback(default(T), 0f, 0);
			}
			CallResult<HTTPRequestCompleted_t> callResult = CallResult<HTTPRequestCompleted_t>.Create(delegate(HTTPRequestCompleted_t p_result, bool p_failure)
			{
				if (p_failure)
				{
					Debug.LogError("SteamService> " + p_method.ToUpper() + " Failed - context[" + request_ctx + "]");
				}
				else if (p_result.m_ulContextValue == request_ctx)
				{
					bool bRequestSuccessful = p_result.m_bRequestSuccessful;
					EHTTPStatusCode eStatusCode = p_result.m_eStatusCode;
					uint unBodySize = 0u;
					byte[] array = null;
					SteamHTTP.GetHTTPResponseBodySize(rhdl, out unBodySize);
					array = new byte[unBodySize];
					SteamHTTP.GetHTTPResponseBodyData(rhdl, array, unBodySize);
					T val = default(T);
					if (typeof(T) == typeof(string))
					{
						val = (T)(object)Encoding.UTF8.GetString(array);
					}
					if (typeof(T) == typeof(byte[]))
					{
						val = (T)(object)array;
					}
					string[] obj = new string[13]
					{
						"SteamService> ",
						p_method.ToUpper(),
						" - code[",
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null
					};
					int num = (int)eStatusCode;
					obj[3] = num.ToString();
					obj[4] = "] context[";
					obj[5] = request_ctx.ToString();
					obj[6] = "] id[";
					obj[7] = request_id.ToString();
					obj[8] = "] success[";
					obj[9] = bRequestSuccessful.ToString();
					obj[10] = "] length[";
					obj[11] = unBodySize.ToString();
					obj[12] = "]\n=== DATA ===\n";
					string text3 = string.Concat(obj);
					if (typeof(T) == typeof(string))
					{
						string text4 = text3;
						T val2 = val;
						text3 = text4 + val2;
					}
					if (typeof(T) == typeof(byte[]))
					{
						text3 += "[bytes]";
					}
					if (httpDebug)
					{
						Debug.Log(text3);
					}
					if (p_callback != null)
					{
						p_callback(val, 1f, (int)eStatusCode);
					}
					SteamHTTP.ReleaseHTTPRequest(rhdl);
				}
			});
			flag = SteamHTTP.SendHTTPRequest(rhdl, out var pCallHandle);
			request_id = pCallHandle.m_SteamAPICall;
			callResult.Set(pCallHandle);
			text = text + "Send - flag[" + flag + "] id[" + pCallHandle.m_SteamAPICall + "]";
			if (httpDebug)
			{
				Debug.Log(text);
			}
			return flag;
		}

		public bool HttpRequest<T>(string p_method, string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, object> p_data)
		{
			return HttpRequest(p_method, p_url, p_callback, null, p_data);
		}

		public bool HttpRequest<T>(string p_method, string p_url, SteamRequestHandler<T> p_callback)
		{
			return HttpRequest(p_method, p_url, p_callback, null, null);
		}

		public bool HttpGet<T>(string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, string> p_headers, Dictionary<string, object> p_data)
		{
			return HttpRequest("GET", p_url, p_callback, p_headers, p_data);
		}

		public bool HttpGet<T>(string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, object> p_data)
		{
			return HttpRequest("GET", p_url, p_callback, null, p_data);
		}

		public bool HttpGet<T>(string p_url, SteamRequestHandler<T> p_callback)
		{
			return HttpRequest("GET", p_url, p_callback, null, null);
		}

		public bool HttpPost<T>(string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, string> p_headers, Dictionary<string, object> p_data)
		{
			return HttpRequest("POST", p_url, p_callback, p_headers, p_data);
		}

		public bool HttpPost<T>(string p_url, SteamRequestHandler<T> p_callback, Dictionary<string, object> p_data)
		{
			return HttpRequest("POST", p_url, p_callback, null, p_data);
		}

		public bool HttpPost<T>(string p_url, SteamRequestHandler<T> p_callback)
		{
			return HttpRequest("POST", p_url, p_callback, null, null);
		}

		public void WatchTransactionEvent(Action<bool> p_callback)
		{
			Debug.Log("SteamService> WatchTransactionEvent / Start");
			if (m_transaction_watch_poll != null)
			{
				m_transaction_watch_poll.Dispose();
				m_transaction_watch_poll = null;
			}
			m_transaction_watch_poll = Callback<MicroTxnAuthorizationResponse_t>.Create(delegate(MicroTxnAuthorizationResponse_t p_result)
			{
				ulong num = p_result.m_unAppID;
				ulong ulOrderID = p_result.m_ulOrderID;
				bool flag = p_result.m_bAuthorized == 1;
				Debug.Log($"SteamService> WatchTransactionEvent / Complete - order-id[{ulOrderID}] success[{flag}] appid[{num}] order-id[{ulOrderID}]");
				if (num != appId)
				{
					Debug.LogWarning("SteamService> WatchTransactionEvent / Error AppID not matched!");
					flag = false;
				}
				if (p_callback != null)
				{
					p_callback(flag);
				}
				if (m_transaction_watch_poll != null)
				{
					m_transaction_watch_poll.Dispose();
					m_transaction_watch_poll = null;
				}
			});
		}

		public void OpenOverlay(string p_dialog)
		{
			SteamFriends.ActivateGameOverlay(p_dialog);
		}

		public void Unload()
		{
			try
			{
				SteamController.Shutdown();
			}
			catch (InvalidOperationException ex)
			{
				Debug.LogError("SteamService> Close / SteamController.Shutdown Error:\n" + ex.Message);
			}
		}

		[ContextMenu("Close")]
		public void Close()
		{
		}

		public override void UpdateAchievement(string p_id, float p_progress, Action p_oncomplete)
		{
			if (!SteamUserStats.RequestCurrentStats())
			{
				Debug.LogWarning("SteamService> UpdateAchievement / RequestCurrentStats [" + p_id + "] failed");
				return;
			}
			Callback<UserStatsReceived_t>.Create(delegate(UserStatsReceived_t p_result)
			{
				Debug.Log($"SteamService> UpdateAchievement / UserStatsReceived [{p_result.m_eResult}]");
				if (p_result.m_eResult == EResult.k_EResultOK)
				{
					ApplyAchievement(p_id, p_progress, p_oncomplete);
				}
			});
		}

		protected void ApplyAchievement(string p_id, float p_progress, Action p_oncomplete)
		{
			if (p_progress < 1f)
			{
				if (!SteamUserStats.IndicateAchievementProgress(p_id, (uint)Mathf.Lerp(1f, 99f, p_progress), 100u))
				{
					Debug.LogWarning("SteamService> ApplyAchievement / IndicateAchievementProgress [" + p_id + "] failed");
				}
				else if (p_oncomplete != null)
				{
					p_oncomplete();
				}
				return;
			}
			if (!SteamUserStats.SetAchievement(p_id))
			{
				Debug.LogWarning("SteamService> ApplyAchievement / SetAchievement [" + p_id + "] failed");
				return;
			}
			if (!SteamUserStats.StoreStats())
			{
				Debug.LogWarning("SteamService> ApplyAchievement / StoreStats [" + p_id + "] failed");
				return;
			}
			Callback<UserStatsStored_t>.Create(delegate(UserStatsStored_t p_result)
			{
				Debug.Log($"SteamService> ApplyAchievement / UserStatsStored [{p_result.m_eResult}]");
				if (p_result.m_eResult == EResult.k_EResultOK && p_oncomplete != null)
				{
					p_oncomplete();
				}
			});
		}

		protected void Dispatch(SteamEventType p_type)
		{
			SteamEventData steamEventData = new SteamEventData();
			steamEventData.target = this;
			steamEventData.type = p_type;
			if (OnEvent != null)
			{
				OnEvent.Invoke(steamEventData);
			}
		}

		protected bool OnApplicationQuitCallback()
		{
			Application.wantsToQuit -= OnApplicationQuitCallback;
			Unload();
			return true;
		}

		public override void RefreshProducts(Action p_on_complete = null)
		{
			if (m_si_result_ready_cb != null)
			{
				m_si_result_ready_cb.Dispose();
				m_si_result_ready_cb = null;
			}
			if (m_si_request_prices_cb != null)
			{
				m_si_request_prices_cb.Dispose();
				m_si_request_prices_cb = null;
			}
			Action on_store_req_complete = delegate
			{
				uint punItemDefIDsArraySize = 0u;
				SteamInventory.GetItemDefinitionIDs(null, ref punItemDefIDsArraySize);
				SteamItemDef_t[] array = new SteamItemDef_t[punItemDefIDsArraySize];
				SteamInventory.GetItemDefinitionIDs(array, ref punItemDefIDsArraySize);
				Debug.Log($"SteamService> RefreshProducts / Store Request Complete with {punItemDefIDsArraySize} Products");
				_ = base.currencySymbol;
				base.products.Clear();
				for (int i = 0; i < array.Length; i++)
				{
					SteamItemDef_t steamItemDef_t = array[i];
					SteamItem steamItem = GetSteamItem(steamItemDef_t);
					PlatformProduct platformProduct = new PlatformProduct
					{
						id = steamItemDef_t.m_SteamItemDef.ToString()
					};
					ulong pCurrentPrice = 0uL;
					ulong pBasePrice = 0uL;
					bool itemPrice = SteamInventory.GetItemPrice(steamItemDef_t, out pCurrentPrice, out pBasePrice);
					platformProduct.price = (itemPrice ? ((float)pCurrentPrice / 100f) : steamItem.priceVLV);
					platformProduct.priceBase = (itemPrice ? ((float)pBasePrice / 100f) : steamItem.priceVLV);
					platformProduct.meta = steamItem;
					base.products.Add(platformProduct);
				}
				if (p_on_complete != null)
				{
					Activity.RunOnce(p_on_complete);
				}
			};
			m_si_result_ready_cb = Callback<SteamInventoryResultReady_t>.Create(delegate(SteamInventoryResultReady_t p_result)
			{
				m_si_result_ready_cb.Dispose();
				m_si_result_ready_cb = null;
				Debug.Log($"SteamService> RefreshProducts / SteamInventoryResultReady - result[{p_result.m_result}]");
				if (p_result.m_result == EResult.k_EResultOK)
				{
					SteamAPICall_t steamAPICall_t = SteamInventory.RequestPrices();
					bool flag2 = steamAPICall_t != SteamAPICall_t.Invalid;
					Debug.Log($"SteamService> RefreshProducts / Requesting Prices - result[{steamAPICall_t.m_SteamAPICall}] valid[{flag2}]");
					if (flag2)
					{
						m_si_request_prices_cb = CallResult<SteamInventoryRequestPricesResult_t>.Create(delegate(SteamInventoryRequestPricesResult_t p_result_prices, bool p_failed_prices)
						{
							m_si_request_prices_cb.Dispose();
							m_si_request_prices_cb = null;
							currencyISO = p_result_prices.m_rgchCurrency;
							Debug.Log($"SteamService> RefreshProducts / SteamInventoryRequestPricesResult - result[{p_result_prices.m_result}] currency[{currencyISO}] failed[{p_failed_prices}]");
							if (p_result_prices.m_result == EResult.k_EResultOK)
							{
								on_store_req_complete();
							}
						});
						m_si_request_prices_cb.Set(steamAPICall_t);
					}
				}
			});
			bool flag = SteamInventory.LoadItemDefinitions();
			currencyISO = "USD";
			Debug.Log($"SteamService> RefreshProducts / Requesting Products - result[{flag}]");
		}

		public override void PurchaseProduct(string p_id, Action<bool, string> p_on_result)
		{
			DRLService component = GetComponent<DRLService>();
			if (!component)
			{
				if (p_on_result != null)
				{
					p_on_result(arg1: false, "Backend Not Available!");
				}
				return;
			}
			Debug.Log("SteamService> PurchaseProduct / Transaction Start - product-id[" + p_id + "]");
			component.Transaction(p_id, 1, delegate(DRLTransactionResult p_transaction_result)
			{
				Debug.Log("SteamService> PurchaseProduct / Transaction Complete - result[" + p_transaction_result.result + "]");
				bool arg = p_transaction_result.result.ToLower() == "ok";
				if (p_on_result != null)
				{
					p_on_result(arg, p_transaction_result.result);
				}
			});
		}
	}
}
