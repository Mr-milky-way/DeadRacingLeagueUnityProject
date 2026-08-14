using thelab.core;

namespace drl.backend
{
	internal class DRLTryoutsHeatsData : SerializedData
	{
		public int heatCount => Get("counter", -1);
	}
}
