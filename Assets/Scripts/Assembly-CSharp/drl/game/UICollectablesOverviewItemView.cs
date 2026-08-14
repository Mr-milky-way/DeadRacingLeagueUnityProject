using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICollectablesOverviewItemView : UICardView
	{
		public Text lapTimeField;

		public Text lapCountField;

		public Color defaultBackgroundColor;

		public Color fastestBackgroundColor;

		public Color slowestBackgroundColor;

		public Image lapMarkerBackground;

		public void Set(int lap_index, float p_lapTime, bool p_fastestLap = false, bool p_slowestLap = false)
		{
			string text = Format.SecondsToTime(p_lapTime, 2, p_use_ms: true) ?? "";
			lapCountField.text = base.app.model.storage.locale.Get("game-race-overview-screen.lap", "LAP") + $" {lap_index + 1} ";
			lapTimeField.text = text;
			lapMarkerBackground.enabled = true;
			if (p_fastestLap)
			{
				lapCountField.text += base.app.model.storage.locale.Get("game-race-overview-screen.fastest", "FASTEST");
				lapMarkerBackground.color = fastestBackgroundColor;
			}
			else if (p_slowestLap)
			{
				lapCountField.text += base.app.model.storage.locale.Get("game-race-overview-screen.slowest", "SLOWEST");
				lapMarkerBackground.color = slowestBackgroundColor;
			}
		}
	}
}
