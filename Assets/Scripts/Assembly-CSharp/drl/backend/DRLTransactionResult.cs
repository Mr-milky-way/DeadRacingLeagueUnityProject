using thelab.core;

namespace drl.backend
{
	public class DRLTransactionResult : SerializedData
	{
		public class Params : SerializedData
		{
			public string orderId
			{
				get
				{
					return Get("orderid", "");
				}
				set
				{
					Set("orderid", value);
				}
			}

			public string transactionId
			{
				get
				{
					return Get("transid", "");
				}
				set
				{
					Set("transid", value);
				}
			}
		}

		private Params m_params;

		public string result
		{
			get
			{
				return Get("result", "ERROR");
			}
			set
			{
				Set("result", value);
			}
		}

		public Params parameters
		{
			get
			{
				if (m_params != null)
				{
					return m_params;
				}
				object obj = Get<object>("params", null);
				string p_data = ((obj == null) ? "{}" : obj.ToString());
				m_params = Serialize.FromJson<Params>(p_data);
				return m_params;
			}
		}
	}
}
