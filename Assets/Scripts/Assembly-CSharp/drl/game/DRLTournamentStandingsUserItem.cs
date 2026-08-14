using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class DRLTournamentStandingsUserItem : MonoBehaviour
	{
		public GameObject userRankColorTexture;

		public GameObject userWinsColorTexture;

		public Text rankField;

		public Text usernameField;

		public Text winsField;

		public Image userBaseColorImage;

		public Image userRankColorImage;

		public Image userWinsColorImage;

		public RawImage userFlagIcon;

		[HideInInspector]
		public string playerId = "";

		private Color m_colorBase
		{
			set
			{
				userRankColorImage.color = value;
			}
		}

		private Color m_colorAdvance
		{
			set
			{
				userBaseColorImage.color = value;
				userWinsColorImage.color = value;
				userRankColorImage.color = value;
			}
		}

		public void Set(string p_playerId, int p_rank, string p_username, Color p_userColor, int p_wins, bool p_isWinner)
		{
			playerId = p_playerId;
			if (p_rank <= 0)
			{
				p_rank = 1;
			}
			rankField.text = p_rank.ToString();
			usernameField.text = p_username;
			if (p_wins < 0)
			{
				p_wins = 0;
			}
			winsField.text = p_wins.ToString();
			SetColors(p_userColor, p_isWinner: false);
			UITruncateText component = usernameField.GetComponent<UITruncateText>();
			if (!(component == null))
			{
				component.Refresh();
			}
		}

		private void SetColors(Color p_color, bool p_isWinner)
		{
			userRankColorTexture.SetActive(p_isWinner);
			userWinsColorTexture.SetActive(p_isWinner);
			if (p_isWinner)
			{
				m_colorAdvance = p_color;
			}
			else
			{
				m_colorBase = p_color;
			}
		}

		public void SetFlag(Texture p_flagIcon)
		{
			if (!(p_flagIcon == null))
			{
				userFlagIcon.texture = p_flagIcon;
			}
		}
	}
}
