using System;
using System.Collections.Generic;
using UnityExt.Core.IO;

namespace drl.sim
{
	[Serializable]
	public class BlackboxRecord
	{
		[SerializableField]
		public List<BlackboxData> clips;

		public static BlackboxRecord Merge(BlackboxRecord[] p_args)
		{
			BlackboxRecord blackboxRecord = new BlackboxRecord();
			foreach (BlackboxRecord blackboxRecord2 in p_args)
			{
				if (blackboxRecord2 != null)
				{
					blackboxRecord.clips.AddRange(blackboxRecord2.clips);
				}
			}
			return blackboxRecord;
		}

		public static BlackboxRecord Merge(List<BlackboxRecord> p_args)
		{
			BlackboxRecord blackboxRecord = new BlackboxRecord();
			for (int i = 0; i < p_args.Count; i++)
			{
				BlackboxRecord blackboxRecord2 = p_args[i];
				if (blackboxRecord2 != null)
				{
					blackboxRecord.clips.AddRange(blackboxRecord2.clips);
				}
			}
			return blackboxRecord;
		}

		public BlackboxRecord()
		{
			clips = new List<BlackboxData>();
		}

		public string GetMapGUID()
		{
			if (clips.Count <= 0)
			{
				return "";
			}
			for (int i = 0; i < clips.Count; i++)
			{
				string mapGUID = clips[i].GetMapGUID();
				if (!string.IsNullOrEmpty(mapGUID))
				{
					return mapGUID;
				}
			}
			return "";
		}

		public string GetCustomMapGUID()
		{
			if (clips.Count <= 0)
			{
				return "";
			}
			for (int i = 0; i < clips.Count; i++)
			{
				string customMapGUID = clips[i].GetCustomMapGUID();
				if (!string.IsNullOrEmpty(customMapGUID))
				{
					return customMapGUID;
				}
			}
			return "";
		}

		public bool IsCustomMap()
		{
			if (clips.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < clips.Count; i++)
			{
				if (clips[i].HasCustomMapFlag())
				{
					return clips[i].IsCustomMap();
				}
			}
			return false;
		}

		public string GetTrackGUID()
		{
			if (clips.Count <= 0)
			{
				return "";
			}
			for (int i = 0; i < clips.Count; i++)
			{
				string trackGUID = clips[i].GetTrackGUID();
				if (!string.IsNullOrEmpty(trackGUID))
				{
					return trackGUID;
				}
			}
			return "";
		}

		public bool GetPhysicsFlag()
		{
			if (clips.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < clips.Count; i++)
			{
				if (clips[i].GetPhysicsFlag())
				{
					return true;
				}
			}
			return false;
		}

		public void SetPhysicsFlag(bool p_flag)
		{
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].SetPhysicsFlag(p_flag);
			}
		}

		public void Clear()
		{
			if (clips == null)
			{
				clips = new List<BlackboxData>();
			}
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].Clear();
			}
			clips.Clear();
		}

		public void ClearTrackTables()
		{
			if (clips == null)
			{
				clips = new List<BlackboxData>();
			}
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].ClearTrackTable();
			}
		}

		public void Compress()
		{
			if (clips == null)
			{
				clips = new List<BlackboxData>();
			}
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].Compress();
			}
		}

		public void Decompress(Action<float> p_progressCallback = null)
		{
			if (clips == null)
			{
				clips = new List<BlackboxData>();
			}
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].Decompress();
				p_progressCallback?.Invoke((float)(i + 1) / (float)clips.Count);
			}
		}

		public void Prune()
		{
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].Prune();
			}
		}

		public bool Contains(BlackboxData p_data)
		{
			if (clips == null)
			{
				return false;
			}
			if (clips.Count <= 0)
			{
				return false;
			}
			return clips.Contains(p_data);
		}

		public BlackboxData Add(BlackboxData p_data)
		{
			if (p_data == null)
			{
				return null;
			}
			if (clips == null)
			{
				return null;
			}
			if (clips.Contains(p_data))
			{
				return p_data;
			}
			clips.Add(p_data);
			return p_data;
		}

		public BlackboxData Add(float p_duration, int p_fps, byte p_flags)
		{
			BlackboxData p_data = new BlackboxData(p_duration, p_fps, p_flags);
			return Add(p_data);
		}

		public void Set(int p_index, BlackboxData p_data)
		{
			if (clips != null && p_index >= 0 && p_index < clips.Count)
			{
				clips[p_index] = p_data;
			}
		}

		public void RemoveAt(int p_index)
		{
			if (clips != null && p_index >= 0 && p_index < clips.Count)
			{
				clips.RemoveAt(p_index);
			}
		}

		public void Remove(BlackboxData p_data)
		{
			if (Contains(p_data))
			{
				clips.Remove(p_data);
			}
		}

		public void Update(float p_dt)
		{
			if (clips != null)
			{
				for (int i = 0; i < clips.Count; i++)
				{
					clips[i].Update(p_dt);
				}
			}
		}

		public void Trim()
		{
			if (clips != null)
			{
				for (int i = 0; i < clips.Count; i++)
				{
					clips[i].Trim();
				}
			}
		}

		public void ParseTracks()
		{
			if (clips != null)
			{
				for (int i = 0; i < clips.Count; i++)
				{
					clips[i].ParseTracks();
				}
			}
		}

		public void ClearFrames()
		{
			if (clips != null)
			{
				for (int i = 0; i < clips.Count; i++)
				{
					clips[i].frames = new BlackboxFrame[0];
				}
			}
		}
	}
}
