using System;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	public class TournamentMatchmakingState : IRoomState
	{
		private bool sentMatchStart;

		private int heatWaiting = 60;

		private int nextHeat;

		private int pullParticipantsPeriod = 10;

		private bool sentPullUsers;

		private Activity m_countdownActivity;

		public void OnEnter(NetworkRoom room)
		{
			sentMatchStart = false;
			sentPullUsers = false;
			room.Reset();
			room.StartTimeUTCTicks = new DateTime(room.ServerTime.Ticks).AddSeconds(heatWaiting).Ticks;
			room.LobbyCountdown = (room.LobbyCountdownAllowed ? heatWaiting : 0);
			nextHeat = room.HeatIdx + 1;
		}

		public void OnExit(NetworkRoom room)
		{
		}

		public void OnUpdate(NetworkRoom room)
		{
			if (!room.IsMaster || !room.HeatAllowed)
			{
				return;
			}
			bool flag = room.Racers.TrueForAll((NetworkActor el) => el.IsRoomReady);
			if (room.RacersCount >= room.MaxRacers && flag && !sentMatchStart)
			{
				sentMatchStart = true;
				room.HeatIdx = nextHeat;
				room.RaceId = GUID.Create(24, "", 200, 0, 15, "x1");
				room.GamePlugin.StartMatch();
			}
			if (room.RacersCount > 0 && room.LobbyCountdownAllowed)
			{
				double totalSeconds = (room.StartTimeUTC - room.ServerTime).TotalSeconds;
				totalSeconds = ((totalSeconds > 0.0) ? totalSeconds : 0.0);
				room.LobbyCountdown = (int)totalSeconds;
				UpdateLobbyCountdown(room, (float)totalSeconds);
				if (!sentMatchStart && room.LobbyCountdown <= 0)
				{
					StopLobbyCountdown();
					sentMatchStart = true;
					room.HeatIdx = nextHeat;
					room.RaceId = GUID.Create(24, "", 200, 0, 15, "x1");
					room.GamePlugin.StartMatch();
				}
			}
		}

		private void UpdateLobbyCountdown(NetworkRoom room, float p_time)
		{
			StopLobbyCountdown();
			float t = p_time;
			m_countdownActivity = Activity.Run((Predicate<float>)delegate
			{
				room.LobbyCountdown = (int)t;
				t -= Time.deltaTime;
				return t > 0f;
			}, 0f, false);
		}

		private void StopLobbyCountdown()
		{
			if (m_countdownActivity != null)
			{
				m_countdownActivity.Stop();
				m_countdownActivity = null;
			}
		}
	}
}
