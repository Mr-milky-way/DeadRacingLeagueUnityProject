using thelab.core;

namespace drl.backend
{
	public class DRLTransactionItem : SerializedData
	{
		public string id;

		public int count;

		public DRLTransactionItem(string p_id, int p_count)
		{
			id = p_id;
			count = p_count;
		}

		public DRLTransactionItem()
			: this("", 1)
		{
		}
	}
}
