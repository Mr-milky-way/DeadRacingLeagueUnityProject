using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class MovieClip<T, U> : MonoBehaviour
	{
		private U m_target;

		public T[] frames;

		public ClipLoopInterval[] loops;

		public int fps = 60;

		public bool playOnAwake;

		public bool playing;

		public bool paused;

		public bool loop;

		public bool infinite;

		public bool reverse;

		private float m_elapsed;

		public float speed = 1f;

		protected int m_loop_count;

		protected bool m_internal_reverse;

		public U target
		{
			get
			{
				if (m_target == null)
				{
					return m_target = GetComponent<U>();
				}
				return m_target;
			}
			set
			{
				m_target = value;
			}
		}

		public int frame
		{
			get
			{
				return ToFrame(m_elapsed);
			}
			set
			{
				elapsed = ToTime(value);
			}
		}

		public int count
		{
			get
			{
				if (frames != null)
				{
					return frames.Length;
				}
				return 0;
			}
		}

		public float elapsed
		{
			get
			{
				return m_elapsed;
			}
			set
			{
				m_elapsed = value;
				Refresh();
			}
		}

		public float duration => ToTime(count);

		protected float m_spf
		{
			get
			{
				if (fps > 0)
				{
					return 1f / (float)fps;
				}
				return 0f;
			}
		}

		protected virtual void Awake()
		{
			m_internal_reverse = false;
			if (playOnAwake)
			{
				Play(-1f);
			}
			Refresh();
		}

		public void Play(float p_time = 0f)
		{
			if (!playing)
			{
				playing = true;
				m_internal_reverse = false;
				m_loop_count = 0;
				if (p_time >= 0f)
				{
					elapsed = p_time;
				}
			}
		}

		public void Stop()
		{
			m_loop_count = 0;
			m_internal_reverse = false;
			playing = false;
			elapsed = 0f;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Unpause()
		{
			paused = false;
		}

		public void PauseSwitch()
		{
			paused = !paused;
		}

		public void Set(T[] p_frames)
		{
			frames = p_frames;
		}

		public void Sort(Comparison<T> p_func)
		{
			List<T> list = new List<T>(frames);
			list.Sort(p_func);
			frames = list.ToArray();
			list.Clear();
		}

		public void SortForward()
		{
			Sort(delegate(T a, T b)
			{
				string strA = "";
				string strB = "";
				if (a is UnityEngine.Object)
				{
					strA = ((UnityEngine.Object)(object)a).name;
				}
				if (b is UnityEngine.Object)
				{
					strB = ((UnityEngine.Object)(object)b).name;
				}
				return string.Compare(strA, strB);
			});
		}

		public void SortBackward()
		{
			Sort(delegate(T a, T b)
			{
				string strA = "";
				string strB = "";
				if (a is UnityEngine.Object)
				{
					strA = ((UnityEngine.Object)(object)a).name;
				}
				if (b is UnityEngine.Object)
				{
					strB = ((UnityEngine.Object)(object)b).name;
				}
				return -string.Compare(strA, strB);
			});
		}

		public void SortRandom()
		{
			Sort((T a, T b) => UnityEngine.Random.Range(-1, 1));
		}

		protected void Refresh()
		{
			int num = frame;
			if (num >= 0 && num < count)
			{
				OnFrame(frames[num]);
			}
		}

		internal void Step(float p_dt)
		{
			float num = p_dt * speed;
			if (reverse)
			{
				num = 0f - num;
			}
			if (m_internal_reverse)
			{
				num = 0f - num;
			}
			float num2 = elapsed;
			float num3 = num2 + num;
			int p_index = 0;
			bool flag = loop;
			ClipLoopInterval clipLoopInterval = GetLoop(num2, out p_index);
			if (loops.Length != 0 && p_index < 0)
			{
				flag = false;
			}
			if (flag)
			{
				float max = (clipLoopInterval.time ? duration : ((float)count));
				float num4 = Mathf.Clamp(Mathf.Min(clipLoopInterval.start, clipLoopInterval.end), 0f, max);
				float num5 = Mathf.Clamp(Mathf.Max(clipLoopInterval.start, clipLoopInterval.end), 0f, max);
				float num6 = (clipLoopInterval.time ? num4 : ToTime((int)num4));
				float num7 = (clipLoopInterval.time ? num5 : ToTime((int)num5));
				bool flag2 = false;
				int num8 = ((num2 >= num6) ? ((num2 <= num7) ? 1 : 0) : 0);
				int num9 = 0;
				if (num8 != 0)
				{
					num9 = ((num3 < num6) ? (-1) : ((num3 > num7) ? 1 : 0));
					if (num9 != 0)
					{
						m_loop_count++;
						flag2 = infinite || m_loop_count < clipLoopInterval.count;
					}
				}
				if (flag2)
				{
					if (clipLoopInterval.pingpong)
					{
						m_internal_reverse = !m_internal_reverse;
					}
					else
					{
						num3 = ((num9 < 0) ? num7 : num6);
					}
				}
			}
			elapsed = Mathf.Clamp(num3, 0f, duration);
		}

		internal virtual void Update()
		{
			if (!paused && playing)
			{
				float num = Time.deltaTime;
				if (num >= 1f / 30f)
				{
					num = 1f / 30f;
				}
				Step(num);
			}
		}

		protected virtual void OnFrame(T p_frame)
		{
		}

		protected ClipLoopInterval GetLoop(float p_time, out int p_index)
		{
			p_index = -1;
			for (int i = 0; i < loops.Length; i++)
			{
				ClipLoopInterval result = loops[i];
				float num = (result.time ? p_time : ((float)ToFrame(p_time)));
				if (result.start <= num && result.end > num)
				{
					p_index = i;
					return result;
				}
			}
			return new ClipLoopInterval
			{
				time = true,
				start = 0f,
				end = duration,
				count = 16777215
			};
		}

		protected ClipLoopInterval GetLoop(float p_time)
		{
			int p_index = 0;
			return GetLoop(p_time, out p_index);
		}

		protected int ToFrame(float p_time)
		{
			return Mathf.FloorToInt((float)fps * p_time);
		}

		protected float ToTime(int p_frame)
		{
			return (float)p_frame * m_spf;
		}
	}
}
