using System;
using System.Collections.Generic;

namespace thelabe.core
{
	[Serializable]
	public class TMatch
	{
		public enum State
		{
			Idle = 0,
			Active = 1,
			Complete = 2
		}

		public int id;

		public int level;

		public string name;

		public List<int> players;

		public int maxWinners;

		public int heatCount;

		public State state;

		public DateTime startTime;

		public DateTime currentTime;

		public DateTime finishTime;

		public int maxResults => players.Count * heatCount;

		public void SetActive()
		{
			startTime = DateTime.Now;
			currentTime = startTime;
			int num = 60 * ((level <= 0) ? 5 : 2);
			int num2 = heatCount * 60 * 3;
			finishTime = startTime + new TimeSpan(0, 0, num + num2);
			state = State.Active;
		}

		public void SetComplete()
		{
			currentTime = DateTime.Now;
			finishTime = currentTime;
			state = State.Complete;
		}
	}
}
