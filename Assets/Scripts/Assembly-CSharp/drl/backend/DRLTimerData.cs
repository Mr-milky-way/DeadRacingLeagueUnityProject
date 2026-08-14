using System;
using thelab.core;

namespace drl.backend
{
	public class DRLTimerData : SerializedData
	{
		public string id
		{
			get
			{
				return Get("id", "");
			}
			set
			{
				Set("id", value);
			}
		}

		public string startDateString
		{
			get
			{
				object obj = Get<object>("start", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime startDate
		{
			get
			{
				string s = startDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public string stopDateString
		{
			get
			{
				object obj = Get<object>("stop", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime stopDate
		{
			get
			{
				string s = stopDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public string currentDateString
		{
			get
			{
				object obj = Get<object>("current", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime currentDate
		{
			get
			{
				string s = currentDateString;
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(s, out result);
				return result;
			}
		}

		public bool active => Get("active", d: false);

		public TimeSpan timeSpan
		{
			get
			{
				if (string.IsNullOrEmpty(startDateString))
				{
					return new TimeSpan(0L);
				}
				DateTime dateTime = startDate;
				return (active ? currentDate : stopDate) - dateTime;
			}
		}

		public float elapsed => timeSpan.Seconds;
	}
}
