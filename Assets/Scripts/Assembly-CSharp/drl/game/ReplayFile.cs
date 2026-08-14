using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using drl.sim;
using drl.sim.rci;

namespace drl.game
{
	public class ReplayFile : ReplayStream
	{
		public static bool EnableVersion2 = true;

		public ReplayHeader header;

		public List<ReplayChannel> channels;

		private Dictionary<string, int> m_channel_lut;

		private float[] m_sample_time_lut;

		private long m_last_sample;

		private ReplayChannelFloat m_channel_time;

		private static string[] m_csv_channels = new string[15]
		{
			"time", "drone-px", "drone-py", "drone-pz", "drone-qx", "drone-qy", "drone-qz", "drone-qw", "drone-vx", "drone-vy",
			"drone-vz", "input-y", "input-t", "input-p", "input-r"
		};

		public float duration
		{
			get
			{
				if (m_sample_time_lut == null)
				{
					return 0f;
				}
				if (m_sample_time_lut.Length < 2)
				{
					return 0f;
				}
				float num = m_sample_time_lut[0];
				return m_sample_time_lut[m_sample_time_lut.Length - 1] - num;
			}
		}

		public static ReplayFile FromBytes(byte[] p_data)
		{
			Stream streamPool = ReplayStream.GetStreamPool();
			streamPool.Write(p_data, 0, p_data.Length);
			if (streamPool is FileStream)
			{
				((FileStream)streamPool).Flush(flushToDisk: true);
			}
			else
			{
				streamPool.Flush();
			}
			streamPool.Position = 0L;
			ReplayFile replayFile = new ReplayFile();
			replayFile.Deserialize(streamPool);
			ReplayStream.SetMemoryPool(streamPool);
			return replayFile;
		}

		public ReplayFile()
		{
			channels = new List<ReplayChannel>();
			m_channel_lut = new Dictionary<string, int>();
			header = new ReplayHeader();
		}

		public void Serialize()
		{
			if (base.stream == null)
			{
				Debug.LogWarning("ReplayFile> Serialize / Stream is <null>");
				return;
			}
			if (!base.stream.CanWrite)
			{
				Debug.LogWarning("ReplayFile> Serialize / Stream can't be used!");
				return;
			}
			Seek(0L);
			bool compressed = header.compressed;
			header.Serialize();
			header.Flush();
			for (int i = 0; i < channels.Count; i++)
			{
				ReplayChannel replayChannel = channels[i];
				if (replayChannel.stream.Length > 0)
				{
					replayChannel.Flush();
					ReplayChannelFloat replayChannelFloat = (ReplayChannelFloat)replayChannel;
					if (compressed)
					{
						replayChannelFloat?.ToDelta();
						replayChannel.Compress();
					}
				}
			}
			StreamWriter streamWriter = new StreamWriter(base.stream);
			header.stream.Position = 0L;
			header.stream.CopyTo(base.stream, ReplayStream.CopyBufferLength);
			base.stream.WriteByte(0);
			for (int j = 0; j < channels.Count; j++)
			{
				ReplayChannel replayChannel2 = channels[j];
				if (replayChannel2.stream.Length > 0)
				{
					streamWriter.Write($"\n{replayChannel2.name},{replayChannel2.stream.Length},{replayChannel2.stride},{replayChannel2.dimension}\n");
					streamWriter.Flush();
					replayChannel2.stream.Position = 0L;
					replayChannel2.stream.CopyTo(base.stream, ReplayStream.CopyBufferLength);
				}
			}
			header.Close();
			for (int k = 0; k < channels.Count; k++)
			{
				channels[k].Close();
			}
			Flush();
			if (base.file != null)
			{
				Close();
			}
			header.Destroy();
			for (int l = 0; l < channels.Count; l++)
			{
				channels[l].Destroy();
			}
			channels.Clear();
		}

