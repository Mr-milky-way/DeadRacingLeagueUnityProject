using System.Collections.Generic;
using thelab.core;

namespace drl.game
{
	public class GameInviteData
	{
		public string guid;

		public List<string> players;

		public List<string> accepts;

		public List<Activity> timeouts;

		public bool success
		{
			get
			{
				if (players.Count <= 0)
				{
					return false;
				}
				return accepts.Count == players.Count;
			}
		}

		public GameInviteData()
		{
			guid = GUID.Create(16, "", 200, 0, 15, "x1");
			players = new List<string>();
			accepts = new List<string>();
		}

		public void Add(string p_id)
		{
			if (!players.Contains(p_id))
			{
				players.Add(p_id);
				timeouts.Add(Activity.RunOnce(delegate
				{
					Remove(p_id);
				}));
			}
		}

		public void Remove(string p_id)
		{
			if (players.Contains(p_id) && !accepts.Contains(p_id))
			{
				players.Remove(p_id);
			}
		}

		public void Accept(string p_id)
		{
			if (players.Contains(p_id))
			{
				accepts.Add(p_id);
			}
		}
	}
}
