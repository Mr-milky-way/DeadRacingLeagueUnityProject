using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTime : View<DRLApp>
	{
		private static DateTime m_clock;

		private static bool m_clock_available;

		private ulong m834235a;

		private float mt = 25920000f;

		private ulong m834235b;

		private bool m_frame_lock;

		private Thread m_time_thread;

		private Thread m_clock_thread;

		private ulong m834235c;

		private ulong m834235d;

		private ulong m834235e;

		private ulong m834235f;

		private List<float> m_dt_ratios;

		private ulong m834235g;

		private bool m_has_init;

		private string m_sysfile_path;

		private DateTime m_start_time;

		private Activity m_clock_poll;

		public static DateTime clock => m_clock;

		public static DateTime serverClock => m_clock;

		public bool clockAvailable => m_clock_available;

		public float elapsed
		{
			get
			{
				return SerializedData.FloatDecode(m834235a, 0f, mt, 4);
			}
			set
			{
				m834235a = SerializedData.FloatEncode(value, 0f, mt, 4, 1, 14);
			}
		}

		public float deltaTime
		{
			get
			{
				return Mathf.Clamp(SerializedData.FloatDecode(m834235b, 0f, 5f, 4), 0f, 5f);
			}
			set
			{
				m834235b = SerializedData.FloatEncode(value, 0f, 5f, 4, 2, 10);
			}
		}

		public bool deltaTimeWarning
		{
			get
			{
				if (!(m_dt_ratio_avg >= 1.5f))
				{
					return m_dt_ratio_avg <= 0.8f;
				}
				return true;
			}
		}

		private float m_last_elapsed
		{
			get
			{
				return SerializedData.FloatDecode(m834235c, 0f, mt, 4);
			}
			set
			{
				m834235c = SerializedData.FloatEncode(value, 0f, mt, 4, 2, 10);
			}
		}

		private float m_unity_clock
		{
			get
			{
				return SerializedData.FloatDecode(m834235d, 0f, mt, 4);
			}
			set
			{
				m834235d = SerializedData.FloatEncode(value, 0f, mt, 4, 2, 10);
			}
		}

		private float m_safe_clock
		{
			get
			{
				return SerializedData.FloatDecode(m834235e, 0f, mt, 4);
			}
			set
			{
				m834235e = SerializedData.FloatEncode(value, 0f, mt, 4, 2, 10);
			}
		}

		private float m_diff_last
		{
			get
			{
				return SerializedData.FloatDecode(m834235f, 0f, mt, 4);
			}
			set
			{
				m834235f = SerializedData.FloatEncode(value, 0f, mt, 4, 2, 10);
			}
		}

		private float m_dt_ratio_avg
		{
			get
			{
				return SerializedData.FloatDecode(m834235g, 0f, mt, 4);
			}
			set
			{
				m834235g = SerializedData.FloatEncode(value, 0f, mt, 4, 2, 10);
			}
		}

		public static DateTime GetNistTime(DateTime p_default)
		{
			try
			{
				byte[] array = new byte[48];
				array[0] = 27;
				IPEndPoint remoteEP = new IPEndPoint(Dns.GetHostEntry("pool.ntp.org").AddressList[0], 123);
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
				{
					socket.Connect(remoteEP);
					socket.ReceiveTimeout = 3000;
					socket.Send(array);
					socket.Receive(array);
					socket.Close();
				}
				long x = BitConverter.ToUInt32(array, 40);
				ulong x2 = BitConverter.ToUInt32(array, 44);
				long num = SwapEndianness((ulong)x);
				x2 = SwapEndianness(x2);
				ulong num2 = (ulong)(num * 1000) + x2 * 1000 / 4294967296L;
				return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)num2);
			}
			catch (Exception)
			{
				return p_default;
			}
		}

		private static uint SwapEndianness(ulong x)
		{
			return (uint)(((x & 0xFF) << 24) + ((x & 0xFF00) << 8) + ((x & 0xFF0000) >> 8) + ((x & 0xFF000000u) >> 24));
		}

		public void ResetTimeAnalysis()
		{
			m_unity_clock = 0f;
			m_safe_clock = 0f;
			m_dt_ratio_avg = 0f;
			m_dt_ratios.Clear();
		}

		public DateTime GetSystemTime()
		{
			string text = m_sysfile_path + "drlst.bin";
			File.WriteAllText(text, "");
			FileInfo fileInfo = new FileInfo(text);
			DateTime creationTimeUtc = fileInfo.CreationTimeUtc;
			fileInfo.Delete();
			return creationTimeUtc;
		}

		public float GetSystemElapsedTime()
		{
			return (float)((GetSystemTime() - m_start_time).TotalMilliseconds / 1000.0);
		}

		protected void Awake()
		{
			m_sysfile_path = DRLPaths.Storage.stateRoot;
		}

		public void Initialize(bool p_use_nist_time)
		{
			UnityEngine.Debug.Log($"<color=#ff0>DRLTime> Initialize! use-nist[{p_use_nist_time}] sysfile-path[{m_sysfile_path}]</color>");
			if (!m_has_init)
			{
				elapsed = Time.time;
				m_last_elapsed = elapsed;
				m_dt_ratios = new List<float>();
				m_start_time = GetSystemTime();
				deltaTime = 0f;
				m_time_thread = new Thread(OnTimeThreadUpdate);
				m_time_thread.Priority = System.Threading.ThreadPriority.Highest;
				m_time_thread.Start();
				if (!m_clock_available)
				{
					m_clock = m_start_time;
				}
				if (p_use_nist_time)
				{
					m_clock_thread = new Thread(OnClockThreadUpdate);
					m_clock_thread.Priority = System.Threading.ThreadPriority.Lowest;
					m_clock_thread.Start();
				}
				else
				{
					OnClockPollUpdate();
				}
				m_has_init = true;
			}
		}

		protected void OnClockThreadUpdate()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (m_clock_thread != null)
			{
				if ((int)(stopwatch.ElapsedMilliseconds / 1000) < 60)
				{
					Thread.Sleep(0);
					continue;
				}
				stopwatch.Restart();
				m_clock = GetNistTime(m_clock);
				m_clock_available = true;
				Thread.Sleep(0);
			}
		}

		protected void OnClockPollUpdate()
		{
			if (!base.validContext)
			{
				return;
			}
			if (m_clock_poll != null)
			{
				m_clock_poll.Stop();
			}
			m_clock_poll = null;
			base.app.model.service.backend.ServerTime(delegate(DRLServiceResult p_result)
			{
				if (base.validContext)
				{
					bool num = p_result?.success ?? false;
					m_clock_available = true;
					m_clock_poll = this.TimerRunOnce(OnClockPollUpdate, 120f);
					if (!num)
					{
						UnityEngine.Debug.LogWarning("DRLTime> ServerTime Failed! / " + ((p_result == null) ? "" : p_result.message));
						m_clock = GetSystemTime();
					}
					else
					{
						Dictionary<string, string> data = p_result.GetData<Dictionary<string, string>>();
						if (data == null)
						{
							UnityEngine.Debug.LogWarning("DRLTime> ServerTime Data Parse Fail");
							m_clock = GetSystemTime();
						}
						else
						{
							string text = data["time"];
							text = text.Substring(0, text.LastIndexOf("-"));
							text = text.Replace("-", "/");
							text = text.Replace("T", " ");
							DateTime result = GetSystemTime();
							if (!DateTime.TryParseExact(text, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
							{
								UnityEngine.Debug.LogWarning("DRLTime> Invalid Date Format [" + text + "] expected [yyyy/MM/dd HH:mm:ss]");
								m_clock = GetSystemTime();
							}
							else
							{
								m_clock = result;
							}
						}
					}
				}
			});
		}

		protected void OnTimeThreadUpdate()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Start();
			while (m_time_thread != null)
			{
				if (m_clock_available)
				{
					m_clock += stopwatch2.Elapsed;
					stopwatch2.Restart();
				}
				if (m_frame_lock)
				{
					elapsed += (float)stopwatch.ElapsedMilliseconds * 0.001f;
					m_frame_lock = false;
					stopwatch.Restart();
				}
				Thread.Sleep(0);
			}
		}

		protected void LateUpdate()
		{
			if (m_has_init)
			{
				if (!m_frame_lock)
				{
					float num = elapsed - m_last_elapsed;
					m_last_elapsed = elapsed;
					deltaTime = num;
				}
				m_frame_lock = true;
				m_unity_clock += Time.deltaTime;
				m_safe_clock += deltaTime;
			}
		}

		protected void OnDestroy()
		{
			if (m_clock_thread != null && m_clock_thread.IsAlive)
			{
				m_clock_thread.Abort();
			}
			m_clock_thread = null;
			if (m_time_thread != null && m_time_thread.IsAlive)
			{
				m_time_thread.Abort();
			}
			m_time_thread = null;
		}
	}
}
