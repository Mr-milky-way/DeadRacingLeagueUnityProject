using System;
using Steamworks;
using UnityEngine;

namespace drl.backend
{
	[Serializable]
	public class SteamFriend
	{
		public enum State
		{
			Offline = 0,
			Online = 1,
			Busy = 2,
			Away = 3,
			Snooze = 4,
			LookingToTrade = 5,
			LookingToPlay = 6
		}

		public string displayName;

		[HideInInspector]
		public CSteamID steamId;

		public ulong id;

		[HideInInspector]
		public ulong appId;

		public ulong ingameId;

		public bool ingame;

		public bool online;

		public State state;

		[SerializeField]
		private Texture2D m_avatar;

		private static Texture2D m_empty_avatar;

		public Texture2D avatar
		{
			get
			{
				if (!m_avatar)
				{
					return m_empty_avatar;
				}
				return m_avatar;
			}
			set
			{
				m_avatar = value;
			}
		}

		public SteamFriend()
		{
			if (!m_empty_avatar)
			{
				m_empty_avatar = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
				m_empty_avatar.SetPixel(1, 1, Color.black);
				m_empty_avatar.Apply();
			}
		}

		public void Refresh()
		{
			displayName = SteamFriends.GetFriendPersonaName(steamId);
			id = steamId.m_SteamID;
			ingame = SteamFriends.GetFriendGamePlayed(steamId, out var pFriendGameInfo);
			ingameId = (ingame ? pFriendGameInfo.m_gameID.AppID().m_AppId : 0u);
			ingame = ingameId == appId;
			state = (State)SteamFriends.GetFriendPersonaState(steamId);
			online = state != State.Offline;
		}

		public bool HasChange(SteamFriend v)
		{
			if (v == null)
			{
				return false;
			}
			if (v.id != id)
			{
				return false;
			}
			if (v.ingame != ingame)
			{
				return true;
			}
			if (v.ingameId != ingameId)
			{
				return true;
			}
			if (v.state != state)
			{
				return true;
			}
			if (v.displayName != displayName)
			{
				return true;
			}
			return false;
		}
	}
}
