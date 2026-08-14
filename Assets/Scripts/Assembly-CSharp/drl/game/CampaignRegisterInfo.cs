using thelab.core;

namespace drl.game
{
	public class CampaignRegisterInfo : SerializedData
	{
		public string guid
		{
			get
			{
				return Get("campaign-guid", "");
			}
			set
			{
				Set("campaign-guid", value);
			}
		}

		public int age
		{
			get
			{
				return Get("profile-age", 0);
			}
			set
			{
				Set("profile-age", value);
			}
		}

		public string name
		{
			get
			{
				return Get("profile-name", "");
			}
			set
			{
				Set("profile-name", value);
			}
		}

		public string email
		{
			get
			{
				return Get("profile-email", "");
			}
			set
			{
				Set("profile-email", value);
			}
		}

		public string area
		{
			get
			{
				return Get("profile-area", "");
			}
			set
			{
				Set("profile-area", value);
			}
		}

		public string phone
		{
			get
			{
				return Get("profile-phone", "");
			}
			set
			{
				Set("profile-phone", value);
			}
		}

		public string gender
		{
			get
			{
				return Get("profile-gender", "");
			}
			set
			{
				Set("profile-gender", value);
			}
		}

		public string country
		{
			get
			{
				return Get("profile-country", "");
			}
			set
			{
				Set("profile-country", value);
			}
		}

		public string americanCitizen
		{
			get
			{
				return Get("profile-american-citizen", "");
			}
			set
			{
				Set("profile-american-citizen", value);
			}
		}

		public string experienceNonFPV
		{
			get
			{
				return Get("experience-non-fpv", "");
			}
			set
			{
				Set("experience-non-fpv", value);
			}
		}

		public string experienceNonFPVYears
		{
			get
			{
				return Get("experience-non-fpv-years", "");
			}
			set
			{
				Set("experience-non-fpv-years", value);
			}
		}

		public string experienceFPV
		{
			get
			{
				return Get("experience-fpv", "");
			}
			set
			{
				Set("experience-fpv", value);
			}
		}

		public string experienceFPVYears
		{
			get
			{
				return Get("experience-fpv-years", "");
			}
			set
			{
				Set("experience-fpv-years", value);
			}
		}

		public string experiencePreferenceFPV
		{
			get
			{
				return Get("experience-preference-fpv", "");
			}
			set
			{
				Set("experience-preference-fpv", value);
			}
		}

		public string experienceRealLifeRacing
		{
			get
			{
				return Get("experience-real-life-racing", "");
			}
			set
			{
				Set("experience-real-life-racing", value);
			}
		}

		public string experienceBuiltOwnDrone
		{
			get
			{
				return Get("experience-built-own-drone", "");
			}
			set
			{
				Set("experience-built-own-drone", value);
			}
		}

		public string affiliationWatchDRL
		{
			get
			{
				return Get("affiliation-watch-drl", "");
			}
			set
			{
				Set("affiliation-watch-drl", value);
			}
		}

		public string affiliationMultiGP
		{
			get
			{
				return Get("affiliation-multigp", "");
			}
			set
			{
				Set("affiliation-multigp", value);
			}
		}

		public string affiliationMilitary
		{
			get
			{
				return Get("affiliation-military", "");
			}
			set
			{
				Set("affiliation-military", value);
			}
		}

		public string affiliationAMA
		{
			get
			{
				return Get("affiliation-ama", "");
			}
			set
			{
				Set("affiliation-ama", value);
			}
		}
	}
}
