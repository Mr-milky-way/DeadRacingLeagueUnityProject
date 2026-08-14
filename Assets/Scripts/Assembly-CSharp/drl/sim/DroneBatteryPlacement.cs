using UnityEngine;

namespace drl.sim
{
	public class DroneBatteryPlacement : MonoBehaviour
	{
		public Transform cables;

		[Tooltip("center - set only on battery")]
		public Transform center;

		[Tooltip("set only on frame")]
		public Transform strapExtCenter;

		public Transform strapExtLeft;

		public Transform strapExtRight;

		public Transform strapIntLeft;

		public Transform strapIntRight;

		[Tooltip("set only on frame")]
		public bool centralize;

		[Tooltip("set only on frame - if left and right points has to be swapped because of battery position")]
		public bool swapLeftAndRight;

		public void PopulateForBattery()
		{
			cables = base.transform.Find("guide-cables-end");
			center = base.transform.Find("guide-center");
			strapExtLeft = base.transform.Find("guide-strap-ext-left");
			strapExtRight = base.transform.Find("guide-strap-ext-right");
			strapIntLeft = base.transform.Find("guide-strap-int-left");
			strapIntRight = base.transform.Find("guide-strap-int-right");
		}

		public void PopulateForFrame()
		{
			cables = base.transform.Find("helper-cables-end");
			strapExtCenter = base.transform.Find("helper-strap-ext-center");
			strapExtLeft = base.transform.Find("helper-strap-ext-left");
			strapExtRight = base.transform.Find("helper-strap-ext-right");
			strapIntLeft = base.transform.Find("helper-strap-int-left");
			strapIntRight = base.transform.Find("helper-strap-int-right");
		}
	}
}
