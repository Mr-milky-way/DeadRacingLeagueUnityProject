using System;
using System.Diagnostics;
using System.Threading;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ServerStateModel : Model<DRLApp>
	{
		private DateTime m_time;

		private byte[] m_time_data;

		private Thread m_clock_thread;

		private Stopwatch m_clock_thread_watch;

		public static bool DisableClockThread;

		public DataFlow data => AssertLocal<DataFlow>("data");

		public bool willNotifyTime
		{
			get
			{
				bool flag = false;
				try
				{
					return base.app.model.storage.state.ready;
				}
				catch
				{
					return false;
				}
			}
		}

		public DateTime time
		{
			get
			{
				if (m_time_data == null)
				{
					return DateTime.Now;
				}
				byte b = (byte)GetHashCode();
				byte[] array = new byte[m_time_data.Length];
				Array.Copy(m_time_data, array, array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] ^= b;
				}
				return Serialize.FromBytes<DateTime>(array);
			}
			set
			{
				byte b = (byte)GetHashCode();
				byte[] array = Serialize.ToBytes(value);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] ^= b;
				}
				m_time_data = array;
				m_time = value;
			}
		}

		public bool maintenance
		{
			get
			{
				if (!data.Contains("maintenance"))
				{
					return false;
				}
				return data.Get<string>("maintenance").ToLower() == "true";
			}
		}

		public DateTime GetTime()
		{
			return m_time;
		}

		public void NotifyTime()
		{
			if (willNotifyTime && !DisableClockThread)
			{
				RunClockThread();
				Notify("state.time@refresh");
				Activity.RunOnce(NotifyTime, 1f);
			}
		}

		protected void RunClockThread()
		{
			if (DisableClockThread || m_clock_thread != null)
			{
				return;
			}
			if (m_clock_thread_watch == null)
			{
				m_clock_thread_watch = new Stopwatch();
			}
			m_clock_thread = new Thread((ThreadStart)delegate
			{
				Stopwatch clock_thread_watch = m_clock_thread_watch;
				m_clock_thread_watch.Restart();
				while (!DisableClockThread)
				{
					m_time += clock_thread_watch.Elapsed;
					clock_thread_watch.Restart();
					Thread.Sleep(0);
				}
				clock_thread_watch.Stop();
				m_clock_thread = null;
			});
			m_clock_thread.Start();
		}
	}
}
