using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ReplayPlayerModel : Model<DRLApp>
	{
		[SerializeField]
		private List<ReplayClipPlayerModel> m_clips;

		[SerializeField]
		private float m_elapsed;

		[Range(0f, 3f)]
		public float speed = 1f;

		public bool reverse;

		public bool playing;

		public bool paused;

		public List<CollectableView> trackCollectables;

		public List<ReplayClipPlayerModel> clips => Reflection<object>.Assert(ref m_clips);

		public float elapsed
		{
			get
			{
				return m_elapsed;
			}
			set
			{
				if (Mathf.Abs(m_elapsed - value) > 0f)
				{
					Seek(value);
				}
			}
		}

		public float duration
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < clips.Count; i++)
				{
					num = Mathf.Max(num, clips[i].duration);
				}
				return num;
			}
		}

		public void InitializeTrack(MAEntity p_root)
		{
			if (!p_root)
			{
				return;
			}
			Debug.Log("ReplayPlayerModel> InitializeTrack");
			List<MACollectable> sortedCollectables = p_root.GetSortedCollectables();
			trackCollectables.Clear();
			for (int i = 0; i < sortedCollectables.Count; i++)
			{
				MACollectable mACollectable = sortedCollectables[i];
				mACollectable.index = i;
				mACollectable.SetHitEnabled(p_flag: false);
				CollectableView collectableView = Hierarchy.Find<CollectableView>(mACollectable.transform);
				if (!collectableView)
				{
					collectableView = mACollectable.collider.gameObject.AddComponent<CollectableView>();
				}
				trackCollectables.Add(collectableView);
			}
			ResetColectablesEvaluate();
			Debug.Log($"ReplayPlayerModel> InitializeTrack / Found {trackCollectables.Count} Collectables!");
		}

		public void EvaluateCollectables(float p_time)
		{
			int num = -1;
			for (int i = 0; i < trackCollectables.Count; i++)
			{
				if (trackCollectables[i].Evaluate(p_time) && num < 0)
				{
					num = i;
				}
			}
			if (num >= 0)
			{
				base.app.view.audio.PlayGameBalloon(trackCollectables[num].gameObject);
			}
		}

		public void ResetColectablesEvaluate()
		{
			for (int i = 0; i < trackCollectables.Count; i++)
			{
				trackCollectables[i].evaluateStartTime = -1f;
			}
		}

		public ReplayClipPlayerModel GetClip(int p_id)
		{
			if (p_id < 0)
			{
				return null;
			}
			if (p_id >= clips.Count)
			{
				return null;
			}
			return clips[p_id];
		}

		public void SetClips(List<BlackboxData> p_clips)
		{
			Clear();
			InitializeTrack(base.app.model.game.level.track.rootMap);
			List<BlackboxData> list = ((p_clips == null) ? new List<BlackboxData>() : p_clips);
			for (int i = 0; i < list.Count; i++)
			{
				ReplayClipPlayerModel replayClipPlayerModel = new GameObject(i.ToString() ?? "").AddComponent<ReplayClipPlayerModel>();
				replayClipPlayerModel.transform.SetParent(base.transform);
				replayClipPlayerModel.parent = this;
				replayClipPlayerModel.clip = list[i];
				clips.Add(replayClipPlayerModel);
			}
		}

		public void SetClips(List<ReplayFile> p_clips)
		{
			Clear();
			InitializeTrack(base.app.model.game.level.track.rootMap);
			List<ReplayFile> list = ((p_clips == null) ? new List<ReplayFile>() : p_clips);
			for (int i = 0; i < list.Count; i++)
			{
				ReplayFile replayFile = list[i];
				if (replayFile == null || replayFile.channels.Count <= 0)
				{
					Debug.LogWarning($"ReplayPlayerModel> SetClips / Invalid Replay at index [{i}]");
					continue;
				}
				ReplayClipPlayerModel replayClipPlayerModel = new GameObject(i.ToString() ?? "").AddComponent<ReplayClipPlayerModel>();
				replayClipPlayerModel.transform.SetParent(base.transform);
				replayClipPlayerModel.parent = this;
				replayClipPlayerModel.clipV2 = replayFile;
				clips.Add(replayClipPlayerModel);
			}
			bool[] array = new bool[6];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = true;
			}
			for (int k = 0; k < clips.Count; k++)
			{
				int num = clips[k].player.order;
				for (int l = 0; l < array.Length; l++)
				{
					if (array[num])
					{
						array[num] = false;
						break;
					}
					num = (num + 1) % array.Length;
				}
				clips[k].player.order = num;
			}
		}

		public void Clear()
		{
			m_elapsed = 0f;
			speed = 1f;
			reverse = false;
			playing = false;
			paused = false;
			for (int i = 0; i < clips.Count; i++)
			{
				if ((bool)clips[i])
				{
					clips[i].Clear();
					Object.Destroy(clips[i].gameObject);
				}
			}
			clips.Clear();
		}

		public void Seek(float p_time, bool p_update_drone)
		{
			m_elapsed = Mathf.Clamp(p_time, 0f, duration);
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].Seek(m_elapsed, p_update_drone);
			}
		}

		public void Seek(float p_time)
		{
			Seek(p_time, p_update_drone: true);
		}

		public void Step()
		{
			float num = Time.deltaTime * speed;
			if (reverse)
			{
				num = 0f - num;
			}
			m_elapsed = Mathf.Clamp(elapsed + num, 0f, duration);
			Seek(m_elapsed, p_update_drone: true);
		}

		public void UpdateDrones()
		{
			for (int i = 0; i < clips.Count; i++)
			{
				clips[i].UpdateDrone();
			}
		}
	}
}
