using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICollectablesTournamentOverviewView : UIScreenView
	{
		[Header("Game")]
		public RaceController race;

		public DRLTournamentLegacyData data;

		[Header("Screen")]
		public DRLStandingsView standings;

		public GameObject promo;

		public Text titleField;

		public RectTransform titleRT;

		public RectTransform contentRT;

		public UIStatusView status;

		[Header("Stats")]
		public TableComponent tableField;

		public FadeComponent tableFade;

		public FadeComponent separatorFade;

		public RectTransform separatorRT;

		[Header("Nav")]
		public RectTransform nextButton;

		public RectTransform roomButton;

		public RectTransform replayButton;

		public RectTransform campaignButton;

		public float titleWidth
		{
			get
			{
				return titleRT.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = titleRT.sizeDelta;
				sizeDelta.x = value;
				titleRT.sizeDelta = sizeDelta;
			}
		}

		protected void Awake()
		{
		}

		public void SetPromoEnabled(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
		}

		public void Clear()
		{
			titleField.text = "";
		}

		public void SetTitle(params string[] p_values)
		{
			string text = "";
			for (int i = 0; i < p_values.Length; i++)
			{
				if (!string.IsNullOrEmpty(p_values[i]) && i > 0)
				{
					text += "<color=#f00> / </color>";
				}
				text += p_values[i];
			}
			titleField.text = text;
		}

		public void SetTitle(string p_value, char p_separator)
		{
			SetTitle(p_value.Split(p_separator));
		}

		public void FadeTitle(float p_duration, float p_delay, params string[] p_values)
		{
			Tween.Kill(this);
			titleWidth = 0f;
			SetTitle(p_values);
			Tween.Add(this, "titleWidth", 1300f, p_duration, p_delay, Cubic.Out);
		}

		public void FadeTitle(float p_duration, float p_delay, string p_title, char p_separator)
		{
			FadeTitle(p_duration, p_delay, p_title.Split(p_separator));
		}

		public void FadeTitle(float p_duration, params string[] p_values)
		{
			FadeTitle(p_duration, 0f, p_values);
		}

		public void FadeTitle(float p_duration, string p_title, char p_separator = ',')
		{
			FadeTitle(p_duration, 0f, p_title, p_separator);
		}

		public void SetCell(int p_row, int p_col, string p_value, string p_time, Color p_color, bool p_bold = false)
		{
			tableField.Get<UITournamentResultsItem>(p_row, p_col).Set(p_value, p_time, p_color, p_bold);
		}

		public void SetHeader(int p_row, int p_col, string p_value, Color p_color, bool p_bold = false)
		{
			Text text = tableField.Set(p_row, p_col, p_value, p_color);
			if ((bool)text)
			{
				ObjectTag component = text.GetComponent<ObjectTag>();
				if ((bool)component)
				{
					Font font = component.tags[0] as Font;
					Font font2 = component.tags[1] as Font;
					text.font = (p_bold ? font2 : font);
				}
			}
		}

		public void ClearTable()
		{
			standings.Clear();
			separatorFade.alpha = -0.1f;
			tableFade.alpha = -0.1f;
			tableField.Clear(p_destroy: true);
		}

		public void Set(string p_player_sid, DRLTournamentLegacyData p_tournament)
		{
			data = p_tournament;
			ClearTable();
			if (data == null)
			{
				return;
			}
			List<DRLTournamentLegacyData.PlayerData> playerPoints = data.GetPlayerPoints(data.heats, 10f, 7f, 5f, 3f, 1f, 0f);
			Vector2 sizeDelta = separatorRT.sizeDelta;
			sizeDelta.y = (float)playerPoints.Count * 56f;
			separatorRT.sizeDelta = sizeDelta;
			sizeDelta = contentRT.sizeDelta;
			sizeDelta.y = 51.5f * (float)(playerPoints.Count - 1);
			contentRT.sizeDelta = sizeDelta;
			int order = data.order;
			int num = -1;
			standings.SetCount(playerPoints.Count);
			for (int i = 0; i < playerPoints.Count; i++)
			{
				if (playerPoints[i].playerId == p_player_sid)
				{
					num = i;
				}
				standings.Set(i, playerPoints[i].profileColor, playerPoints[i].profileThumbURL, playerPoints[i].profileName.ToUpper(), 0f, playerPoints[i].playerId == p_player_sid, playerPoints[i].playerId.ToString());
			}
			int num2 = data.heats + 1;
			int num3 = playerPoints.Count + 1;
			tableField.Resize(num3, num2);
			float num4 = 0f;
			string p_title = data.name.ToUpper() + ((order <= 0) ? "" : (",HEAT " + order));
			FadeTitle(1f, num4, p_title, ',');
			num4 += 0.5f;
			standings.Fade(p_flag: true, 0.6f, num4, 0.04f);
			num4 += 1f;
			for (int j = 0; j < num2; j++)
			{
				string p_value = ((j >= num2 - 1) ? "TOTAL" : ("HEAT " + (j + 1)));
				Color p_color = ((j >= num2 - 1) ? Color.white : DRLColor.gray4);
				SetHeader(0, j, p_value, p_color);
				if (num >= 0)
				{
					SetHeader(num + 1, j, "", Color.white, p_bold: true);
				}
			}
			IList<UITournamentResultsItem> p_list = new List<UITournamentResultsItem>();
			tableField.SetRange(1, 0, num3 - 1, num2 - 1, "·", ref p_list);
			tableFade.FadeIn(0.4f, num4);
			num4 += 0.5f;
			Vector2 a = new Vector2(0f, 0f);
			float num5 = Vector2.Distance(a, new Vector2(num2 - 1, num3 - 1));
			for (int k = 0; k < p_list.Count; k++)
			{
				TextInt textInt = (p_list[k] ? p_list[k].scoreTextInt : null);
				if ((bool)textInt)
				{
					int num6 = k % num2;
					int num7 = k / num2;
					int index = k / num2;
					float num8 = Vector2.Distance(a, new Vector2(num6, num7));
					num8 = ((num5 <= 0f) ? 0f : (num8 / num5));
					float num9 = ((num6 >= num2 - 1) ? playerPoints[index].totalPoint : playerPoints[index].points[num6]);
					if (!float.IsNaN(num9))
					{
						textInt.Animate((int)num9, 1f, num4 + num8 * 1f, Cubic.Out);
					}
				}
			}
			num4 += 2f;
			separatorFade.FadeIn(0.4f, num4);
		}

		public void SetGameType(GameFlag p_type, bool p_multiplayer)
		{
			nextButton.gameObject.SetActive(value: false);
			campaignButton.gameObject.SetActive(value: false);
			roomButton.gameObject.SetActive(value: false);
			UINavigation right = null;
			switch (p_type)
			{
			case GameFlag.Race:
				nextButton.gameObject.SetActive(!p_multiplayer);
				roomButton.gameObject.SetActive(p_multiplayer);
				right = (p_multiplayer ? roomButton.GetComponent<UINavigation>() : nextButton.GetComponent<UINavigation>());
				break;
			case GameFlag.Campaign:
				nextButton.gameObject.SetActive(value: true);
				campaignButton.gameObject.SetActive(value: true);
				right = nextButton.GetComponent<UINavigation>();
				break;
			}
			base.leftNavigation.right = right;
		}

		public void SetReplayEnabled(bool p_flag)
		{
			replayButton.GetComponent<FadeComponent>().alpha = (p_flag ? 1f : 0.2f);
		}
	}
}
