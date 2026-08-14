using UnityEngine;
using UnityEngine.UI;
using thelab.core;

public class UITournamentResultsItem : MonoBehaviour
{
	public Text score;

	public TextInt scoreTextInt;

	public Text time;

	public new ObjectTag tag;

	public void Set(string p_score, string p_time, Color p_scoreColor, bool p_bold = false)
	{
		score.text = p_score;
		score.color = p_scoreColor;
		if (!(tag == null))
		{
			Font font = tag.tags[0] as Font;
			Font font2 = tag.tags[1] as Font;
			score.font = (p_bold ? font2 : font);
		}
	}

	public void SetTime(float p_time)
	{
		if (p_time < 0f)
		{
			time.text = "TIMEOUT";
			return;
		}
		if (Mathf.Approximately(p_time, 0f))
		{
			time.text = "--:--:--";
			return;
		}
		float p_seconds = p_time / 1000f;
		time.text = Format.SecondsToTime(p_seconds, 2, p_use_ms: true);
	}

	public void SetPositionSuffix(string p_text = "")
	{
		if (!string.IsNullOrEmpty(p_text))
		{
			scoreTextInt.SetText(p_text);
			return;
		}
		if (!int.TryParse(scoreTextInt.text, out var result))
		{
			result = -1;
		}
		switch (result)
		{
		case -1:
		case 0:
			break;
		case 1:
			scoreTextInt.SetText("1st");
			break;
		case 2:
			scoreTextInt.SetText("2nd");
			break;
		case 3:
			scoreTextInt.SetText("3rd");
			break;
		default:
			scoreTextInt.SetText(result + "th");
			break;
		}
	}
}
