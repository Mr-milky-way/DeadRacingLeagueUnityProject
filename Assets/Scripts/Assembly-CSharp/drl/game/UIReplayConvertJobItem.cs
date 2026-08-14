using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIReplayConvertJobItem : MonoBehaviour
	{
		[Header("Job")]
		public ReplayConvertJob job;

		public InputField jobInfoField;

		public Text jobStatusField;

		public Image jobStatusProgressField;

		public RectTransform jobStatusProgressRT;

		public float jobProgressBarWidth;

		public void Clear()
		{
			job = null;
			SetProgress(0f);
			jobInfoField.text = "";
			jobStatusField.text = "Ready";
			jobStatusProgressField.color = Colorf.RGBToColor(26367u);
		}

		public void SetProgress(float p_progress)
		{
			RectTransform rectTransform = jobStatusProgressRT;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.x = jobProgressBarWidth * p_progress;
			rectTransform.sizeDelta = sizeDelta;
		}

		public void Update()
		{
			if (job != null)
			{
				SetProgress(job.progress);
				string text = job.state.ToString();
				ReplayConvertJobState state = job.state;
				if (state == ReplayConvertJobState.Download || (uint)(state - 7) <= 1u)
				{
					text += $" | {(int)(job.requestProgress * 100f)}%";
				}
				jobStatusField.text = text;
				string text2 = "";
				text2 = text2 + job.srcName + "\n";
				if (job.useAmazonS3)
				{
					text2 = text2 + job.amazonFileKey + "\n";
				}
				if (job.replayV1LengthKb > 0)
				{
					text2 += $"v1: {job.replayV1LengthKb}kb\n";
				}
				if (job.replayV2LengthKb > 0)
				{
					text2 += $"v2: {job.replayV2LengthKb}kb\n";
				}
				jobInfoField.text = text2;
				Color color = Colorf.RGBToColor(26367u);
				switch (job.state)
				{
				case ReplayConvertJobState.Converting:
					color = Colorf.RGBToColor(12303155u);
					break;
				case ReplayConvertJobState.Deserializing:
					color = Colorf.RGBToColor(12303155u);
					break;
				case ReplayConvertJobState.Complete:
					color = Colorf.RGBToColor(3390259u);
					break;
				case ReplayConvertJobState.Error:
					color = Colorf.RGBToColor(16711680u);
					break;
				}
				jobStatusProgressField.color = color;
			}
		}
	}
}
