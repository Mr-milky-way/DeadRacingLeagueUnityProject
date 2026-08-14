using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class DRLACS : View<DRLApp>
	{
		public float clockLocal;

		public float clockSystem;

		public List<float> ratioSamples;

		public int maxSamples = 10;

		public float avgRatio = 1f;

		public int cheatCount;

		public float cheatThreshold = 0.05f;

		public float cheatRepeatThreshold = 5f;

		public bool cheat;

		public bool cheatEver;

		public int sampleRate = 5000;

		public bool guiEnabled;

		public bool guiAllowed;

		protected Thread m_poll_thread;

		protected float m_clock_local;

		protected bool m_has_clear;

		protected string m_file_root;

		public string GetSamplesString()
		{
			if (ratioSamples == null)
			{
				return "";
			}
			return string.Join(",", ratioSamples.ConvertAll((float v) => v.ToString("0.00")));
		}

		protected void Start()
		{
			m_file_root = DRLPaths.Storage.userDocumentsRoot;
			ratioSamples = new List<float>();
			if (m_poll_thread != null && m_poll_thread.IsAlive)
			{
				m_poll_thread.Abort();
			}
			m_poll_thread = new Thread((ThreadStart)delegate
			{
				Clear();
				while (true)
				{
					DateTime systemTime = GetSystemTime();
					Thread.Sleep(sampleRate);
					DateTime systemTime2 = GetSystemTime();
					if (!cheatEver)
					{
						if (m_has_clear)
						{
							m_has_clear = false;
							m_clock_local = 0f;
						}
						else
						{
							float num = (float)((systemTime2 - systemTime).TotalMilliseconds / 1000.0);
							clockSystem = num;
							clockLocal = m_clock_local;
							clockSystem = Mathf.Round(clockSystem * 100f) / 100f;
							clockLocal = Mathf.Round(clockLocal * 100f) / 100f;
							m_clock_local = 0f;
							float num2 = ((clockSystem <= 0f) ? 1f : (clockLocal / clockSystem));
							num2 = Mathf.Round(num2 * 100f) / 100f;
							ratioSamples.Add(num2);
							if (ratioSamples.Count > maxSamples)
							{
								ratioSamples.RemoveAt(0);
							}
							avgRatio = 1f;
							if (ratioSamples.Count > 1)
							{
								float num3 = 0f;
								float num4 = ((ratioSamples.Count <= 0) ? 1f : (1f / (float)ratioSamples.Count));
								for (int i = 0; i < ratioSamples.Count; i++)
								{
									num3 += ratioSamples[i] * num4;
								}
								float b = Mathf.Round(num3 * 100f) / 100f;
								avgRatio = Mathf.Lerp(avgRatio, b, 0.9f);
								cheat = Mathf.Abs(1f - avgRatio) > cheatThreshold;
								int num5 = 0;
								for (int j = 0; j < ratioSamples.Count; j++)
								{
									float num6 = ratioSamples[j];
									num5 = ((((num6 >= 1f) ? (num6 - 1f) : (1f - num6)) >= cheatThreshold) ? (num5 + 1) : 0);
									if ((float)num5 >= cheatRepeatThreshold)
									{
										break;
									}
								}
								cheatCount = num5;
								bool num7 = cheatEver;
								if ((float)cheatCount >= cheatRepeatThreshold)
								{
									cheatEver = true;
								}
								if (num7 != cheatEver && cheatEver)
								{
									Debug.Log($"DRLACS> Cheat Detected\n{avgRatio}\n{GetSamplesString()}");
								}
							}
						}
					}
				}
			});
			m_poll_thread.Start();
		}

		public DateTime GetSystemTime()
		{
			DateTime result = DateTime.UtcNow;
			string text = ((m_poll_thread == null) ? "" : ("-" + m_poll_thread.ManagedThreadId.ToString("x")));
			try
			{
				string path = m_file_root + "drlst" + text + ".bin";
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				File.WriteAllText(path, "");
				result = File.GetLastWriteTimeUtc(path);
				File.Delete(path);
			}
			catch (Exception ex)
			{
				Debug.Log("DRLACS> GetSystemTime Error\n" + ex.Message);
			}
			return result;
		}

		public void FixedUpdate()
		{
			m_clock_local += Time.fixedDeltaTime;
		}

		public void Clear()
		{
			m_clock_local = 0f;
			clockLocal = 0f;
			clockSystem = 0f;
			ratioSamples.Clear();
			avgRatio = 1f;
			cheat = false;
			cheatEver = false;
			cheatCount = 0;
			m_has_clear = true;
		}

		public void Handcap(int p_count)
		{
			for (int i = 0; i < p_count; i++)
			{
				ratioSamples.Add(1f);
			}
		}

		protected void OnDestroy()
		{
			string text = ((m_poll_thread == null) ? "" : ("-" + m_poll_thread.ManagedThreadId.ToString("x")));
			if (m_poll_thread != null && m_poll_thread.IsAlive)
			{
				m_poll_thread.Abort();
				m_poll_thread = null;
			}
			FileInfo fileInfo = null;
			fileInfo = new FileInfo(m_file_root + "drlst" + text + ".bin");
			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
			fileInfo = new FileInfo(m_file_root + "drlst.bin");
			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}
	}
}
