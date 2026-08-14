using System.Collections.Generic;

namespace drl.backend
{
	public class DRLLeaderboardRivalsResult
	{
		public DRLLeaderboardData[] top;

		public int player;

		public DRLLeaderboardData[] rivals;

		public DRLLeaderboardData past;

		public DRLLeaderboardRivalsResult()
		{
			top = new DRLLeaderboardData[0];
			player = -1;
			rivals = new DRLLeaderboardData[0];
		}

		public DRLLeaderboardData GetTop(int p_index)
		{
			if (top == null)
			{
				return null;
			}
			if (top.Length == 0)
			{
				return null;
			}
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= top.Length)
			{
				return null;
			}
			return top[p_index];
		}

		public DRLLeaderboardData GetRival(int p_index)
		{
			if (rivals == null)
			{
				return null;
			}
			if (rivals.Length == 0)
			{
				return null;
			}
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= rivals.Length)
			{
				return null;
			}
			return rivals[p_index];
		}

		public DRLLeaderboardData GetPlayer()
		{
			return GetRival(player);
		}

		public DRLLeaderboardData[] GetRival3()
		{
			DRLLeaderboardData[] array = new DRLLeaderboardData[3];
			int num = player;
			int num2 = ((num <= 0) ? (-1) : ((num >= array.Length - 1) ? 1 : 0));
			int num3 = num - 2;
			int num4 = num - 1;
			int num5 = num + 1;
			int num6 = num + 2;
			int num7 = num + 3;
			int[] array2 = null;
			switch (num2)
			{
			case -1:
				array2 = new int[3] { num, num5, num6 };
				break;
			case 0:
				array2 = new int[3] { num4, num, num5 };
				break;
			case 1:
				array2 = new int[3] { num3, num4, num };
				break;
			}
			if (array2 == null)
			{
				return array;
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetRival(array2[i]);
			}
			if (array[0] == null)
			{
				array2 = new int[3] { num5, num6, num7 };
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = GetRival(array2[j]);
				}
			}
			return array;
		}

		public string[] GetReplays(DRLLeaderboardData[] p_list, bool p_player_only, int p_player_id)
		{
			List<string> list = new List<string>();
			if (p_list == null)
			{
				return list.ToArray();
			}
			for (int i = 0; i < p_list.Length; i++)
			{
				if ((!p_player_only || i == p_player_id) && (p_player_only || i != p_player_id))
				{
					DRLLeaderboardData dRLLeaderboardData = p_list[i];
					list.Add(dRLLeaderboardData.replayURL);
				}
			}
			return list.ToArray();
		}

		public string[] GetReplays(DRLLeaderboardData[] p_list)
		{
			return GetReplays(p_list, p_player_only: false, player);
		}

		public string[] GetTopReplays(bool p_include_player = false)
		{
			return GetReplays(top, p_player_only: false, p_include_player ? (-1) : player);
		}

		public string[] GetRivalReplays()
		{
			return GetReplays((player < 0) ? new DRLLeaderboardData[0] : rivals, p_player_only: false, player);
		}

		public string[] GetPastReplays()
		{
			return GetReplays((player < 0) ? new DRLLeaderboardData[0] : rivals, p_player_only: true, player);
		}
	}
}
