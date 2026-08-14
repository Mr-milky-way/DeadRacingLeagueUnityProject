using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class InviteNotificationData : NotificationData
	{
		public string platformId
		{
			get
			{
				return Get<string>("platform-id");
			}
			set
			{
				Set("platform-id", value);
			}
		}

		public bool isFriend
		{
			get
			{
				return Get<bool>("is-friend");
			}
			set
			{
				Set("is-friend", value);
			}
		}

		public string profileName
		{
			get
			{
				return Get<string>("profile-name");
			}
			set
			{
				Set("profile-name", value);
			}
		}

		public string playerId
		{
			get
			{
				return Get<string>("player-id");
			}
			set
			{
				Set("player-id", value);
			}
		}

		public Color profileColor
		{
			get
			{
				return Colorf.ParseRGB(Get("profile-color", "#ff0000"), DRLColor.red);
			}
			set
			{
				Set("profile-color", Colorf.ToRGBHex(value));
			}
		}

		public Color profileSecondaryColor
		{
			get
			{
				return Colorf.ParseRGB(Get("profile-secondary-color", "#ff0000"), DRLColor.red);
			}
			set
			{
				Set("profile-secondary-color", Colorf.ToRGBHex(value));
			}
		}

		public int inviteRegionCode
		{
			get
			{
				return Get("invite-region-code", -1);
			}
			set
			{
				Set("invite-region-code", value);
			}
		}

		public string inviteRoomId
		{
			get
			{
				return Get<string>("invite-room-id");
			}
			set
			{
				Set("invite-room-id", value);
			}
		}

		public string inviteRoomName
		{
			get
			{
				return Get<string>("invite-room-name");
			}
			set
			{
				Set("invite-room-name", value);
			}
		}

		public bool crossplay
		{
			get
			{
				return Get("is-crossplay", p_default: true);
			}
			set
			{
				Set("is-crossplay", value);
			}
		}

		public bool inviteIsRace
		{
			get
			{
				return Get<bool>("is-race");
			}
			set
			{
				Set("is-race", value);
			}
		}

		public List<string> blockedList
		{
			get
			{
				return Get<List<string>>("blocked-list");
			}
			set
			{
				Set("blocked-list", value);
			}
		}
	}
}
