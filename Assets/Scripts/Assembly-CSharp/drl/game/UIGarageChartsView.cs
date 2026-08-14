using UnityEngine;
using UnityEngine.UI;
using drl.sim;

namespace drl.game
{
	public class UIGarageChartsView : UIScreenView
	{
		public Text caption;

		public RectTransform[] graphs;

		public Drone drone;

		public Text debugCaption;
	}
}
