using System;
using System.Collections.Generic;

namespace drl.game
{
	[Serializable]
	public class ReplayRecord
	{
		private List<ReplayFile> m_replays;

		public List<ReplayFile> replays
		{
			get
			{
				if (m_replays != null)
				{
					return m_replays;
				}
				return m_replays = new List<ReplayFile>();
			}
		}

		public void Clear()
		{
			for (int i = 0; i < replays.Count; i++)
			{
				replays[i].ClearChannels();
			}
		}

		public void Destroy()
		{
			for (int i = 0; i < replays.Count; i++)
			{
				replays[i].Destroy();
			}
			replays.Clear();
		}

		public void Serialize()
		{
			for (int i = 0; i < replays.Count; i++)
			{
				replays[i].Serialize();
			}
		}

		public void Seek(long p_sample)
		{
			for (int i = 0; i < replays.Count; i++)
			{
				replays[i].Seek(p_sample);
			}
		}
	}
}
