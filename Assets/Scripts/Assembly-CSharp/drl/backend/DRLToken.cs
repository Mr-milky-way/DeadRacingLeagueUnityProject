using System;
using System.Collections.Generic;
using thelab.core;

namespace drl.backend
{
	[Serializable]
	public class DRLToken
	{
		public string steamId;

		public string xbuid;

		public string playstationId;

		public string epicId;

		public string ticket;

		public string os;

		public string version;

		internal string plaformId
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

		public DRLToken()
		{
			ticket = "";
			version = "";
			os = "";
		}

		public string ToJson()
		{
			return Serialize.ToJson(this);
		}

		public string ToBase64()
		{
			return Serialize.ToBase64(ToJson());
		}

		public Dictionary<string, string> ToHashTable()
		{
			return new Dictionary<string, string> { ["token"] = ToBase64() };
		}

		public void FromBase64(string p_data)
		{
			FromJson(Serialize.FromBase64<string>(p_data));
		}

		public void FromJson(string p_data)
		{
			Serialize.FromJson(p_data, this, p_populate: true);
		}
	}
}
