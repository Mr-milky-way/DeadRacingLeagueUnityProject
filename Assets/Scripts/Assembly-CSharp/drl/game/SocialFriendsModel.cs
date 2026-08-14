using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class SocialFriendsModel : Model<DRLApp>
	{
		[SerializeField]
		private GameInviteData m_invite;

		public List<GameFriendData> list;

		protected Activity m_refresh_delay;

		private Activity m_friend_load_loop;

		public ServiceModel service => base.app.model.service;

		public SocialModel parent => AssertParent<SocialModel>("parent");

		public GameInviteData invite
		{
			get
			{
				if (m_invite == null)
				{
					return null;
				}
				if (string.IsNullOrEmpty(m_invite.guid))
				{
					return null;
				}
				return m_invite;
			}
			set
			{
				m_invite = value;
			}
		}

		protected void Start()
		{
			(service.platform as SteamService).OnEvent.AddListener(OnFriendRefresh);
		}

		public bool IsFriend(string p_id)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].platformId == p_id)
				{
					return true;
				}
			}
			return false;
		}

		public GameFriendData Get(string p_id)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].platformId == p_id)
				{
					return list[i];
				}
			}
			return null;
		}

		public void Add(string p_id)
		{
			SteamService steamService = service.platform as SteamService;
			ulong result = 0uL;
			if ((bool)steamService && ulong.TryParse(p_id, out result))
			{
				steamService.AddFriend(result);
			}
		}

		public void Remove(string p_id)
		{
			SteamService steamService = service.platform as SteamService;
			ulong result = 0uL;
			if ((bool)steamService && ulong.TryParse(p_id, out result))
			{
				steamService.RemoveFriend(result);
			}
		}

		public void Invite(string p_id, params string[] p_args)
		{
			ulong result = 0uL;
			if (!ulong.TryParse(p_id, out result))
			{
				return;
			}
			if (invite == null)
			{
				invite = new GameInviteData();
			}
			if (invite != null)
			{
				invite.Add(p_id);
				OnInviteRefresh();
			}
			SteamService obj = service.platform as SteamService;
			Notify("service.social.friends.invite@start");
			obj.InvitePlayer(result, string.Join(" ", p_args), delegate(string p_invite_args)
			{
				if (string.IsNullOrEmpty(p_invite_args))
				{
					if (invite != null)
					{
						invite.Remove(p_id);
						OnInviteRefresh();
					}
					Notify("service.social.friends.invite@fail");
				}
				else
				{
					GameFriendData gameFriendData = Get(p_id);
					if (invite != null)
					{
						invite.Accept(p_id);
						OnInviteRefresh();
					}
					Notify("service.social.friends.invite@success", gameFriendData, p_invite_args);
				}
			});
		}

		public void OpenOverlay()
		{
			(service.platform as SteamService).OpenFriends();
		}

		protected void OnInviteRefresh()
		{
			if (invite != null && invite.success)
			{
				Notify("service.social.friends.invite@complete");
			}
		}

		protected void OnFriendRefresh(SteamEventData p_event)
		{
			if (!this)
			{
				return;
			}
			List<SteamFriend> steamFriends = (service.platform as SteamService).steamFriends;
			bool flag = false;
			List<string> load_ids = new List<string>();
			for (int i = 0; i < steamFriends.Count; i++)
			{
				SteamFriend steamFriend = steamFriends[i];
				GameFriendData gameFriendData = Get(steamFriend.id.ToString());
				if (gameFriendData == null)
				{
					gameFriendData = new GameFriendData();
					flag = true;
					load_ids.Add(steamFriend.id.ToString());
					list.Add(gameFriendData);
				}
				gameFriendData.name = steamFriend.displayName;
				gameFriendData.platformId = steamFriend.id.ToString();
				gameFriendData.photo = steamFriend.avatar;
				gameFriendData.status = (GameFriendStatusType)steamFriend.state;
				gameFriendData.ingame = steamFriend.ingame;
			}
			if (m_friend_load_loop != null)
			{
				m_friend_load_loop.Stop();
			}
			m_friend_load_loop = Activity.Run((Func<bool>)delegate
			{
				if (!DRLBootController.ready)
				{
					return true;
				}
				if (!base.validContext)
				{
					return false;
				}
				Debug.Log("SocialFriendsModel> OnFriendRefresh / Loading Profiles...");
				if (load_ids.Count > 0)
				{
					service.GetSocialProfile(load_ids.ToArray(), delegate(DRLPlayerProfileData[] p_res)
					{
						if (p_res != null && p_res.Length != 0)
						{
							for (int j = 0; j < p_res.Length; j++)
							{
								if (p_res[j] != null)
								{
									string p_id = p_res[j].platformId.ToString();
									GameFriendData gameFriendData2 = Get(p_id);
									if (gameFriendData2 != null)
									{
										gameFriendData2.hasGame = p_res[j].hasGame;
										gameFriendData2.color = (gameFriendData2.hasGame ? p_res[j].profileColor : Colorf.transparent);
										gameFriendData2.flagURL = p_res[j].flagThumbURL;
										gameFriendData2.profileThumbURL = p_res[j].profileThumbURL;
										gameFriendData2.profileRank = p_res[j].profileRank;
									}
								}
							}
							DelayFriendRefresh();
						}
					});
				}
				return false;
			}, 0f, false);
			if (!flag)
			{
				RefreshFriends();
			}
			else
			{
				DelayFriendRefresh();
			}
		}

		public void RefreshFriendsAPI()
		{
			if (!DRLApp.offline)
			{
				(service.platform as SteamService).RefreshFriends(p_force: true);
			}
		}

		protected void RefreshFriends()
		{
			if (this == null || DRLApp.offline)
			{
				return;
			}
			List<SteamFriend> steamFriends = (service.platform as SteamService).steamFriends;
			for (int i = 0; i < list.Count; i++)
			{
				GameFriendData gameFriendData = list[i];
				bool flag = false;
				for (int j = 0; j < steamFriends.Count; j++)
				{
					if (steamFriends[j].id.ToString() == gameFriendData.platformId)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.RemoveAt(i--);
				}
			}
			Notify("service.social.friends@refresh");
		}

		protected void DelayFriendRefresh()
		{
			if (m_refresh_delay != null)
			{
				m_refresh_delay.Stop();
			}
			m_refresh_delay = this.TimerRunOnce(delegate
			{
				if (base.validContext)
				{
					RefreshFriends();
				}
			}, 2f);
		}
	}
}
