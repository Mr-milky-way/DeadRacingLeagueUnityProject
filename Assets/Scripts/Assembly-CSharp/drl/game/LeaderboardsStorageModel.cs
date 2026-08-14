using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class LeaderboardsStorageModel : Model<DRLApp>
	{
		public void Save(DRLLeaderboardData p_data)
		{
			string offlineLeaderboardsRoot = DRLPaths.Storage.offlineLeaderboardsRoot;
			_ = DateTime.UtcNow;
			string filename = offlineLeaderboardsRoot + "lb-" + DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss") + ".lbc";
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
			new Thread((ThreadStart)delegate
			{
				List<byte[]> list = new List<byte[]>();
				string s = p_data.ToJson();
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				for (int i = 0; i < bytes.Length; i++)
				{
					bytes[i] ^= 0x3F;
				}
				list.Add(bytes);
				byte[] bytes2 = Serialize.ToGzip(list);
				File.WriteAllBytes(filename, bytes2);
				Debug.Log("LeaderboardsStorageModel> Leaderboard data stored. " + DateTime.UtcNow.ToString());
			}).Start();
		}

		public void Load(Action<List<DRLLeaderboardData>> p_complete)
		{
			string offlineLeaderboardsRoot = DRLPaths.Storage.offlineLeaderboardsRoot;
			List<string> leaderboard_cache_files = new List<string>(Directory.GetFiles(offlineLeaderboardsRoot, "*lbc"));
			if (leaderboard_cache_files.Count == 0 && p_complete != null)
			{
				p_complete(null);
				return;
			}
			List<DRLLeaderboardData> leaderboards = new List<DRLLeaderboardData>();
			Thread thread = new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < leaderboard_cache_files.Count; i++)
				{
					if (File.Exists(leaderboard_cache_files[i]))
					{
						byte[] array = File.ReadAllBytes(leaderboard_cache_files[i]);
						if (array != null)
						{
							byte[] array2 = Serialize.FromGZip<List<byte[]>>(array, array.Length * 20)[0];
							for (int j = 0; j < array2.Length; j++)
							{
								array2[j] ^= 0x3F;
							}
							DRLLeaderboardData item = Serialize.FromJson<DRLLeaderboardData>(Encoding.UTF8.GetString(array2));
							leaderboards.Add(item);
						}
					}
				}
				if (p_complete != null)
				{
					this.TimerRunOnce(delegate
					{
						p_complete(leaderboards);
					}, 1f / 60f);
				}
			});
			thread.Priority = System.Threading.ThreadPriority.Highest;
			thread.Start();
		}

		public void Clear()
		{
			string offlineLeaderboardsRoot = DRLPaths.Storage.offlineLeaderboardsRoot;
			List<string> leaderboard_cache_files = new List<string>(Directory.GetFiles(offlineLeaderboardsRoot, "*lbc"));
			if (leaderboard_cache_files.Count == 0)
			{
				return;
			}
			new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < leaderboard_cache_files.Count; i++)
				{
					if (File.Exists(leaderboard_cache_files[i]))
					{
						File.Delete(leaderboard_cache_files[i]);
					}
				}
			}).Start();
		}
	}
}
