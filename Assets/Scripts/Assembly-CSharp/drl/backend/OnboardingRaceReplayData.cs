using System.Collections.Generic;
using Newtonsoft.Json;
using thelab.core;

namespace drl.backend
{
	public class OnboardingRaceReplayData : SerializedData
	{
		public class Data
		{
			[JsonProperty("replay-url")]
			public static string ReplayUrl { get; set; }
		}

		public class Root
		{
			public bool success { get; set; }

			public object message { get; set; }

			public string token { get; set; }

			public object webtoken { get; set; }

			public bool encoded { get; set; }

			public List<Data> data { get; set; }
		}
	}
}
