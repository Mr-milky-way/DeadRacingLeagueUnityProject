using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIDMVDiagnosticCompleteScreenView : UIScreenView
	{
		public int rank;

		[SerializeField]
		private Text m_rankMessageLbl;

		public void SetRank(int rank)
		{
			m_rankMessageLbl.text = "Our diagnostic test has determined that you will be placed into Level " + rank;
		}
	}
}
