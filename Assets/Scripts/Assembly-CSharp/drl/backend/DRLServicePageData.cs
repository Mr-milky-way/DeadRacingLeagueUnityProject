using thelab.core;

namespace drl.backend
{
	public class DRLServicePageData : SerializedData
	{
		public int pageTotal
		{
			get
			{
				return Get("page-total", 0);
			}
			set
			{
				Set("page-total", value);
			}
		}

		public int page
		{
			get
			{
				return Get("page", 0);
			}
			set
			{
				Set("page", value);
			}
		}

		public string nextPageURL
		{
			get
			{
				return Get("next-page-url", "");
			}
			set
			{
				Set("next-page-url", value);
			}
		}

		public string prevPageURL
		{
			get
			{
				return Get("previous-page-url", "");
			}
			set
			{
				Set("previous-page-url", value);
			}
		}
	}
}
