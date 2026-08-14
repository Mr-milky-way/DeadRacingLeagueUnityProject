using System;

namespace drl.game
{
	public class TournamentNotificationData : NotificationData
	{
		public string tournamentGuid
		{
			get
			{
				return Get<string>("tournament-guid");
			}
			set
			{
				Set("tournament-guid", value);
			}
		}

		public string tournamentTitle
		{
			get
			{
				return Get<string>("tournament-title");
			}
			set
			{
				Set("tournament-title", value);
			}
		}

		public string tournamentThumbnailURL
		{
			get
			{
				return Get<string>("tournament-thumb");
			}
			set
			{
				Set("tournament-thumb", value);
			}
		}

		public string tournamentDescription
		{
			get
			{
				return Get<string>("tournament-description");
			}
			set
			{
				Set("tournament-description", value);
			}
		}

		public bool isParticipant
		{
			get
			{
				return Get<bool>("tournament-participant");
			}
			set
			{
				Set("tournament-participant", value);
			}
		}

		public bool isPrivate
		{
			get
			{
				return Get("tournament-private", p_default: false);
			}
			set
			{
				Set("tournament-private", value);
			}
		}

		public TournamentNotificationType status
		{
			get
			{
				object obj = Get<object>("tournament-status", null);
				if (obj == null)
				{
					return TournamentNotificationType.None;
				}
				return (TournamentNotificationType)Enum.Parse(typeof(TournamentNotificationType), obj.ToString());
			}
			set
			{
				Set("tournament-status", value);
			}
		}
	}
}
