using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace drl.game
{
	public class ReplayStream
	{
		public static bool UseMemoryPool;

		protected static int CopyBufferLength;

		private static System.Random m_random;

		private static List<Stream> m_stream_pool;

		private static object m_stream_pool_lock;

		private static string m_replay_tmp_folder;

		private Stream m_stream;

		private BinaryWriter m_writer;

		private BinaryReader m_reader;

		public Stream stream => m_stream;

		public BinaryWriter writer
		{
			get
			{
				if (!valid)
				{
					return null;
				}
				if (m_writer == null)
				{
					m_writer = new BinaryWriter(stream);
				}
				return m_writer;
			}
		}

		public BinaryReader reader
		{
			get
			{
				if (!valid)
				{
					return null;
				}
				if (m_reader == null)
				{
					m_reader = new BinaryReader(stream);
				}
				return m_reader;
			}
		}

		public FileStream file
		{
			get
			{
				if (!valid)
				{
					return null;
				}
				if (!(stream is FileStream))
				{
					return null;
				}
				return (FileStream)stream;
			}
		}

		public MemoryStream memory
		{
			get
			{
				if (!valid)
				{
					return null;
				}
				if (!(stream is MemoryStream))
				{
					return null;
				}
				return (MemoryStream)stream;
			}
		}

		public long length
		{
			get
			{
				if (!valid)
				{
					return 0L;
				}
				return stream.Length;
			}
		}

		public bool valid => m_stream != null;

		static ReplayStream()
		{
			UseMemoryPool = false;
			CopyBufferLength = 8192;
			m_random = new System.Random();
			m_stream_pool_lock = new object();
			m_replay_tmp_folder = null;
			m_stream_pool = new List<Stream>();
		}

		public static Stream GetStreamPool()
		{
			Stream stream = null;
			lock (m_stream_pool_lock)
			{
				if (m_stream_pool.Count > 0)
				{
					while (m_stream_pool[0] == null)
					{
						m_stream_pool.RemoveAt(0);
						if (m_stream_pool.Count <= 0)
						{
							break;
						}
					}
				}
				if (m_stream_pool.Count <= 0)
				{
					stream = ((!UseMemoryPool) ? ((Stream)new FileStream(GetReplayTempFilePath("", "$pool-"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, CopyBufferLength)) : ((Stream)new MemoryStream()));
				}
				else
				{
					stream = m_stream_pool[0];
					stream.SetLength(0L);
					stream.Position = 0L;
					m_stream_pool.RemoveAt(0);
				}
			}
			return stream;
		}

		public static void SetMemoryPool(Stream p_stream)
		{
			lock (m_stream_pool_lock)
			{
				if (p_stream != null && !m_stream_pool.Contains(p_stream))
				{
					m_stream_pool.Add(p_stream);
				}
			}
		}

		public static string GetReplayTempFilePath(string p_folder = "", string p_prefix = "")
		{
			if (string.IsNullOrEmpty(m_replay_tmp_folder))
			{
				m_replay_tmp_folder = DRLPaths.Storage.replaysTemp;
			}
			string obj = (string.IsNullOrEmpty(p_folder) ? m_replay_tmp_folder : p_folder);
			string text = "";
			text += m_random.Next(0, 16777215).ToString("x6");
			text += m_random.Next(0, 16777215).ToString("x6");
			text += m_random.Next(0, 16777215).ToString("x6");
			text += m_random.Next(0, 16777215).ToString("x6");
			return obj + p_prefix + text;
		}

		public void Clear()
		{
			if (valid)
			{
				stream.Position = 0L;
				if (file != null)
				{
					file.SetLength(0L);
				}
			}
			OnClear();
		}

		public void Flush()
		{
			if (valid)
			{
				if (file != null)
				{
					file.Flush(flushToDisk: true);
				}
				else
				{
					stream.Flush();
				}
			}
		}

		public void Close()
		{
			if (valid)
			{
				stream.Close();
			}
		}

		protected virtual void OnClear()
		{
		}

		protected virtual void OnDestroy()
		{
		}

		public void Initialize(Stream p_stream)
		{
			if (!valid)
			{
				if (m_stream != null)
				{
					m_stream.Flush();
					m_stream.Close();
				}
				m_stream = p_stream;
			}
		}

		public void Initialize(string p_path)
		{
			if (!valid)
			{
				FileStream fileStream = new FileStream(p_path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, CopyBufferLength);
				fileStream.SetLength(0L);
				Initialize(fileStream);
			}
		}

		public void Destroy()
		{
			if (!valid)
			{
				return;
			}
			if (file != null)
			{
				file.Close();
				if (File.Exists(file.Name))
				{
					try
					{
						File.Delete(file.Name);
					}
					catch (Exception ex)
					{
						Debug.LogWarning("ReplayStream> Destroy / Error\n" + ex.Message);
					}
				}
			}
			else
			{
				m_stream.Close();
			}
			m_stream = null;
			OnDestroy();
		}
	}
}
