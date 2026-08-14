using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ReplaysStorageModel : Model<DRLApp>
	{
		private static bool TempReplayCleaned;

		public TextAsset recorderWin;

		public TextAsset recorderOSX;

		private string m_replayTmpPath;

		private bool m_is_written;

		public StorageModel storage => AssertParent<StorageModel>("storage");

		protected void Awake()
		{
			if (true)
			{
				UnityEngine.Object.Destroy(recorderWin);
				UnityEngine.Object.Destroy(recorderOSX);
				recorderWin = (recorderOSX = null);
			}
			if (!TempReplayCleaned)
			{
				DRLPaths.Storage.ClearReplaysTemp();
				TempReplayCleaned = true;
			}
			m_replayTmpPath = ReplayStream.GetReplayTempFilePath();
		}

		public void WriteRecorderApp()
		{
			if (!m_is_written)
			{
				m_is_written = true;
				TextAsset textAsset = ((OS.prefix == "win") ? recorderWin : recorderOSX);
				if ((bool)textAsset)
				{
					byte[] bytes = textAsset.bytes;
					File.WriteAllBytes(DRLPaths.Storage.videoRecorderExecutable, bytes);
				}
			}
		}

		public List<FileInfo> FindAllReplays(string p_path, Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			List<FileInfo> list = new List<FileInfo>();
			string[] files = Directory.GetFiles(p_path, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(files[i]);
				string text = fileInfo.FullName.ToLower();
				if (text.Contains(".json"))
				{
					continue;
				}
				bool flag = text.Contains("rpl2");
				if (!ReplayFile.EnableVersion2 || flag)
				{
					bool flag2 = true;
					if (p_filter != null)
					{
						flag2 = p_filter(fileInfo);
					}
					if (flag2)
					{
						list.Add(fileInfo);
					}
					if (p_max_results > 0 && list.Count >= p_max_results)
					{
						break;
					}
				}
			}
			list.Sort((FileInfo a, FileInfo b) => (!(a.CreationTime < b.CreationTime)) ? 1 : (-1));
			return list;
		}

		public List<FileInfo> FindAllReplays(Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			return FindAllReplays(DRLPaths.Storage.replaysRoot, p_filter, p_max_results);
		}

		public List<FileInfo> FindAllMapEditorReplays(Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			return FindAllReplays(DRLPaths.Storage.replaysMapEditorRoot, p_filter, p_max_results);
		}

		public List<BlackboxRecord> ReadReplays(string p_path, Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			List<BlackboxRecord> list = new List<BlackboxRecord>();
			List<FileInfo> list2 = FindAllReplays(p_path, p_filter, p_max_results);
			for (int i = 0; i < list2.Count; i++)
			{
				BlackboxRecord item = Serialize.FromBytes<BlackboxRecord>(File.ReadAllBytes(list2[i].FullName), p_unsafe: true);
				list.Add(item);
			}
			return list;
		}

		public List<ReplayFile> ReadReplaysV2(string p_path, Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			List<ReplayFile> list = new List<ReplayFile>();
			List<FileInfo> list2 = FindAllReplays(p_path, p_filter, p_max_results);
			for (int i = 0; i < list2.Count; i++)
			{
				string fullName = list2[i].FullName;
				ReplayFile replayFile = new ReplayFile();
				replayFile.Deserialize(fullName);
				list.Add(replayFile);
			}
			return list;
		}

		public List<BlackboxRecord> ReadMapEditorReplays(Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			return ReadReplays(DRLPaths.Storage.replaysMapEditorRoot, p_filter, p_max_results);
		}

		public List<ReplayFile> ReadMapEditorReplaysV2(Predicate<FileInfo> p_filter = null, int p_max_results = 0)
		{
			return ReadReplaysV2(DRLPaths.Storage.replaysMapEditorRoot, p_filter, p_max_results);
		}

		public void SaveReplayCache(string p_key, byte[] p_file)
		{
			if (p_file == null)
			{
				Debug.LogWarning("ReplayStorageModel> SaveReplayCache / Invalid Replay File!");
				return;
			}
			string replaysRoot = DRLPaths.Storage.replaysRoot;
			string text = "$cache-" + p_key + ".rpl.bytes";
			File.WriteAllBytes(replaysRoot + text, p_file);
		}

		public ReplayFile GetReplayCache(string p_key)
		{
			string replaysRoot = DRLPaths.Storage.replaysRoot;
			string text = "$cache-" + p_key + ".rpl.bytes";
			string text2 = replaysRoot + text;
			if (!File.Exists(text2))
			{
				Debug.LogWarning("ReplayStorageModel> GetReplayCache / File not found!\n  " + text2);
				return null;
			}
			ReplayFile replayFile = new ReplayFile();
			replayFile.Deserialize(text2);
			return replayFile;
		}

		public void DeleteReplayCache(string p_pattern)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(DRLPaths.Storage.replaysRoot);
			if (directoryInfo.Exists)
			{
				FileInfo[] files = directoryInfo.GetFiles("$cache-" + p_pattern, SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					files[i].Delete();
				}
			}
		}
	}
}
