using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIRaceOverviewItemView : UICardView
	{
		public Text lapTimeField;

		public Text lapCountField;

		public Color defaultBackgroundColor;

		public Color fastestBackgroundColor;

		public Color slowestBackgroundColor;

		public Image lapMarkerBackground;

		public void Set(int lap_index, float p_lapTime, bool p_fastestLap = false, bool p_slowestLap = false)
		{
			string text = Format.SecondsToMMSSFFF(p_lapTime);
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

		public void Set(float p_time, float p_topSpeed, int p_collected)
		{
			string text = Format.SecondsToMMSSFFF(p_time);
			lapTimeField.text = text;
			lapMarkerBackground.color = fastestBackgroundColor;
		}
	}
}
