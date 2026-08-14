using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class CircuitController : RaceController
	{
		private Activity m_race_complete_timer;

		private new MonoActivity m_replay_stop_timer;

		private int m_droneClass;

		private bool m_official;

		private bool m_resetAfterPause;

		private bool m_raceFinished;

		private bool m_tournamentRefreshed;

		private bool m_replayUploadStarted;

		private CircuitStateModel circuitModel;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				_ = p_event == "back-button-pressed";
			}
		}

		protected override void OnRaceComplete(float p_race_time, RaceStatusType p_status)
		{
			Debug.Log("<color=green>CircuitController></color> OnRaceCompleted was called..");
		}
	}
}
