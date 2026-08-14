using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICardButtonCampaignMap : UICardButtonMap
	{
		public FadeComponent lockFade;

		public RectTransform lockFooterRT;

		public RectTransform leaderboardFieldRT;

		public FadeComponent leaderboardIconField;

		public Text leaderboardField;

		public RectTransform resultFieldRT;

		public Image resultIconField;

		public Text resultField;

		public RectTransform trackFieldRT;

		public Text trackField;

		private float m_time;

		public bool locked;

		public bool complete;

		public DRLCampaignRace race;

		public override UICardType type => UICardType.ButtonCampaignMap;

		public float time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
				if (!resultIconField.gameObject.activeInHierarchy)
				{
					resultIconField.gameObject.SetActive(value: true);
				}
				resultField.text = "YOUR TIME " + Format.SecondsToTime(m_time, 2, p_use_ms: true);
			}
		}

		public float lockFooterHeight
		{
			get
			{
				return lockFooterRT.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = lockFooterRT.sizeDelta;
				sizeDelta.y = value;
				lockFooterRT.sizeDelta = sizeDelta;
			}
		}

		public float leaderboardHeight
		{
			get
			{
				return leaderboardFieldRT.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = leaderboardFieldRT.sizeDelta;
				sizeDelta.y = value;
				leaderboardFieldRT.sizeDelta = sizeDelta;
			}
		}

		public float resultHeight
		{
			get
			{
				return resultFieldRT.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = resultFieldRT.sizeDelta;
				sizeDelta.y = value;
				resultFieldRT.sizeDelta = sizeDelta;
			}
		}

		public float trackHeight
		{
			get
			{
				return trackFieldRT.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = trackFieldRT.sizeDelta;
				sizeDelta.y = value;
				trackFieldRT.sizeDelta = sizeDelta;
			}
		}

		public override void Build()
		{
			base.Build();
		}

		public void Set(DRLCampaignRace p_race)
		{
			if (p_race != null)
			{
				race = p_race;
				if (race.isCustomMap)
				{
					SetTrack(race.customMap, race.track ? race.track.map : null);
					return;
				}
				Set(race.track.map);
				SetTrack(race.track.label);
			}
		}

		public void SetLocked(bool p_flag)
		{
			locked = p_flag;
			if (p_flag)
			{
				lockFade.alpha = 0.6f;
				lockFooterHeight = 180f;
			}
			else
			{
				lockFade.alpha = 0f;
				lockFooterHeight = 0f;
			}
		}

		public void SetResultsVisible(bool p_flag)
		{
			if (!p_flag)
			{
				leaderboardHeight = 0f;
				resultHeight = 0f;
				trackHeight = 0f;
			}
			else
			{
				resultHeight = 60f;
				trackHeight = (race.isCustomMap ? 0f : 60f);
			}
		}

		public void SetLocked(bool p_flag, float p_delay, bool p_show_result = true)
		{
			float num = p_delay;
			locked = p_flag;
			if (p_flag)
			{
				if (p_show_result)
				{
					Tween.Add(this, "trackHeight", 0f, 0.15f, num, Cubic.InOut);
					num += 0.06f;
					Tween.Add(this, "resultHeight", 0f, 0.15f, num, Cubic.InOut);
					num += 0.06f;
					num += 0.1f;
				}
				lockFade.Fade(0.6f, 0.2f, num);
				Tween.Add(this, "lockFooterHeight", 180f, 0.15f, num, Cubic.Out);
			}
			else
			{
				lockFade.Fade(0f, 0.2f, num);
				Tween.Add(this, "lockFooterHeight", 0f, 0.15f, num, Cubic.Out);
				if (p_show_result)
				{
					num += 0.1f;
					Tween.Add(this, "resultHeight", 60f, 0.15f, num, Cubic.InOut);
					num += 0.06f;
					Tween.Add(this, "trackHeight", race.isCustomMap ? 0f : 60f, 0.15f, num, Cubic.InOut);
					num += 0.06f;
				}
			}
		}

		public new void Clear()
		{
			locked = true;
			trackHeight = 0f;
			resultHeight = 0f;
			leaderboardHeight = 0f;
			lockFooterHeight = 0f;
			lockFade.alpha = -0.1f;
		}

		public void SetLeaderboard(int p_position)
		{
			if (p_position <= 0)
			{
				leaderboardField.text = "";
				leaderboardIconField.alpha = 0.2f;
			}
			else
			{
				leaderboardField.text = "#" + p_position + " ON LEADERBOARDS";
				leaderboardIconField.alpha = 1f;
			}
		}

		public void SetResult(string p_text)
		{
			Tween.Kill(this, "time");
			resultField.text = p_text;
			resultIconField.gameObject.SetActive(value: false);
		}

		public void SetResult(float p_time, float p_delay = 0f)
		{
			Tween.Kill(this, "time");
			Tween.Add(this, "time", p_time, 0.5f, p_delay, Cubic.Out);
		}

		public void SetTrack(string p_text)
		{
			trackField.text = p_text;
		}
	}
}
