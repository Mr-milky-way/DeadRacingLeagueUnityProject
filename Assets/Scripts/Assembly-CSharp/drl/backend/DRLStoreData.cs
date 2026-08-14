using thelab.core;

namespace drl.backend
{
	public class DRLStoreData : SerializedData
	{
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

		public int limit
		{
			get
			{
				return Get("limit", 0);
			}
			set
			{
				Set("limit", value);
			}
		}

		public string search
		{
			get
			{
				return Get("q", "");
			}
			set
			{
				Set("q", value);
			}
		}

		public string category
		{
			get
			{
				return Get("category", "");
			}
			set
			{
				Set("category", value);
			}
		}

		public SortType sort
		{
			set
			{
				string text = "";
				string text2 = "";
				switch (value)
				{
				case SortType.RatingCountAsc:
					text = "rating-count";
					text2 = "asc";
					break;
				case SortType.RatingCountDesc:
					text = "rating-count";
					text2 = "desc";
					break;
				case SortType.DateAsc:
					text = "created-at";
					text2 = "asc";
					break;
				case SortType.DateDesc:
					text = "created-at";
					text2 = "desc";
					break;
				case SortType.PriceAsc:
					text = "price";
					text2 = "asc";
					break;
				case SortType.PriceDesc:
					text = "price";
					text2 = "desc";
					break;
				case SortType.Featured:
					text = "featured";
					text2 = "";
					break;
				case SortType.Popular:
					text = "popular";
					text2 = "";
					break;
				}
				if (!string.IsNullOrEmpty(text))
				{
					Set("sort", text);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					Set("order", text2);
				}
			}
		}

		public void Load(string p_json)
		{
			Serialize.FromJson(p_json, this, p_populate: true);
		}
	}
}
