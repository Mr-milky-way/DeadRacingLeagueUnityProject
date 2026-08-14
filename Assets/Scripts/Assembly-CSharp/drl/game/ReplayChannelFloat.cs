using System.IO;
using UnityEngine;

namespace drl.game
{
	public class ReplayChannelFloat : ReplayChannel<float>
	{
		public float precision;

		private byte[] m_float_buffer = new byte[4];

		public ReplayChannelFloat()
		{
			base.stride = 4;
		}

		protected override void OnWrite(float v)
		{
			if (precision > 0f)
			{
				v = (float)(int)(v * precision + 0.5f) / precision;
			}
			UIntToFloat uIntToFloat = new UIntToFloat
			{
				Value = v
			};
			m_float_buffer[0] = uIntToFloat.Byte0;
			m_float_buffer[1] = uIntToFloat.Byte1;
			m_float_buffer[2] = uIntToFloat.Byte2;
			m_float_buffer[3] = uIntToFloat.Byte3;
			base.stream.Write(m_float_buffer, 0, 4);
		}

		protected override float OnRead()
		{
			UIntToFloat uIntToFloat = default(UIntToFloat);
			base.stream.Read(m_float_buffer, 0, 4);
			uIntToFloat.Byte0 = m_float_buffer[0];
			uIntToFloat.Byte1 = m_float_buffer[1];
			uIntToFloat.Byte2 = m_float_buffer[2];
			uIntToFloat.Byte3 = m_float_buffer[3];
			return uIntToFloat.Value;
		}

		public void ToDelta()
		{
			DeltaStream(p_from: false);
		}

		public void FromDelta()
		{
			DeltaStream(p_from: true);
		}

		private void DeltaStream(bool p_from)
		{
			if (!base.valid)
			{
				return;
			}
			Stream streamPool = ReplayStream.GetStreamPool();
			bool flag = true;
			float num = 0f;
			long num2 = base.stream.Length;
			UIntToFloat uIntToFloat = default(UIntToFloat);
			int num3 = m_float_buffer.Length;
			base.stream.Position = 0L;
			while (base.stream.Position < num2)
			{
				base.stream.Read(m_float_buffer, 0, num3);
				for (int i = 0; i < num3 / 4; i++)
				{
					int num4 = i * 4;
					uIntToFloat.Byte0 = m_float_buffer[num4];
					uIntToFloat.Byte1 = m_float_buffer[num4 + 1];
					uIntToFloat.Byte2 = m_float_buffer[num4 + 2];
					uIntToFloat.Byte3 = m_float_buffer[num4 + 3];
					float value = uIntToFloat.Value;
					float num5 = value;
					if (!flag)
					{
						num5 = (p_from ? (num + value) : (value - num));
					}
					uIntToFloat.Value = num5;
					m_float_buffer[num4] = uIntToFloat.Byte0;
					m_float_buffer[num4 + 1] = uIntToFloat.Byte1;
					m_float_buffer[num4 + 2] = uIntToFloat.Byte2;
					m_float_buffer[num4 + 3] = uIntToFloat.Byte3;
					num = (p_from ? num5 : value);
					flag = false;
				}
				streamPool.Write(m_float_buffer, 0, num3);
			}
			if (streamPool is FileStream)
			{
				((FileStream)streamPool).Flush(flushToDisk: true);
			}
			else
			{
				streamPool.Flush();
			}
			base.stream.Position = 0L;
			streamPool.Position = 0L;
			streamPool.CopyTo(base.stream, ReplayStream.CopyBufferLength);
			ReplayStream.SetMemoryPool(streamPool);
			Flush();
		}

		protected override float OnEvaluate(float v0, float v1, float r)
		{
			return Mathf.Lerp(v0, v1, r);
		}
	}
}
