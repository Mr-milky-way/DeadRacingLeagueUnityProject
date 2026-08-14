using System;
using System.Collections.Generic;
using thelab.core;

namespace drl.game
{
	public class PollResultModel : SerializedData
	{
		[Serializable]
		public class Entry
		{
			public string question;

			public string answer;

			public Entry(string p_question, string p_answer)
			{
				question = p_question;
				answer = p_answer;
			}

			public Entry()
				: this("", "")
			{
			}
		}

		public string playerId
		{
			get
			{
				return Get("player-id", "");
			}
			set
			{
				Set("player-id", value);
			}
		}

		public GameFlag type
		{
			get
			{
				return Reflection<object>.GetEnum<GameFlag>(Get("type", "None"));
			}
			set
			{
				Set("type", value.ToString());
			}
		}

		public GameFlag mode
		{
			get
			{
				return Reflection<object>.GetEnum<GameFlag>(Get("mode", "None"));
			}
			set
			{
				Set("mode", value.ToString());
			}
		}

		public string timestamp
		{
			get
			{
				return Get("timestamp", "");
			}
			set
			{
				Set("timestamp", value);
			}
		}

		public string mission
		{
			get
			{
				return Get("mission", "");
			}
			set
			{
				Set("mission", value);
			}
		}

		public string campaign
		{
			get
			{
				return Get("campaign", "");
			}
			set
			{
				Set("campaign", value);
			}
		}

		public string map
		{
			get
			{
				return Get("map", "");
			}
			set
			{
				Set("map", value);
			}
		}

		public string track
		{
			get
			{
				return Get("track", "");
			}
			set
			{
				Set("track", value);
			}
		}

		public int score
		{
			get
			{
				return Get("score", 0);
			}
			set
			{
				Set("score", value);
			}
		}

		public List<Entry> entries
		{
			get
			{
				return Serialize.FromJson<List<Entry>>(Get("entries", "[]"));
			}
			set
			{
				string v = ((value == null) ? "[]" : Serialize.ToJson(value));
				Set("entries", v);
			}
		}

		public string guid
		{
			get
			{
				string text = Get("guid", "");
				if (string.IsNullOrEmpty(text) || text.Length < 24)
				{
					text = GUID.Create(24, "", 200, 0, 15, "x1");
					Set("guid", text);
				}
				return text;
			}
			set
			{
				string text = value;
				if (string.IsNullOrEmpty(text) || text.Length < 24)
				{
					text = GUID.Create(24, "", 200, 0, 15, "x1");
				}
				Set("guid", text);
			}
		}

		public PollResultModel()
		{
			guid = "";
		}
	}
}
