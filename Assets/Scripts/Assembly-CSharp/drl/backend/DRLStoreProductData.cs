using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLStoreProductData : SerializedData
	{
		public string productId => Get("id", "");

		public string platformId => Get("platform-id", "");

		public float price => Get("price", 0f);

		public bool limited
		{
			get
			{
				return Get("limited", d: false);
			}
			set
			{
				Set("limited", value);
			}
		}

		public int currentAvailableAmount => Get("currentAvailableAmount", 0);

		public int maxAvailableAmount => Get("maxAvailableAmount", 0);

		public string name => Get("name", "");

		public bool featured
		{
			get
			{
				return Get("featured", d: false);
			}
			set
			{
				Set("featured", value);
			}
		}

		public string category => Get("category", "");

		public string thumbURL => Get("thumb-url", "");

		public string[] assets => Get("assets", new string[0]);

		public string imageURL => Get("image-url", "");

		public string[] items
		{
			get
			{
				JArray jArray = (JArray)Get<object>("items", null);
				if (jArray != null)
				{
					return jArray.ToObject<string[]>();
				}
				return new string[0];
			}
		}
	}
}
