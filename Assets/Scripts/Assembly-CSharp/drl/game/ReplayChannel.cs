using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace drl.game
{
	public class ReplayChannel : ReplayStream
	{
		private int m_stride = 1;

		private int m_dimension = 1;

		public long offset;

		public System.IO.Compression.CompressionLevel compression;

		public string name { get; set; }

		public int stride
		{
			get
			{
				return m_stride;
			}
			set
			{
				m_stride = Mathf.Max(value, 1);
			}
		}

		public int dimension
		{
			get
			{
				return m_dimension;
			}
			set
			{
				m_dimension = Mathf.Max(value, 1);
			}
		}

		public long sampleLength
		{
			get
			{
				if (!base.valid)
				{
					return 0L;
				}
				return base.stream.Length / (stride * dimension);
			}
		}

		public long sample
		{
			get
			{
				if (!base.valid)
				{
					return 0L;
				}
				long num = base.stream.Position / (stride * dimension) - offset;
				if (num >= 0)
				{
					if (num < sampleLength)
					{
						return num;
					}
					return sampleLength - 1;
				}
				return 0L;
			}
			set
			{
				if (base.valid)
				{
					long num = value + offset;
					num = ((num < 0) ? 0 : ((num >= sampleLength) ? (sampleLength - 1) : num));
					base.stream.Position = num * (stride * dimension);
				}
			}
		}

		public void Compress()
		{
			ToZipStream(CompressionMode.Compress, compression);
		}

		public void Decompress()
		{
			ToZipStream(CompressionMode.Decompress, System.IO.Compression.CompressionLevel.NoCompression);
		}

		private void ToZipStream(CompressionMode p_mode, System.IO.Compression.CompressionLevel p_level)
		{
			if (!base.valid)
			{
				return;
			}
			Stream streamPool = ReplayStream.GetStreamPool();
			GZipStream gZipStream = null;
			switch (p_mode)
			{
			case CompressionMode.Compress:
				gZipStream = new GZipStream(streamPool, p_level, leaveOpen: true);
				break;
			case CompressionMode.Decompress:
				base.stream.Position = 0L;
				base.stream.CopyTo(streamPool, ReplayStream.CopyBufferLength);
				streamPool.Position = 0L;
				if (streamPool is FileStream)
				{
					((FileStream)streamPool).Flush(flushToDisk: true);
				}
				else
				{
					streamPool.Flush();
				}
				gZipStream = new GZipStream(streamPool, CompressionMode.Decompress, leaveOpen: true);
				break;
			}
			base.stream.Position = 0L;
			switch (p_mode)
			{
			case CompressionMode.Compress:
				base.stream.CopyTo(gZipStream, ReplayStream.CopyBufferLength);
				gZipStream.Flush();
				if (streamPool is FileStream)
				{
					((FileStream)streamPool).Flush(flushToDisk: true);
				}
				else
				{
					streamPool.Flush();
				}
				gZipStream.Close();
				base.stream.Position = 0L;
				base.stream.SetLength(0L);
				base.stream.Flush();
				streamPool.Position = 0L;
				streamPool.CopyTo(base.stream, ReplayStream.CopyBufferLength);
				break;
			case CompressionMode.Decompress:
				base.stream.SetLength(0L);
				gZipStream.CopyTo(base.stream, ReplayStream.CopyBufferLength);
				gZipStream.Close();
				break;
			}
			ReplayStream.SetMemoryPool(streamPool);
			Flush();
		}
	}
	public class ReplayChannel<T> : ReplayChannel
	{
		private T m_last_value;

		public void Write(T v)
		{
			if (base.valid)
			{
				OnWrite(v);
			}
		}

		public T Read()
		{
			if (!base.valid)
			{
				return default(T);
			}
			if (base.stream.Position < base.stream.Length)
			{
				return m_last_value = OnRead();
			}
			return m_last_value;
		}

		public T Evaluate(float p_ratio)
		{
			T v = Read();
			T v2 = Read();
			return OnEvaluate(v, v2, p_ratio);
		}

		protected virtual void OnWrite(T v)
		{
		}

		protected virtual T OnRead()
		{
			return default(T);
		}

		protected virtual T OnEvaluate(T v0, T v1, float r)
		{
			return v0;
		}
	}
}
