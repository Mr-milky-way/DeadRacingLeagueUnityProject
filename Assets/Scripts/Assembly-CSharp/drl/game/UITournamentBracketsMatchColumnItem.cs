using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UITournamentBracketsMatchColumnItem : MonoBehaviour
	{
		public ListComponent matchesList;

		public float lineMarginWidth = 72.5f;

		public float lineMiddleWidth = 25f;

		public float leftLineWidth;

		public float rightLineWidth;

		public float graphicLinesFirstWidth = 75f;

		public float graphicLinesMiddleWidth = 50f;

		public float graphicLinesSecondLastWidth = 75f;

		public float graphicLinesLastWidth = 25f;

		public TournamentRoundGameMode roundGameMode;

		public DRLTournamentRoundData data;

		private void Set()
		{
		}
	}
}