		public void Deserialize(string p_path)
		{
			if (!File.Exists(p_path))
			{
				Debug.LogWarning("ReplayFile> File [" + p_path + "] not found!");
				return;
			}
			FileStream fileStream = new FileStream(p_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			Deserialize(fileStream);
			fileStream.Close();
		}

		public void Deserialize(Stream p_stream)
		{
			if (p_stream == null)
			{
				return;
			}
			long num = p_stream.Length;
			header.Initialize(ReplayStream.GetReplayTempFilePath("", "header_"));
			StreamWriter streamWriter = new StreamWriter(header.stream);
			while (p_stream.Position < num)
			{
				byte b = (byte)p_stream.ReadByte();
				if (b <= 0)
				{
					break;
				}
				streamWriter.Write((char)b);
			}
			streamWriter.Flush();
			header.Flush();
			header.Deserialize();
			bool compressed = header.compressed;
			StringBuilder stringBuilder = new StringBuilder();
			string p_name = "";
			long result = 0L;
			int result2 = 0;
			int result3 = 0;
			int num2 = 0;
			while (p_stream.Position < num)
			{
				int num3 = 0;
				stringBuilder.Clear();
				num2 = 0;
				while (p_stream.Position < num && num3 < 2)
				{
					char c = (char)p_stream.ReadByte();
					if (c == '\n' || c == ',')
					{
						if (c == '\n')
						{
							num3++;
							if (num3 <= 1)
							{
								continue;
							}
						}
						string text = stringBuilder.ToString();
						switch (num2)
						{
						case 0:
							p_name = text;
							break;
						case 1:
							long.TryParse(text, out result);
							break;
						case 2:
							int.TryParse(text, out result2);
							break;
						case 3:
							int.TryParse(text, out result3);
							break;
						}
						num2++;
						stringBuilder.Clear();
					}
					else
					{
						stringBuilder.Append(c);
					}
				}
				ReplayChannelFloat replayChannelFloat = GetChannel<ReplayChannelFloat>(p_name);
				if (replayChannelFloat == null)
				{
					replayChannelFloat = AddChannel<ReplayChannelFloat>(p_name);
				}
				replayChannelFloat.Clear();
				if (result2 > 0)
				{
					replayChannelFloat.stride = result2;
				}
				if (result3 > 0)
				{
					replayChannelFloat.dimension = result3;
				}
				while (p_stream.Position < num)
				{
					replayChannelFloat.stream.WriteByte((byte)p_stream.ReadByte());
					if (replayChannelFloat.stream.Position >= result)
					{
						break;
					}
				}
				streamWriter.Flush();
				replayChannelFloat.Flush();
				if (compressed)
				{
					replayChannelFloat.Decompress();
					replayChannelFloat.FromDelta();
				}
			}
			ReplayChannelFloat channel = GetChannel<ReplayChannelFloat>("time");
			if (channel != null && channel.valid)
			{
				channel.sample = 0L;
				if (m_sample_time_lut == null)
				{
					m_sample_time_lut = new float[channel.sampleLength];
				}
				if (m_sample_time_lut.Length != channel.sampleLength)
				{
					Array.Resize(ref m_sample_time_lut, (int)channel.sampleLength);
				}
				for (int i = 0; i < m_sample_time_lut.Length; i++)
				{
					m_sample_time_lut[i] = channel.reader.ReadSingle();
				}
				channel.sample = 0L;
			}
		}

		public void ToCSV(Stream p_stream)
		{
			if (m_sample_time_lut == null || m_sample_time_lut.Length == 0)
			{
				Debug.LogWarning("ReplayFile> ToCSV / There is no warmed up data after deserialization");
				return;
			}
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberDecimalSeparator = ".";
			StreamWriter streamWriter = new StreamWriter(p_stream);
			ReplayHeader replayHeader = header;
			streamWriter.Write("#player-id,player-name,player-color,map-guid,track-guid,custom-map-guid,controller-type,camera-tilt,camera-fov,race-time");
			streamWriter.Write("\n");
			streamWriter.Write(replayHeader.playerId + ",");
			streamWriter.Write(replayHeader.profileName.ToUpper() + ",");
			streamWriter.Write("0x" + replayHeader.profileColorHex + ",");
			streamWriter.Write(replayHeader.mapGUID + ",");
			streamWriter.Write(replayHeader.trackGUID + ",");
			streamWriter.Write(replayHeader.customMapGUID + ",");
			streamWriter.Write($"{replayHeader.controllerTypeFlag},");
			streamWriter.Write(replayHeader.cameraTilt.ToString(numberFormatInfo) + ",");
			streamWriter.Write(replayHeader.cameraFOV.ToString(numberFormatInfo) + ",");
			streamWriter.Write(replayHeader.raceTime.ToString(numberFormatInfo) ?? "");
			streamWriter.Write("\n");
			streamWriter.Write("#event-index,type,time,pos-x,pos-y,pos-z,data-0,data-1,data-3");
			streamWriter.Write("\n");
			List<ReplayEvent> events = replayHeader.events;
			for (int i = 0; i < events.Count; i++)
			{
				ReplayEvent replayEvent = events[i];
				streamWriter.Write($"{i},");
				streamWriter.Write($"{replayEvent.type},");
				streamWriter.Write(replayEvent.time.ToString(numberFormatInfo) + ",");
				streamWriter.Write(replayEvent.position.x.ToString(numberFormatInfo) + ",");
				streamWriter.Write(replayEvent.position.y.ToString(numberFormatInfo) + ",");
				streamWriter.Write(replayEvent.position.z.ToString(numberFormatInfo) + ",");
				object[] data = replayEvent.data;
				string text = ((data.Length >= 1) ? $"{data[0]}" : "");
				streamWriter.Write(text + ",");
				text = ((data.Length >= 2) ? $"{data[1]}" : "");
				streamWriter.Write(text + ",");
				text = ((data.Length >= 3) ? $"{data[2]}" : "");
				streamWriter.Write(text + ",");
				text = ((data.Length >= 4) ? $"{data[3]}" : "");
				streamWriter.Write(text ?? "");
				if (i < events.Count - 1)
				{
					streamWriter.Write("\n");
				}
			}
			streamWriter.Write("\n");
			streamWriter.Write("#sample-index,time-seconds,pos-x,pos-y,pos-z,rot-x,rot-y,rot-z,rot-w,vel-x,vel-y,vel-z,yaw,throttle,pitch,roll");
			streamWriter.Write("\n");
			int num = m_sample_time_lut.Length;
			for (int j = 0; j < num; j++)
			{
				Seek(j);
				streamWriter.Write($"{j}");
				if (m_csv_channels.Length != 0)
				{
					streamWriter.Write(",");
				}
				for (int k = 0; k < m_csv_channels.Length; k++)
				{
					streamWriter.Write(ReadFloat(m_csv_channels[k]).ToString(numberFormatInfo));
					if (k < m_csv_channels.Length - 1)
					{
						streamWriter.Write(",");
					}
				}
				streamWriter.Write("\n");
			}
			streamWriter.Flush();
			if (p_stream is FileStream)
			{
				((FileStream)p_stream).Flush(flushToDisk: true);
			}
		}

		public void ClearChannels()
		{
			for (int i = 0; i < channels.Count; i++)
			{
				channels[i].Clear();
			}
			channels.Clear();
			header.Clear();
		}

		public T GetChannel<T>(string p_name) where T : ReplayChannel
		{
			int num = (m_channel_lut.ContainsKey(p_name) ? m_channel_lut[p_name] : (-1));
			if (num < 0)
			{
				return null;
			}
			if (num >= channels.Count)
			{
				return null;
			}
			return (T)channels[num];
		}

		public T GetChannel<T>(string p_name, int p_index) where T : ReplayChannel
		{
			return GetChannel<T>($"{p_name}@{p_index}");
		}

		public T AddChannel<T>(string p_name) where T : ReplayChannel, new()
		{
			T channel = GetChannel<T>(p_name);
			if (channel != null)
			{
				return null;
			}
			channel = new T();
			if (ReplayStream.UseMemoryPool)
			{
				channel.Initialize(new MemoryStream());
			}
			else
			{
				channel.Initialize(ReplayStream.GetReplayTempFilePath("", p_name + "_"));
			}
			channel.name = p_name;
			m_channel_lut[p_name] = channels.Count;
			channels.Add(channel);
			if (p_name == "time")
			{
				m_channel_time = (ReplayChannelFloat)(object)channel;
			}
			return channel;
		}

		public T AddChannel<T>(string p_name, int p_index) where T : ReplayChannel, new()
		{
			return AddChannel<T>($"{p_name}@{p_index}");
		}

		public T AddChannel<T>(string p_name, Stream p_stream) where T : ReplayChannel, new()
		{
			T channel = GetChannel<T>(p_name);
			if (channel != null)
			{
				return null;
			}
			channel = new T();
			channel.Initialize(p_stream);
			channel.name = p_name;
			m_channel_lut[p_name] = channels.Count;
			channels.Add(channel);
			if (p_name == "time")
			{
				m_channel_time = (ReplayChannelFloat)(object)channel;
			}
			return channel;
		}

		public T AddChannel<T>(string p_name, Stream p_stream, int p_index) where T : ReplayChannel, new()
		{
			return AddChannel<T>($"{p_name}@{p_index}", p_stream);
		}

		public void AddSimulatorChannels(bool p_all = false, int p_crash_nodes = 0)
		{
			string[] array = (p_all ? ReplayChannelIds.ChannelAll : ReplayChannelIds.ChannelBasic);
			for (int i = 0; i < array.Length; i++)
			{
				ReplayChannelFloat replayChannelFloat = AddChannel<ReplayChannelFloat>(array[i]);
				switch (replayChannelFloat.name)
				{
				case "time":
					replayChannelFloat.precision = 1000f;
					break;
				case "drone-px":
				case "drone-py":
				case "drone-pz":
					replayChannelFloat.precision = 1000f;
					break;
				case "input-y":
				case "input-t":
				case "input-r":
				case "input-p":
					replayChannelFloat.precision = 100f;
					break;
				case "drone-rpm0":
				case "drone-rpm1":
				case "drone-rpm2":
				case "drone-rpm3":
					replayChannelFloat.precision = 100f;
					break;
				case "drone-vx":
				case "drone-vy":
				case "drone-vz":
					replayChannelFloat.precision = 10f;
					break;
				}
			}
			for (int j = 0; j < p_crash_nodes; j++)
			{
				for (int k = 0; k < ReplayChannelIds.DroneParts.Length; k++)
				{
					string text = ReplayChannelIds.DroneParts[k];
					ReplayChannelFloat replayChannelFloat2 = AddChannel<ReplayChannelFloat>(text, j);
					switch (text)
					{
					case "drone-part":
						replayChannelFloat2.precision = 1000f;
						replayChannelFloat2.dimension = 7;
						break;
					case "drone-part-px":
					case "drone-part-py":
					case "drone-part-pz":
						replayChannelFloat2.precision = 200f;
						break;
					case "drone-part-qx":
					case "drone-part-qy":
					case "drone-part-qz":
					case "drone-part-qw":
						replayChannelFloat2.precision = 1000f;
						break;
					}
				}
			}
		}

		public void SetSimulatorCrashChannelsOffset(long p_offset)
		{
			for (int i = 0; i < ReplayChannelIds.DroneParts.Length; i++)
			{
				string value = ReplayChannelIds.DroneParts[i];
				for (int j = 0; j < channels.Count; j++)
				{
					ReplayChannel replayChannel = channels[j];
					if (replayChannel.name.Contains(value))
					{
						replayChannel.offset = p_offset;
					}
				}
			}
		}

		public void SetChannelsCompression(System.IO.Compression.CompressionLevel p_compression)
		{
			for (int i = 0; i < channels.Count; i++)
			{
				channels[i].compression = p_compression;
			}
		}

		public float Seek(float p_time)
		{
			if (GetChannel<ReplayChannelFloat>("time") == null)
			{
				return 0f;
			}
			float[] sample_time_lut = m_sample_time_lut;
			if (sample_time_lut == null)
			{
				return 0f;
			}
			if (sample_time_lut.Length == 0)
			{
				return 0f;
			}
			long last_sample = m_last_sample;
			long num = sample_time_lut.Length;
			last_sample = ((last_sample < 0) ? 0 : ((last_sample >= num) ? (num - 1) : last_sample));
			float num2 = sample_time_lut[last_sample];
			int num3 = ((!(p_time < num2)) ? 1 : (-1));
			float result = 0f;
			while (true)
			{
				long num4 = last_sample;
				num4 = ((num4 < 0) ? 0 : ((num4 >= num) ? (num - 1) : num4));
				long num5 = last_sample + 1;
				num5 = ((num5 < 0) ? 0 : ((num5 >= num) ? (num - 1) : num5));
				float num6 = sample_time_lut[num4];
				float num7 = sample_time_lut[num5];
				if (p_time >= num6 && p_time < num7)
				{
					last_sample = num4;
					float num8 = Mathf.Abs(num7 - num6);
					result = ((num8 <= 0f) ? 0f : ((p_time - num6) / num8));
					break;
				}
				last_sample += num3;
				if (last_sample < 0)
				{
					last_sample = 0L;
					break;
				}
				if (last_sample >= num)
				{
					last_sample = num - 1;
					break;
				}
			}
			Seek(last_sample);
			return result;
		}

		public void Seek(long p_sample)
		{
			for (int i = 0; i < channels.Count; i++)
			{
				ReplayChannel replayChannel = channels[i];
				if (replayChannel.valid && replayChannel.sampleLength > 0 && replayChannel.sample != p_sample)
				{
					replayChannel.sample = p_sample;
				}
			}
			m_last_sample = p_sample;
		}

		public void SeekOffset(long p_offset)
		{
			if (p_offset == 0L)
			{
				return;
			}
			for (int i = 0; i < channels.Count; i++)
			{
				ReplayChannel replayChannel = channels[i];
				if (replayChannel.valid && replayChannel.sampleLength > 0)
				{
					long num = replayChannel.sample + p_offset;
					if (replayChannel.sample != num)
					{
						replayChannel.sample = num;
					}
				}
			}
		}

		public float ReadFloat(string p_channel)
		{
			return GetChannel<ReplayChannelFloat>(p_channel)?.Read() ?? 0f;
		}

		public float EvaluateFloat(string p_channel, float p_ratio)
		{
			return GetChannel<ReplayChannelFloat>(p_channel)?.Evaluate(p_ratio) ?? 0f;
		}

		public float ReadFloat(string p_channel, int p_index)
		{
			return ReadFloat($"{p_channel}@{p_index}");
		}

		public float EvaluateFloat(string p_channel, int p_index, float p_ratio)
		{
			return EvaluateFloat($"{p_channel}@{p_index}", p_ratio);
		}

		public Vector3 ReadVector3(string[] p_channels)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 3) : 0);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = ReadFloat(p_channels[i]);
			}
			return zero;
		}

		public Vector3 ReadVector3(string[] p_channels, int p_index)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 3) : 0);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = ReadFloat(p_channels[i], p_index);
			}
			return zero;
		}

		public Vector3 EvaluateVector3(string[] p_channels, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 3) : 0);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = EvaluateFloat(p_channels[i], p_ratio);
			}
			return zero;
		}

		public Vector3 EvaluateVector3(string[] p_channels, int p_index, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 3) : 0);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = EvaluateFloat(p_channels[i], p_index, p_ratio);
			}
			return zero;
		}

		public Vector4 ReadVector4(string[] p_channels)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Vector4 zero = Vector4.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = ReadFloat(p_channels[i]);
			}
			return zero;
		}

		public Vector4 ReadVector4(string[] p_channels, int p_index)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Vector4 zero = Vector4.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = ReadFloat(p_channels[i], p_index);
			}
			return zero;
		}

		public Vector4 EvaluateVector4(string[] p_channels, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Vector4 zero = Vector4.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = EvaluateFloat(p_channels[i], p_ratio);
			}
			return zero;
		}

		public Vector4 EvaluateVector4(string[] p_channels, int p_index, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Vector4 zero = Vector4.zero;
			for (int i = 0; i < num; i++)
			{
				zero[i] = EvaluateFloat(p_channels[i], p_index, p_ratio);
			}
			return zero;
		}

		public Quaternion ReadQuaternion(string[] p_channels)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Quaternion identity = Quaternion.identity;
			for (int i = 0; i < num; i++)
			{
				identity[i] = ReadFloat(p_channels[i]);
			}
			return identity;
		}

		public Quaternion ReadQuaternion(string[] p_channels, int p_index)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Quaternion identity = Quaternion.identity;
			for (int i = 0; i < num; i++)
			{
				identity[i] = ReadFloat(p_channels[i], p_index);
			}
			return identity;
		}

		public Quaternion EvaluateQuaternion(string[] p_channels, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Quaternion identity = Quaternion.identity;
			for (int i = 0; i < num; i++)
			{
				identity[i] = EvaluateFloat(p_channels[i], p_ratio);
			}
			return identity;
		}

		public Quaternion EvaluateQuaternion(string[] p_channels, int p_index, float p_ratio)
		{
			int num = ((p_channels != null) ? Mathf.Min(p_channels.Length, 4) : 0);
			Quaternion identity = Quaternion.identity;
			for (int i = 0; i < num; i++)
			{
				identity[i] = EvaluateFloat(p_channels[i], p_index, p_ratio);
			}
			return identity;
		}

		public float[] ReadFloatArray(string p_channel, float[] p_buffer, int p_count = 0)
		{
			int num = ((p_count > 0) ? p_count : ((p_buffer != null) ? p_buffer.Length : 0));
			float[] array = ((p_buffer == null) ? new float[num] : p_buffer);
			num = Mathf.Min(array.Length, num);
			for (int i = 0; i < num; i++)
			{
				array[i] = ReadFloat(p_channel);
			}
			return array;
		}

		public float[] ReadFloatArray(string p_channel, int p_index, float[] p_buffer, int p_count = 0)
		{
			int num = ((p_count > 0) ? p_count : ((p_buffer != null) ? p_buffer.Length : 0));
			float[] array = ((p_buffer == null) ? new float[num] : p_buffer);
			num = Mathf.Min(array.Length, num);
			for (int i = 0; i < num; i++)
			{
				array[i] = ReadFloat(p_channel, p_index);
			}
			return array;
		}

		public float[] EvaluateFloatArray(string p_channel, float p_ratio, float[] p_buffer, int p_count = 0)
		{
			int num = ((p_count > 0) ? p_count : ((p_buffer != null) ? p_buffer.Length : 0));
			float[] array = ((p_buffer == null) ? new float[num] : p_buffer);
			num = Mathf.Min(array.Length, num);
			for (int i = 0; i < num; i++)
			{
				array[i] = EvaluateFloat(p_channel, p_ratio);
			}
			return array;
		}

		public float[] EvaluateFloatArray(string p_channel, int p_index, float p_ratio, float[] p_buffer, int p_count = 0)
		{
			int num = ((p_count > 0) ? p_count : ((p_buffer != null) ? p_buffer.Length : 0));
			float[] array = ((p_buffer == null) ? new float[num] : p_buffer);
			num = Mathf.Min(array.Length, num);
			for (int i = 0; i < num; i++)
			{
				array[i] = EvaluateFloat(p_channel, p_index, p_ratio);
			}
			return array;
		}

		public void Write(float p_time, Drone p_drone)
		{
			Vector3 position = p_drone.position;
			Quaternion rotation = p_drone.transform.rotation;
			Vector4 vector = new Vector4(RCI.GetRawAxis(RawAxis.LeftStickX), RCI.GetRawAxis(RawAxis.LeftStickY), RCI.GetRawAxis(RawAxis.RightStickX), RCI.GetRawAxis(RawAxis.RightStickY));
			DroneRigidbody droneRigidbody = (p_drone.hasRigidbody ? p_drone.rigidbody : null);
			Vector3 vector2 = (droneRigidbody ? droneRigidbody.rb.velocity : Vector3.zero);
			float[] array = ((!p_drone.hasBody) ? null : (p_drone.body.hasFrame ? p_drone.body.frame.GetRPMRatios() : null));
			DroneFlightController droneFlightController = (p_drone.hasFc ? p_drone.fc : null);
			Vector3 vector3 = (droneRigidbody ? droneRigidbody.currentDragFactors : Vector3.zero);
			Vector3 vector4 = (droneRigidbody ? droneRigidbody.currentDragForce : Vector3.zero);
			float[] array2 = (droneRigidbody ? droneRigidbody.currentThrust : null);
			if ((bool)droneRigidbody)
			{
				_ = droneRigidbody.currentTorque;
			}
			float v = 0f;
			float v2 = 0f;
			float v3 = 0f;
			if ((bool)droneFlightController && (bool)droneFlightController.gameObject && droneFlightController.processes != null)
			{
				for (int i = 0; i < droneFlightController.processes.Count; i++)
				{
					FCProcess fCProcess = droneFlightController.processes[i];
					if ((bool)fCProcess && (bool)fCProcess.gameObject)
					{
						switch (fCProcess.nameLower)
						{
						case "yaw":
							v = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						case "pitch":
							v2 = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						case "roll":
							v3 = ((fCProcess.pid == null) ? 0f : fCProcess.pid.control);
							break;
						}
					}
				}
			}
			for (int j = 0; j < channels.Count; j++)
			{
				ReplayChannel replayChannel = channels[j];
				if (!replayChannel.valid)
				{
					continue;
				}
				ReplayChannelFloat replayChannelFloat = (ReplayChannelFloat)replayChannel;
				switch (replayChannel.name)
				{
				case "time":
					replayChannelFloat.Write(p_time);
					break;
				case "drone-px":
					replayChannelFloat.Write(position.x);
					break;
				case "drone-py":
					replayChannelFloat.Write(position.y);
					break;
				case "drone-pz":
					replayChannelFloat.Write(position.z);
					break;
				case "drone-qx":
					replayChannelFloat.Write(rotation.x);
					break;
				case "drone-qy":
					replayChannelFloat.Write(rotation.y);
					break;
				case "drone-qz":
					replayChannelFloat.Write(rotation.z);
					break;
				case "drone-qw":
					replayChannelFloat.Write(rotation.w);
					break;
				case "input-y":
					replayChannelFloat.Write(vector.x);
					break;
				case "input-t":
					replayChannelFloat.Write(vector.y);
					break;
				case "input-r":
					replayChannelFloat.Write(vector.z);
					break;
				case "input-p":
					replayChannelFloat.Write(vector.w);
					break;
				case "drone-vx":
					replayChannelFloat.Write(vector2.x);
					break;
				case "drone-vy":
					replayChannelFloat.Write(vector2.y);
					break;
				case "drone-vz":
					replayChannelFloat.Write(vector2.z);
					break;
				case "drone-rpm0":
					if (array != null)
					{
						replayChannelFloat.Write(array[0]);
					}
					break;
				case "drone-rpm1":
					if (array != null)
					{
						replayChannelFloat.Write(array[1]);
					}
					break;
				case "drone-rpm2":
					if (array != null)
					{
						replayChannelFloat.Write(array[2]);
					}
					break;
				case "drone-rpm3":
					if (array != null)
					{
						replayChannelFloat.Write(array[3]);
					}
					break;
				case "drone-pid-y":
					replayChannelFloat.Write(v);
					break;
				case "drone-pid-p":
					replayChannelFloat.Write(v2);
					break;
				case "drone-pid-r":
					replayChannelFloat.Write(v3);
					break;
				case "drone-drag-x":
					replayChannelFloat.Write(vector3.x);
					break;
				case "drone-drag-y":
					replayChannelFloat.Write(vector3.y);
					break;
				case "drone-drag-z":
					replayChannelFloat.Write(vector3.z);
					break;
				case "drone-drag-fx":
					replayChannelFloat.Write(vector4.x);
					break;
				case "drone-drag-fy":
					replayChannelFloat.Write(vector4.y);
					break;
				case "drone-drag-fz":
					replayChannelFloat.Write(vector4.z);
					break;
				case "drone-thrust0":
					if (array2 != null)
					{
						replayChannelFloat.Write(array2[0]);
					}
					break;
				case "drone-thrust1":
					if (array2 != null)
					{
						replayChannelFloat.Write(array2[1]);
					}
					break;
				case "drone-thrust2":
					if (array2 != null)
					{
						replayChannelFloat.Write(array2[2]);
					}
					break;
				case "drone-thrust3":
					if (array2 != null)
					{
						replayChannelFloat.Write(array2[3]);
					}
					break;
				}
			}
			CrashData crashData = p_drone.crashData;
			if (crashData == null || !crashData.isBroken)
			{
				return;
			}
			DroneCrashBody crash = p_drone.body.frame.crash;
			List<DroneCrashNode> list = (crash ? crash.nodes : null);
			int num = ((list != null) ? (list.Count + 1) : 0);
			for (int k = 0; k < num; k++)
			{
				DroneCrashNode droneCrashNode = ((k <= 0) ? null : list[k - 1]);
				Transform transform = ((k <= 0) ? crash.transform : droneCrashNode.transform);
				if (transform == null)
				{
					continue;
				}
				Vector3 position2 = transform.position;
				Quaternion rotation2 = transform.rotation;
				for (int l = 0; l < ReplayChannelIds.DroneParts.Length; l++)
				{
					string text = ReplayChannelIds.DroneParts[l];
					ReplayChannelFloat channel = GetChannel<ReplayChannelFloat>(text, k);
					if (channel != null)
					{
						switch (text)
						{
						case "drone-part":
							channel.Write(position2.x);
							channel.Write(position2.y);
							channel.Write(position2.z);
							channel.Write(rotation2.x);
							channel.Write(rotation2.y);
							channel.Write(rotation2.z);
							channel.Write(rotation2.w);
							break;
						case "drone-part-px":
							channel.Write(position2.x);
							break;
						case "drone-part-py":
							channel.Write(position2.y);
							break;
						case "drone-part-pz":
							channel.Write(position2.z);
							break;
						case "drone-part-qx":
							channel.Write(rotation2.x);
							break;
						case "drone-part-qy":
							channel.Write(rotation2.y);
							break;
						case "drone-part-qz":
							channel.Write(rotation2.z);
							break;
						case "drone-part-qw":
							channel.Write(rotation2.w);
							break;
						}
					}
				}
			}
		}

		public void Write(string p_channel, float p_value)
		{
			GetChannel<ReplayChannelFloat>(p_channel)?.Write(p_value);
		}

		public void Write(string[] p_channels, Vector3 p_value)
		{
			int num = ((p_channels.Length < 3) ? p_channels.Length : 3);
			for (int i = 0; i < num; i++)
			{
				Write(p_channels[i], p_value[i]);
			}
		}

		public void Write(string[] p_channels, Vector4 p_value)
		{
			int num = ((p_channels.Length < 4) ? p_channels.Length : 4);
			for (int i = 0; i < num; i++)
			{
				Write(p_channels[i], p_value[i]);
			}
		}

		public void Write(string[] p_channels, Quaternion p_value)
		{
			int num = ((p_channels.Length < 4) ? p_channels.Length : 4);
			for (int i = 0; i < num; i++)
			{
				Write(p_channels[i], p_value[i]);
			}
		}

		public void Write(string[] p_channels, float[] p_value)
		{
			int num = ((p_channels.Length < p_value.Length) ? p_channels.Length : p_value.Length);
			for (int i = 0; i < num; i++)
			{
				Write(p_channels[i], p_value[i]);
			}
		}

		public void WriteTime(float p_time)
		{
			Write("time", p_time);
		}

		public void PushEvent(ReplayEventType p_type, float p_time, Vector3 p_position, params object[] p_data)
		{
			List<ReplayEvent> events = header.events;
			ReplayEvent replayEvent = new ReplayEvent();
			replayEvent.time = p_time;
			if (m_channel_time != null)
			{
				replayEvent.sample = m_channel_time.sample;
			}
			replayEvent.typeFlag = p_type;
			replayEvent.position = p_position;
			if (p_data != null && p_data.Length != 0 && p_data[0] != null)
			{
				replayEvent.data = p_data;
			}
			events.Add(replayEvent);
			header.events = events;
		}

		public void PushEvent(ReplayEventType p_type, float p_time, Drone p_drone, params object[] p_data)
		{
			Vector3 position = p_drone.position;
			PushEvent(p_type, p_time, position, p_data);
		}

		protected override void OnClear()
		{
			for (int i = 0; i < channels.Count; i++)
			{
				channels[i].Clear();
			}
			if (header != null)
			{
				header.Clear();
			}
		}

		protected override void OnDestroy()
		{
			for (int i = 0; i < channels.Count; i++)
			{
				channels[i].Destroy();
			}
			if (header != null)
			{
				header.Destroy();
			}
			channels.Clear();
			m_channel_lut.Clear();
			m_channel_time = null;
		}
	}
}
