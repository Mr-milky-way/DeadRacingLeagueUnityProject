using UnityEngine;

namespace drl.game
{
	public class DRLTournamentStandingsHeatItem : MonoBehaviour
	{
		public GameObject activeLayout;

		public GameObject inactiveLayout;

		public void SetLayoutActive(bool p_active = true)
		{
			activeLayout.SetActive(p_active);
			inactiveLayout.SetActive(!p_active);
		}
	}
}
