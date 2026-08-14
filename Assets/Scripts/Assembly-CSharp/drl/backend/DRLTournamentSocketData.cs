using System;
using Newtonsoft.Json.Linq;
using drl.game;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentSocketData : SerializedData
	{
		public string classType
		{
			get
			{
				return Get<string>("class");
			}
			set
			{
				Set("class", value);
			}
		}

		public string status
		{
			get
			{
				return Get<string>("status");
			}
			set
			{
				Set("status", value);
			}
		}

		public string description
		{
			get
			{
				return Get<string>("description");
			}
			set
			{
				Set("description", value);
			}
		}

		public string refresh
		{
			get
			{
				return Get<string>("refresh");
			}
			set
			{
				Set("refresh", value);
			}
		}

		public JObject meta => Get<JObject>("meta", null);

		public DRLTournamentSocketMetaData metaData
		{
			get
			{
				if (meta == null)
				{
					return null;
				}
				return meta.ToObject<DRLTournamentSocketMetaData>();
			}
		}

		public TournamentActionEvent action
		{
			get
			{
				object obj = Get<object>("action", null);
				if (obj == null)
				{
					return TournamentActionEvent.none;
				}
				return (TournamentActionEvent)Enum.Parse(typeof(TournamentActionEvent), obj.ToString());
			}
			set
			{
				Set("action", value.ToString());
			}
		}

		public string id
		{
			get
			{
				return Get("object_id", "");
			}
			set
			{
				Set("object_id", value);
			}
		}
	}
}
